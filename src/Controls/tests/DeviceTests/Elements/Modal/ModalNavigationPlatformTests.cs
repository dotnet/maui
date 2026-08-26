using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

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
				});

				builder.Services.AddSingleton(factory);
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

			public bool IsReady => true;

			public List<string> Operations { get; } = new();

			public List<(Page Page, bool Animated, Page CurrentPlatformPage)> Pushed { get; } = new();

			public List<(Page Page, bool Animated, Page CurrentPlatformPage)> Popped { get; } = new();

			public int DisposeCount { get; private set; }

			public Task PushModalAsync(Page modal, bool animated)
			{
				Operations.Add("Push");
				Pushed.Add((modal, animated, Host.CurrentPlatformPage));
				return Task.CompletedTask;
			}

			public Task PopModalAsync(Page modal, bool animated)
			{
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
