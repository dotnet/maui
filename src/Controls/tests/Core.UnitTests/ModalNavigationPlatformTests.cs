using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ModalNavigationPlatformTests : BaseTestFixture
	{
		static Window CreateWindow(Action<IServiceProvider> configureServices = null, IElementHandler windowHandler = null)
		{
			var services = Substitute.For<IServiceProvider>();
			configureServices?.Invoke(services);

			var mauiContext = Substitute.For<IMauiContext>();
			mauiContext.Services.Returns(services);

			windowHandler ??= Substitute.For<IElementHandler>();
			windowHandler.MauiContext.Returns(mauiContext);

			var window = new Window
			{
				Handler = windowHandler
			};

			var app = Substitute.For<Element, IApplication>();
			window.Parent = app;

			return window;
		}

		static (Window Window, RecordingModalNavigationPlatform Platform, StubModalNavigationPlatformFactory Factory) CreateWindowWithPlatform(
			Func<IModalNavigationHost, IModalNavigationPlatform> create = null)
		{
			RecordingModalNavigationPlatform platform = null;
			var factory = new StubModalNavigationPlatformFactory(host =>
			{
				if (create is not null)
					return create(host);

				platform = new RecordingModalNavigationPlatform(host);
				return platform;
			});

			var window = CreateWindow(services => RegisterFactory(services, factory));

			return (window, platform, factory);
		}

		static void RegisterFactory(IServiceProvider services, IModalNavigationPlatformFactory factory) =>
			services.GetService(Arg.Is<Type>(t => t == typeof(IModalNavigationPlatformFactory))).Returns(factory);

		static ContentPage AttachRootPage(Window window)
		{
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>() };
			window.Page = page;
			return page;
		}

		static IModalNavigationHost Host(Window window) => window.ModalNavigationManager;

		[Fact]
		public void ModalNavigationSeamInterfacesArePublic()
		{
			// These are the extensibility contract an external platform backend implements. If they are
			// ever re-hidden to internal the PublicAPI baseline could be updated in the same commit and
			// the behavioral tests would still compile (InternalsVisibleTo), so assert visibility here.
			Assert.True(typeof(IModalNavigationPlatform).IsPublic, "IModalNavigationPlatform must be public.");
			Assert.True(typeof(IModalNavigationPlatformFactory).IsPublic, "IModalNavigationPlatformFactory must be public.");
			Assert.True(typeof(IModalNavigationHost).IsPublic, "IModalNavigationHost must be public.");
		}

		[Fact]
		public async Task NoRegistrationUsesBuiltInPlatform()
		{
			var window = CreateWindow();
			AttachRootPage(window);

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			Assert.Same(modal, Assert.Single(window.Navigation.ModalStack));
			Assert.Same(modal, Assert.Single(Host(window).PlatformModalStack));

			var popped = await window.Navigation.PopModalAsync();

			Assert.Same(modal, popped);
			Assert.Empty(window.Navigation.ModalStack);
			Assert.Empty(Host(window).PlatformModalStack);
		}

		[Fact]
		public async Task CustomPlatformResolvedFromDependencyInjection()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);

			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			Assert.Same(modal, Assert.Single(platform.Pushed).Page);
			Assert.Same(modal, Assert.Single(window.Navigation.ModalStack));
		}

		[Fact]
		public void FactoryIsCalledExactlyOncePerWindow()
		{
			var (window, _, factory) = CreateWindowWithPlatform();

			AttachRootPage(window);
			_ = window.Navigation.ModalStack;
			_ = Host(window).IsModalReady;
			_ = Host(window).IsModalReady;

			Assert.Equal(1, factory.CallCount);
		}

		[Fact]
		public async Task EachWindowGetsItsOwnPlatformInstance()
		{
			var factory = new StubModalNavigationPlatformFactory(host => new RecordingModalNavigationPlatform(host));

			var firstWindow = CreateWindow(services => RegisterFactory(services, factory));
			var secondWindow = CreateWindow(services => RegisterFactory(services, factory));

			AttachRootPage(firstWindow);
			AttachRootPage(secondWindow);

			Assert.Equal(2, factory.CallCount);

			var first = (RecordingModalNavigationPlatform)factory.Created[0];
			var second = (RecordingModalNavigationPlatform)factory.Created[1];

			Assert.NotSame(first, second);
			Assert.Same(firstWindow, first.Host.Window);
			Assert.Same(secondWindow, second.Host.Window);

			var modal = new ContentPage();
			await firstWindow.Navigation.PushModalAsync(modal);

			Assert.Single(first.Pushed);
			Assert.Empty(second.Pushed);
			Assert.Empty(second.Host.PlatformModalStack);
		}

		[Fact]
		public async Task PushAndPopArriveInStackOrder()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			var first = new ContentPage();
			var second = new ContentPage();

			await window.Navigation.PushModalAsync(first);
			await window.Navigation.PushModalAsync(second);
			await window.Navigation.PopModalAsync();
			await window.Navigation.PopModalAsync();

			Assert.Equal(
				new[] { "Push:0", "Push:1", "Pop:1", "Pop:0" },
				platform.Operations);

			Assert.Equal(new[] { first, second }, new[] { platform.Pushed[0].Page, platform.Pushed[1].Page });
			Assert.Equal(new[] { second, first }, new[] { platform.Popped[0].Page, platform.Popped[1].Page });
		}

		[Fact]
		public async Task PlatformStackReflectsTheRequestedStateDuringPushAndPop()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			var root = AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			// The modal is already on the platform stack while the push is running, so
			// CurrentPlatformPage points at the page being presented.
			Assert.Same(modal, platform.Pushed[0].CurrentPlatformPage);
			Assert.Equal(new[] { modal }, platform.Pushed[0].PlatformStack);

			await window.Navigation.PopModalAsync();

			// The modal is already off the platform stack while the pop is running, so
			// CurrentPlatformPage points at the page being revealed.
			Assert.Same(root, platform.Popped[0].CurrentPlatformPage);
			Assert.Empty(platform.Popped[0].PlatformStack);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task AnimationFlagIsForwardedToThePlatform(bool animated)
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal, animated);
			Assert.Equal(animated, Assert.Single(platform.Pushed).Animated);

			await window.Navigation.PopModalAsync(animated);
			Assert.Equal(animated, Assert.Single(platform.Popped).Animated);
		}

		[Fact]
		public async Task DeferredPushUsesTheAnimationFlagFromTheOriginalRequest()
		{
			RecordingModalNavigationPlatform platform = null;
			var factory = new StubModalNavigationPlatformFactory(host =>
			{
				platform = new RecordingModalNavigationPlatform(host) { IsReady = false };
				return platform;
			});

			var window = CreateWindow(services => RegisterFactory(services, factory));
			AttachRootPage(window);

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal, animated: false);

			// Not ready: the cross-platform stack records the push but the platform is untouched.
			Assert.Empty(platform.Pushed);
			Assert.Same(modal, Assert.Single(window.Navigation.ModalStack));
			Assert.Empty(Host(window).PlatformModalStack);

			platform.IsReady = true;
			platform.Host.RequestSync();

			Assert.False(Assert.Single(platform.Pushed).Animated);
			Assert.Same(modal, platform.Pushed[0].Page);
			Assert.Same(modal, Assert.Single(Host(window).PlatformModalStack));
		}

		[Fact]
		public async Task PlatformIsNotInvokedWhenPopIsCanceled()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			window.ModalPopping += (_, e) => e.Cancel = true;

			var popped = await window.Navigation.PopModalAsync();

			Assert.Null(popped);
			Assert.Empty(platform.Popped);
			Assert.Same(modal, Assert.Single(window.Navigation.ModalStack));
			Assert.Same(modal, Assert.Single(Host(window).PlatformModalStack));
		}

		[Fact]
		public async Task FactoryReturningNullFallsBackToTheBuiltInPlatform()
		{
			var factory = new StubModalNavigationPlatformFactory(_ => null);
			var window = CreateWindow(services => RegisterFactory(services, factory));
			AttachRootPage(window);

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			Assert.Equal(1, factory.CallCount);
			Assert.Same(modal, Assert.Single(Host(window).PlatformModalStack));

			var popped = await window.Navigation.PopModalAsync();

			Assert.Same(modal, popped);
			Assert.Empty(Host(window).PlatformModalStack);
		}

		[Fact]
		public async Task FailedPushRollsBackThePlatformStack()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];
			platform.PushBehavior = (_, _) => throw new InvalidOperationException("boom");

			var modal = new ContentPage();

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(
				() => window.Navigation.PushModalAsync(modal));

			Assert.Equal("boom", exception.Message);
			Assert.Empty(Host(window).PlatformModalStack);
		}

		[Fact]
		public void PlatformIsDisposedWhenTheWindowIsDestroyed()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			((IWindow)window).Destroying();

			Assert.Equal(1, platform.DisposeCount);
		}

		[Fact]
		public void PlatformIsDisposedAndRecreatedWhenTheWindowHandlerChanges()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var first = (RecordingModalNavigationPlatform)factory.Created[0];

			var secondHandler = Substitute.For<IElementHandler>();
			var secondContext = Substitute.For<IMauiContext>();
			var secondServices = Substitute.For<IServiceProvider>();
			RegisterFactory(secondServices, factory);
			secondContext.Services.Returns(secondServices);
			secondHandler.MauiContext.Returns(secondContext);

			window.Handler = secondHandler;

			Assert.Equal(1, first.DisposeCount);

			// The next access resolves a fresh instance from the new scope.
			_ = Host(window).IsModalReady;

			Assert.Equal(2, factory.CallCount);
			Assert.NotSame(first, factory.Created[1]);
		}

		[Fact]
		public void PageAttachedIsCalledWhenTheWindowPageGetsAHandler()
		{
			var (window, _, factory) = CreateWindowWithPlatform();

			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			Assert.Equal(1, platform.PageAttachedCount);

			AttachRootPage(window);

			Assert.Equal(2, platform.PageAttachedCount);
		}

		[Fact]
		public async Task HostExposesTheFrameworkNavigationState()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			var root = AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];
			var host = platform.Host;

			Assert.Same(window, host.Window);
			Assert.Same(window.Handler.MauiContext, host.MauiContext);
			Assert.Same(root, host.CurrentPage);
			Assert.Same(root, host.CurrentPlatformPage);
			Assert.True(host.IsModalReady);
			Assert.False(host.IsBatchPopping);

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			Assert.Same(modal, host.CurrentPage);
			Assert.Same(modal, host.CurrentPlatformPage);
			Assert.Equal(new[] { modal }, host.PlatformModalStack);
		}

		[Fact]
		public async Task PushIsAppliedOnceTheWindowPageGetsAHandler()
		{
			var (window, _, factory) = CreateWindowWithPlatform();

			// No page yet, so the framework is not ready even though the platform is.
			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			Assert.Same(modal, Assert.Single(window.Navigation.ModalStack));
			Assert.Empty(Host(window).PlatformModalStack);

			AttachRootPage(window);

			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			Assert.Same(modal, Assert.Single(platform.Pushed).Page);
			Assert.Same(modal, Assert.Single(Host(window).PlatformModalStack));
		}

		[Fact]
		public void ResolutionIsDeferredUntilTheWindowHasAServiceScope()
		{
			var factory = new StubModalNavigationPlatformFactory(host => new RecordingModalNavigationPlatform(host));

			var services = Substitute.For<IServiceProvider>();
			RegisterFactory(services, factory);

			var mauiContext = Substitute.For<IMauiContext>();
			mauiContext.Services.Returns(services);

			var handler = Substitute.For<IElementHandler>();
			handler.MauiContext.Returns(mauiContext);

			// No handler yet, so there is no MauiContext and no service scope to resolve from. The
			// resolution must not latch on to "nothing registered" while the window is still bare.
			var window = new Window();
			_ = window.Navigation.ModalStack;
			_ = Host(window).IsModalReady;

			Assert.Equal(0, factory.CallCount);

			window.Handler = handler;
			AttachRootPage(window);

			Assert.Equal(1, factory.CallCount);
		}

		sealed class StubModalNavigationPlatformFactory : IModalNavigationPlatformFactory
		{
			readonly Func<IModalNavigationHost, IModalNavigationPlatform> _create;

			public StubModalNavigationPlatformFactory(Func<IModalNavigationHost, IModalNavigationPlatform> create)
			{
				_create = create;
			}

			public int CallCount { get; private set; }

			public List<IModalNavigationPlatform> Created { get; } = new();

			public IModalNavigationPlatform CreateModalNavigationPlatform(IModalNavigationHost host)
			{
				CallCount++;
				var platform = _create(host);
				if (platform is not null)
					Created.Add(platform);

				return platform;
			}
		}

		sealed record ModalOperation(Page Page, bool Animated, Page CurrentPlatformPage, IReadOnlyList<Page> PlatformStack);

		sealed class RecordingModalNavigationPlatform : IModalNavigationPlatform
		{
			public RecordingModalNavigationPlatform(IModalNavigationHost host)
			{
				Host = host;
			}

			public IModalNavigationHost Host { get; set; }

			public bool IsReady { get; set; } = true;

			public List<string> Operations { get; } = new();

			public List<ModalOperation> Pushed { get; } = new();

			public List<ModalOperation> Popped { get; } = new();

			public int PageAttachedCount { get; private set; }

			public int DisposeCount { get; private set; }

			public Action<Page, bool> PushBehavior { get; set; }

			public Task PushModalAsync(Page modal, bool animated)
			{
				PushBehavior?.Invoke(modal, animated);

				Operations.Add($"Push:{Pushed.Count}");
				Pushed.Add(new ModalOperation(modal, animated, Host.CurrentPlatformPage, new List<Page>(Host.PlatformModalStack)));

				return Task.CompletedTask;
			}

			public Task PopModalAsync(Page modal, bool animated)
			{
				Operations.Add($"Pop:{Pushed.Count - Popped.Count - 1}");
				Popped.Add(new ModalOperation(modal, animated, Host.CurrentPlatformPage, new List<Page>(Host.PlatformModalStack)));

				return Task.CompletedTask;
			}

			public void PageAttached() => PageAttachedCount++;

			public void Dispose() => DisposeCount++;
		}
	}
}
