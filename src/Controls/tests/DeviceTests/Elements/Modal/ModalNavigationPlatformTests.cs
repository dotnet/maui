using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Modal)]
#if ANDROID || IOS || MACCATALYST
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
#endif
	public class ModalNavigationPlatformTests : ControlsHandlerTestBase
	{
		void SetupBuilder(IModalNavigationPlatformFactory factory)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Window, WindowHandlerStub>();
					handlers.AddHandler<Label, LabelHandler>();
				});

				builder.Services.AddSingleton<IModalNavigationPlatformFactory>(factory);
			});
		}

		static IReadOnlyList<Page> PlatformModalStack(Window window) =>
			((IModalNavigationHost)window.ModalNavigationManager).PlatformModalStack;

		[Fact]
		public async Task RegisteredPlatformReceivesPushAndPop()
		{
			var factory = new RecordingModalNavigationPlatformFactory();
			SetupBuilder(factory);

			var windowPage = new ContentPage { Content = new Label { Text = "Root" } };
			var modalPage = new ContentPage { Content = new Label { Text = "Modal" } };
			var window = new Window(windowPage);

			int modalAppearing = 0;
			int modalDisappearing = 0;
			modalPage.Appearing += (_, _) => modalAppearing++;
			modalPage.Disappearing += (_, _) => modalDisappearing++;

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async _ =>
			{
				await windowPage.Navigation.PushModalAsync(modalPage);

				Assert.Same(modalPage, Assert.Single(window.Navigation.ModalStack));

				await windowPage.Navigation.PopModalAsync();
			});

			var platform = factory.Latest;

			Assert.Same(modalPage, Assert.Single(platform.Pushed).Page);
			Assert.Same(modalPage, Assert.Single(platform.Popped).Page);
			Assert.Equal(new[] { "Push", "Pop" }, platform.Operations);

			// The framework still owns the cross-platform stack and the page lifecycle.
			Assert.Empty(window.Navigation.ModalStack);
			Assert.Equal(1, modalAppearing);
			Assert.Equal(1, modalDisappearing);
		}

		[Fact]
		public async Task RegisteredPlatformSuppressesTheBuiltInPresentation()
		{
			var factory = new RecordingModalNavigationPlatformFactory();
			SetupBuilder(factory);

			var windowPage = new ContentPage { Content = new Label { Text = "Root" } };
			var modalPage = new ContentPage { Content = new Label { Text = "Modal" } };
			var window = new Window(windowPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async _ =>
			{
				await windowPage.Navigation.PushModalAsync(modalPage);

				// The framework tracks the modal as presented, but the recording platform does no
				// platform work, which proves the built-in presentation path did not run.
				Assert.Same(modalPage, Assert.Single(PlatformModalStack(window)));
				Assert.Null(modalPage.Handler);

				await windowPage.Navigation.PopModalAsync();

				Assert.Empty(PlatformModalStack(window));
			});

			Assert.Single(factory.Latest.Pushed);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task AnimationFlagReachesTheRegisteredPlatform(bool animated)
		{
			var factory = new RecordingModalNavigationPlatformFactory();
			SetupBuilder(factory);

			var windowPage = new ContentPage { Content = new Label { Text = "Root" } };
			var modalPage = new ContentPage { Content = new Label { Text = "Modal" } };
			var window = new Window(windowPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async _ =>
			{
				await windowPage.Navigation.PushModalAsync(modalPage, animated);
				await windowPage.Navigation.PopModalAsync(animated);
			});

			var platform = factory.Latest;

			Assert.Equal(animated, Assert.Single(platform.Pushed).Animated);
			Assert.Equal(animated, Assert.Single(platform.Popped).Animated);
		}

		[Fact]
		public async Task RegisteredPlatformSeesTheHostStackWhilePresenting()
		{
			var factory = new RecordingModalNavigationPlatformFactory();
			SetupBuilder(factory);

			var windowPage = new ContentPage { Content = new Label { Text = "Root" } };
			var first = new ContentPage { Content = new Label { Text = "First" } };
			var second = new ContentPage { Content = new Label { Text = "Second" } };
			var window = new Window(windowPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async _ =>
			{
				await windowPage.Navigation.PushModalAsync(first);
				await windowPage.Navigation.PushModalAsync(second);
				await windowPage.Navigation.PopModalAsync();
				await windowPage.Navigation.PopModalAsync();
			});

			var platform = factory.Latest;

			Assert.Equal(new[] { "Push", "Push", "Pop", "Pop" }, platform.Operations);
			Assert.Equal(new[] { first, second }, new[] { platform.Pushed[0].Page, platform.Pushed[1].Page });
			Assert.Equal(new[] { second, first }, new[] { platform.Popped[0].Page, platform.Popped[1].Page });

			// While presenting, the page being pushed is already the top of the platform stack.
			Assert.Same(first, platform.Pushed[0].CurrentPlatformPage);
			Assert.Same(second, platform.Pushed[1].CurrentPlatformPage);

			// While dismissing, the page being revealed is already the top of the platform stack.
			Assert.Same(first, platform.Popped[0].CurrentPlatformPage);
			Assert.Same(windowPage, platform.Popped[1].CurrentPlatformPage);

			Assert.Same(window, platform.Host.Window);
		}

		[Fact]
		public async Task FailedPopLeavesTheModalOnThePlatformStack()
		{
			var factory = new RecordingModalNavigationPlatformFactory();
			SetupBuilder(factory);

			var windowPage = new ContentPage { Content = new Label { Text = "Root" } };
			var modalPage = new ContentPage { Content = new Label { Text = "Modal" } };
			var window = new Window(windowPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async _ =>
			{
				await windowPage.Navigation.PushModalAsync(modalPage);

				factory.Latest.PopBehavior = (_, _) => throw new InvalidOperationException("pop boom");

				await Assert.ThrowsAsync<InvalidOperationException>(
					() => windowPage.Navigation.PopModalAsync());

				// A failed dismissal means the modal is presumed still visible, so it has to stay on the
				// platform stack — otherwise it would be absent from both stacks and unreachable.
				Assert.Same(modalPage, Assert.Single(PlatformModalStack(window)));
				Assert.Empty(window.Navigation.ModalStack);

				// And the next reconciliation pass converges once the backend recovers.
				factory.Latest.PopBehavior = null;
				factory.Latest.Host.RequestSync();

				await AssertEventually(() => PlatformModalStack(window).Count == 0);
			});
		}

		[Fact]
		public async Task DeferredPopPreservesTheRequestedAnimationFlag()
		{
			var factory = new RecordingModalNavigationPlatformFactory();
			SetupBuilder(factory);

			var windowPage = new ContentPage { Content = new Label { Text = "Root" } };
			var modalPage = new ContentPage { Content = new Label { Text = "Modal" } };
			var window = new Window(windowPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async _ =>
			{
				var platform = factory.Latest;

				// Push unanimated so a leaked push flag can't masquerade as the pop flag.
				await windowPage.Navigation.PushModalAsync(modalPage, false);

				platform.IsReadyValue = false;
				await windowPage.Navigation.PopModalAsync(true);

				Assert.Empty(platform.Popped);

				platform.IsReadyValue = true;
				platform.Host.RequestSync();

				await AssertEventually(() => platform.Popped.Count == 1);

				Assert.True(platform.Popped[0].Animated);
			});
		}

		[Fact]
		public async Task TeardownWithPresentedModalsOnlyDisposes()
		{
			var factory = new RecordingModalNavigationPlatformFactory();
			SetupBuilder(factory);

			var windowPage = new ContentPage { Content = new Label { Text = "Root" } };
			var window = new Window(windowPage);

			RecordingModalNavigationPlatform platform = null;

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async _ =>
			{
				await windowPage.Navigation.PushModalAsync(new ContentPage());
				platform = factory.Latest;
				Assert.Single(PlatformModalStack(window));
			});

			// Documented contract: teardown does not pop still-presented modals, Dispose owns that.
			Assert.Empty(platform.Popped);
			Assert.Equal(1, platform.DisposeCount);
		}

		[Fact]
		public async Task FailedPopRetryPreservesTheRequestedAnimationFlag()
		{
			var factory = new RecordingModalNavigationPlatformFactory();
			SetupBuilder(factory);

			var windowPage = new ContentPage { Content = new Label { Text = "Root" } };
			var modalPage = new ContentPage { Content = new Label { Text = "Modal" } };
			var window = new Window(windowPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async _ =>
			{
				var platform = factory.Latest;

				// Push unanimated so a leaked push flag cannot masquerade as the pop flag.
				await windowPage.Navigation.PushModalAsync(modalPage, false);

				// The inline pop fails. The animation intent has to survive the failure, otherwise the
				// retry silently downgrades to unanimated.
				platform.PopBehavior = (_, _) => throw new InvalidOperationException("pop boom");
				await Assert.ThrowsAsync<InvalidOperationException>(
					() => windowPage.Navigation.PopModalAsync(true));

				Assert.Empty(platform.Popped);
				Assert.Same(modalPage, Assert.Single(PlatformModalStack(window)));

				platform.PopBehavior = null;
				platform.Host.RequestSync();

				await AssertEventually(() => platform.Popped.Count == 1);

				Assert.True(platform.Popped[0].Animated);
			});
		}

		sealed class RecordingModalNavigationPlatformFactory : IModalNavigationPlatformFactory
		{
			public List<RecordingModalNavigationPlatform> Created { get; } = new();

			public RecordingModalNavigationPlatform Latest
			{
				get
				{
					Assert.NotEmpty(Created);
					return Created[Created.Count - 1];
				}
			}

			public IModalNavigationPlatform CreateModalNavigationPlatform(IModalNavigationHost host)
			{
				var platform = new RecordingModalNavigationPlatform(host);
				Created.Add(platform);
				return platform;
			}
		}

		sealed class RecordingModalNavigationPlatform : IModalNavigationPlatform
		{
			public RecordingModalNavigationPlatform(IModalNavigationHost host)
			{
				Host = host;
			}

			public IModalNavigationHost Host { get; }

			public bool IsReadyValue { get; set; } = true;

			public bool IsReady => IsReadyValue && Host.IsWindowReady;

			public List<string> Operations { get; } = new();

			public List<(Page Page, bool Animated, Page CurrentPlatformPage)> Pushed { get; } = new();

			public List<(Page Page, bool Animated, Page CurrentPlatformPage)> Popped { get; } = new();

			public int DisposeCount { get; private set; }

			public Action<Page, bool> PopBehavior { get; set; }

			public Task PushModalAsync(Page modal, bool animated)
			{
				Operations.Add("Push");
				Pushed.Add((modal, animated, Host.CurrentPlatformPage));
				return Task.CompletedTask;
			}

			public Task PopModalAsync(Page modal, bool animated)
			{
				PopBehavior?.Invoke(modal, animated);

				Operations.Add("Pop");
				Popped.Add((modal, animated, Host.CurrentPlatformPage));
				return Task.CompletedTask;
			}

			public void PageAttached()
			{
			}

			public void Dispose() => DisposeCount++;
		}
	}
}
