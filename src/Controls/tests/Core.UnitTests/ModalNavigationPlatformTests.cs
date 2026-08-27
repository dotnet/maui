using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
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

		// Builds a fresh handler backed by its own service scope, which is what an Android activity
		// recreation does.
		static IElementHandler CreateHandler(IModalNavigationPlatformFactory factory)
		{
			var services = Substitute.For<IServiceProvider>();
			if (factory is not null)
				RegisterFactory(services, factory);

			var mauiContext = Substitute.For<IMauiContext>();
			mauiContext.Services.Returns(services);

			var handler = Substitute.For<IElementHandler>();
			handler.MauiContext.Returns(mauiContext);
			return handler;
		}

		static ContentPage AttachRootPage(Window window)
		{
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>() };
			window.Page = page;
			return page;
		}

		static IModalNavigationHost Host(Window window) => window.ModalNavigationManager;

		// Forces the lazy factory resolution through the same entry point the framework uses when the
		// window's page handler is attached.
		static void ForceResolvePlatform(Window window) => window.ModalNavigationManager.PageAttachedHandler();

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
		public async Task BuiltInPlatformMaintainsTheSameStackOrderingAsTheSeam()
		{
			// The fallback path applies the same stack bookkeeping the seam promises: the modal is on
			// PlatformModalStack for the duration of a push, and off it for the duration of a pop.
			var window = CreateWindow();
			var root = AttachRootPage(window);
			var host = Host(window);

			var first = new ContentPage();
			var second = new ContentPage();

			await window.Navigation.PushModalAsync(first);
			Assert.Equal(new[] { first }, host.PlatformModalStack);
			Assert.Same(first, host.CurrentPlatformPage);

			await window.Navigation.PushModalAsync(second);
			Assert.Equal(new[] { first, second }, host.PlatformModalStack);
			Assert.Same(second, host.CurrentPlatformPage);

			await window.Navigation.PopModalAsync();
			Assert.Equal(new[] { first }, host.PlatformModalStack);
			Assert.Same(first, host.CurrentPlatformPage);

			await window.Navigation.PopModalAsync();
			Assert.Empty(host.PlatformModalStack);
			Assert.Same(root, host.CurrentPlatformPage);
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
			ForceResolvePlatform(window);
			ForceResolvePlatform(window);

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
				platform = new RecordingModalNavigationPlatform(host) { IsReadyValue = false };
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

			platform.IsReadyValue = true;
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

			// Nothing was presented, so the platform stack must not claim it was. The requested stack
			// keeps the page so a later reconciliation pass can retry.
			Assert.Empty(Host(window).PlatformModalStack);
			Assert.Same(modal, Assert.Single(window.Navigation.ModalStack));
		}

		[Fact]
		public async Task FailedPopRestoresThePlatformStack()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			platform.PopBehavior = (_, _) => throw new InvalidOperationException("pop boom");

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(
				() => window.Navigation.PopModalAsync());

			Assert.Equal("pop boom", exception.Message);

			// The dismissal failed so the modal is presumed to still be on screen. If the platform stack
			// dropped it here it would be absent from BOTH stacks and unreachable forever.
			Assert.Same(modal, Assert.Single(Host(window).PlatformModalStack));
			Assert.Empty(window.Navigation.ModalStack);
		}

		[Fact]
		public async Task FailedPopRetryPreservesTheRequestedAnimationFlag()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			// Push unanimated so a leaked push flag cannot masquerade as the pop flag.
			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal, animated: false);

			// The pop is applied INLINE (the platform is ready), and fails. The animation intent must
			// survive the failure, otherwise the retry silently downgrades to unanimated.
			platform.PopBehavior = (_, _) => throw new InvalidOperationException("pop boom");
			await Assert.ThrowsAsync<InvalidOperationException>(() => window.Navigation.PopModalAsync(animated: true));

			Assert.Empty(platform.Popped);

			platform.PopBehavior = null;
			platform.Host.RequestSync();

			Assert.True(Assert.Single(platform.Popped).Animated);
			Assert.Empty(Host(window).PlatformModalStack);
		}

		[Fact]
		public async Task FailedPopIsRetriedByTheNextSync()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			platform.PopBehavior = (_, _) => throw new InvalidOperationException("pop boom");
			await Assert.ThrowsAsync<InvalidOperationException>(() => window.Navigation.PopModalAsync());

			// Recovery: with the modal restored on the platform stack and gone from the requested stack,
			// reconciliation converges by dismissing it again.
			platform.PopBehavior = null;
			platform.Host.RequestSync();

			Assert.Same(modal, Assert.Single(platform.Popped).Page);
			Assert.Empty(Host(window).PlatformModalStack);
			Assert.Empty(window.Navigation.ModalStack);
		}

		[Fact]
		public async Task DelayedPopFailureAfterTeardownDoesNotRestorePlatformState()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			var failure = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			platform.PopTaskBehavior = (_, _) => failure.Task;

			var popTask = window.Navigation.PopModalAsync();
			Assert.Empty(Host(window).PlatformModalStack);

			((IWindow)window).Destroying();
			failure.SetException(new InvalidOperationException("pop boom"));

			await Assert.ThrowsAsync<InvalidOperationException>(() => popTask);
			Assert.Empty(Host(window).PlatformModalStack);
		}

		[Fact]
		public async Task DelayedPushFailureFromReplacedHandlerDoesNotRollbackNewPlatformState()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var first = (RecordingModalNavigationPlatform)factory.Created[0];

			var failure = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			first.PushTaskBehavior = (_, _) => failure.Task;

			var modal = new ContentPage();
			var pushTask = window.Navigation.PushModalAsync(modal);
			Assert.Same(modal, Assert.Single(Host(window).PlatformModalStack));

			window.Handler = CreateHandler(factory);
			ForceResolvePlatform(window);

			// The replacement backend has established the same modal as its current platform state
			// while the outgoing backend's push is still completing.
			var replacementStack = Assert.IsType<List<Page>>(Host(window).PlatformModalStack);
			replacementStack.Add(modal);
			Assert.Same(modal, Assert.Single(Host(window).PlatformModalStack));

			failure.SetException(new InvalidOperationException("push boom"));

			await Assert.ThrowsAsync<InvalidOperationException>(() => pushTask);
			Assert.Same(modal, Assert.Single(Host(window).PlatformModalStack));
		}

		[Fact]
		public async Task RequestSyncQueuedBehindStaleFailureReconcilesReplacementPlatform()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var first = (RecordingModalNavigationPlatform)factory.Created[0];

			first.IsReadyValue = false;
			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			var failure = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			first.PushTaskBehavior = (_, _) => failure.Task;
			first.IsReadyValue = true;
			first.Host.RequestSync();

			Assert.Same(modal, Assert.Single(Host(window).PlatformModalStack));
			Assert.Empty(first.Pushed);

			window.Handler = CreateHandler(factory);
			ForceResolvePlatform(window);
			var second = (RecordingModalNavigationPlatform)factory.Created[1];
			var secondPushed = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			second.PushBehavior = (_, _) => secondPushed.TrySetResult(null);

			second.Host.RequestSync();
			failure.SetException(new InvalidOperationException("push boom"));

			await secondPushed.Task.WaitAsync(TimeSpan.FromSeconds(5));
			Assert.Same(modal, Assert.Single(second.Pushed).Page);
			Assert.Same(modal, Assert.Single(Host(window).PlatformModalStack));
		}

		[Fact]
		public async Task DeferredPopPreservesTheRequestedAnimationFlag()
		{
			RecordingModalNavigationPlatform platform = null;
			var factory = new StubModalNavigationPlatformFactory(host =>
			{
				platform = new RecordingModalNavigationPlatform(host);
				return platform;
			});

			var window = CreateWindow(services => RegisterFactory(services, factory));
			AttachRootPage(window);

			var modal = new ContentPage();

			// Push without animation so a leaked push flag can't masquerade as the pop flag.
			await window.Navigation.PushModalAsync(modal, animated: false);

			// Go not-ready so the pop has to be deferred. The pop request is taken off the logical stack
			// immediately, so the animation flag has to be remembered separately.
			platform.IsReadyValue = false;
			await window.Navigation.PopModalAsync(animated: true);

			Assert.Empty(platform.Popped);

			platform.IsReadyValue = true;
			platform.Host.RequestSync();

			Assert.True(Assert.Single(platform.Popped).Animated);
			Assert.Empty(Host(window).PlatformModalStack);
		}

		[Fact]
		public async Task DeferredPopWithoutAnimationStaysUnanimated()
		{
			RecordingModalNavigationPlatform platform = null;
			var factory = new StubModalNavigationPlatformFactory(host =>
			{
				platform = new RecordingModalNavigationPlatform(host);
				return platform;
			});

			var window = CreateWindow(services => RegisterFactory(services, factory));
			AttachRootPage(window);

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal, animated: true);

			platform.IsReadyValue = false;
			await window.Navigation.PopModalAsync(animated: false);

			platform.IsReadyValue = true;
			platform.Host.RequestSync();

			// The push was animated; the deferred pop must not inherit that.
			Assert.False(Assert.Single(platform.Popped).Animated);
		}

		[Fact]
		public void PlatformCanConsultWindowReadinessFromIsReadyWithoutRecursing()
		{
			// IModalNavigationHost.IsWindowReady must not fold IModalNavigationPlatform.IsReady back in.
			// The natural implementation below would otherwise recurse into an uncatchable
			// StackOverflowException that no test could observe as a failure.
			SelfConsultingModalNavigationPlatform platform = null;
			var factory = new StubModalNavigationPlatformFactory(host =>
			{
				platform = new SelfConsultingModalNavigationPlatform(host);
				return platform;
			});

			var window = CreateWindow(services => RegisterFactory(services, factory));
			var page = AttachRootPage(window);

			Assert.True(platform.IsReady);
			Assert.True(Host(window).IsWindowReady);

			// And it tracks the framework state rather than being hardcoded.
			window.Page = new ContentPage();

			Assert.False(Host(window).IsWindowReady);
			Assert.False(platform.IsReady);
		}

		[Fact]
		public async Task PlatformInitiatedDismissalRoundTripsThroughNavigation()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			int modalPopped = 0;
			window.ModalPopped += (_, _) => modalPopped++;

			// The backend dismissed the modal natively and now tells the framework about it. Its
			// PopModalAsync is still called for the page, so it has to tolerate an already-gone modal.
			bool alreadyDismissed = true;
			platform.PopBehavior = (_, _) => Assert.True(alreadyDismissed);

			await window.Navigation.PopModalAsync(animated: false);

			Assert.Same(modal, Assert.Single(platform.Popped).Page);
			Assert.Equal(1, modalPopped);
			Assert.Empty(window.Navigation.ModalStack);
			Assert.Empty(Host(window).PlatformModalStack);
		}

		// Window.Dispatcher is captured from the creating thread in the BindableObject constructor, so
		// these tests drive the thread's dispatcher stub rather than registering an IDispatcher in DI.
		// That is the point of the change: RequestSync must not depend on the window handler's scope.
		static IDisposable UseThreadDispatcher(Func<bool> isDispatchRequired, Action<Action> dispatch)
		{
			DispatcherProviderStubOptions.IsInvokeRequired = isDispatchRequired;
			DispatcherProviderStubOptions.InvokeOnMainThread = dispatch;

			return new ActionDisposable(() =>
			{
				DispatcherProviderStubOptions.IsInvokeRequired = null;
				DispatcherProviderStubOptions.InvokeOnMainThread = null;
			});
		}

		sealed class ActionDisposable : IDisposable
		{
			Action _action;

			public ActionDisposable(Action action) => _action = action;

			public void Dispose()
			{
				_action?.Invoke();
				_action = null;
			}
		}

		[Fact]
		public void RequestSyncFromABackgroundThreadIsMarshalledToTheUIThread()
		{
			bool dispatchRequired = false;
			var queue = new List<Action>();

			using var _ = UseThreadDispatcher(() => dispatchRequired, queue.Add);

			RecordingModalNavigationPlatform platform = null;
			var factory = new StubModalNavigationPlatformFactory(host =>
			{
				platform = new RecordingModalNavigationPlatform(host);
				return platform;
			});

			var window = CreateWindow(services => RegisterFactory(services, factory));
			AttachRootPage(window);

			platform.IsReadyValue = false;
			var modal = new ContentPage();
			window.Navigation.PushModalAsync(modal).Wait();

			Assert.Empty(platform.Pushed);

			platform.IsReadyValue = true;
			platform.IsReadyReadThreadIds.Clear();

			// Pretend the caller is on a background thread. The whole reconciliation entry — readiness
			// checks, lifecycle events and the platform call — has to run through the dispatcher.
			dispatchRequired = true;
			platform.Host.RequestSync();

			// Nothing ran inline; it was queued instead.
			Assert.Empty(platform.Pushed);
			Assert.Empty(platform.IsReadyReadThreadIds);
			Assert.Single(queue);

			dispatchRequired = false;
			foreach (var action in queue.ToArray())
				action();

			Assert.Same(modal, Assert.Single(platform.Pushed).Page);
		}

		[Fact]
		public void RequestSyncOnTheUIThreadRunsInline()
		{
			var queue = new List<Action>();

			using var _ = UseThreadDispatcher(() => false, queue.Add);

			RecordingModalNavigationPlatform platform = null;
			var factory = new StubModalNavigationPlatformFactory(host =>
			{
				platform = new RecordingModalNavigationPlatform(host);
				return platform;
			});

			var window = CreateWindow(services => RegisterFactory(services, factory));
			AttachRootPage(window);

			platform.IsReadyValue = false;
			var modal = new ContentPage();
			window.Navigation.PushModalAsync(modal).Wait();

			platform.IsReadyValue = true;
			platform.Host.RequestSync();

			Assert.Empty(queue);
			Assert.Same(modal, Assert.Single(platform.Pushed).Page);
		}

		[Fact]
		public void RequestSyncStillMarshalsWhenTheWindowHandlerIsNull()
		{
			bool dispatchRequired = false;
			var queue = new List<Action>();

			using var _ = UseThreadDispatcher(() => dispatchRequired, queue.Add);

			RecordingModalNavigationPlatform platform = null;
			var factory = new StubModalNavigationPlatformFactory(host =>
			{
				platform = new RecordingModalNavigationPlatform(host);
				return platform;
			});

			var window = CreateWindow(services => RegisterFactory(services, factory));
			AttachRootPage(window);

			// Handler teardown takes the handler's MauiContext — and the IDispatcher registered in its
			// scope — with it. Resolving the dispatcher from the handler would silently degrade the
			// documented any-thread guarantee to inline execution exactly when it matters.
			window.Handler = null;

			dispatchRequired = true;
			platform.Host.RequestSync();

			Assert.Single(queue);
		}

		[Fact]
		public void RequestSyncQueuedBeforeDestroyIsDroppedAfterTeardown()
		{
			bool dispatchRequired = false;
			var queue = new List<Action>();

			using var _ = UseThreadDispatcher(() => dispatchRequired, queue.Add);

			RecordingModalNavigationPlatform platform = null;
			var factory = new StubModalNavigationPlatformFactory(host =>
			{
				platform = new RecordingModalNavigationPlatform(host);
				return platform;
			});

			var window = CreateWindow(services => RegisterFactory(services, factory));
			AttachRootPage(window);

			platform.IsReadyValue = false;
			var modal = new ContentPage();
			window.Navigation.PushModalAsync(modal).Wait();

			platform.IsReadyValue = true;

			// Queue the sync from a "background thread", then let teardown overtake it.
			dispatchRequired = true;
			platform.Host.RequestSync();
			Assert.Single(queue);

			((IWindow)window).Destroying();

			dispatchRequired = false;
			foreach (var action in queue.ToArray())
				action();

			// The stale callback must not repopulate modal state on a torn-down window or drive
			// presentation through a disposed scope.
			Assert.Empty(platform.Pushed);
			Assert.Empty(Host(window).PlatformModalStack);
		}

		[Fact]
		public void RequestSyncQueuedBeforeAHandlerSwapIsDropped()
		{
			bool dispatchRequired = false;
			var queue = new List<Action>();

			using var _ = UseThreadDispatcher(() => dispatchRequired, queue.Add);

			var factory = new StubModalNavigationPlatformFactory(host => new RecordingModalNavigationPlatform(host));

			var window = CreateWindow(services => RegisterFactory(services, factory));
			AttachRootPage(window);
			var first = (RecordingModalNavigationPlatform)factory.Created[0];

			first.IsReadyValue = false;
			window.Navigation.PushModalAsync(new ContentPage()).Wait();
			first.IsReadyValue = true;

			dispatchRequired = true;
			first.Host.RequestSync();
			Assert.Single(queue);

			// The handler (and therefore the whole service scope) is replaced before the callback runs.
			window.Handler = CreateHandler(factory);

			dispatchRequired = false;
			foreach (var action in queue.ToArray())
				action();

			// The queued callback belonged to the previous scope, so the disposed platform must not be
			// driven by it.
			Assert.Empty(first.Pushed);
		}

		[Fact]
		public void HandlerChangeDoesNotResolveFromTheOutgoingScope()
		{
			var oldFactory = new StubModalNavigationPlatformFactory(host => new RecordingModalNavigationPlatform(host));
			var newFactory = new StubModalNavigationPlatformFactory(host => new RecordingModalNavigationPlatform(host));

			var window = CreateWindow(services => RegisterFactory(services, oldFactory));
			AttachRootPage(window);

			Assert.Equal(1, oldFactory.CallCount);

			int oldCallsDuringTransition = -1;
			window.HandlerChanging += (_, _) =>
			{
				// During HandlerChanging, Window.Handler still points at the OUTGOING handler. A reentrant
				// resolution here must not latch an override built from the scope that is going away.
				ForceResolvePlatform(window);
				oldCallsDuringTransition = oldFactory.CallCount;
			};

			window.Handler = CreateHandler(newFactory);
			ForceResolvePlatform(window);

			Assert.Equal(1, oldCallsDuringTransition);
			Assert.Equal(1, oldFactory.CallCount);

			// Resolution happened once the new handler was actually installed, against the NEW scope.
			Assert.Equal(1, newFactory.CallCount);
		}

		[Fact]
		public void ThrowingFactoryFallsBackToTheBuiltInPlatformWithoutRetrying()
		{
			var factory = new StubModalNavigationPlatformFactory(_ => throw new InvalidOperationException("factory boom"));
			var window = CreateWindow(services => RegisterFactory(services, factory));

			// The throw must not surface from whatever navigation code happened to trigger resolution.
			var page = AttachRootPage(window);

			Assert.Equal(1, factory.CallCount);

			// And it must not be retried on every subsequent access.
			ForceResolvePlatform(window);
			ForceResolvePlatform(window);

			Assert.Equal(1, factory.CallCount);
		}

		[Fact]
		public async Task ThrowingFactoryStillAllowsModalNavigation()
		{
			var factory = new StubModalNavigationPlatformFactory(_ => throw new InvalidOperationException("factory boom"));
			var window = CreateWindow(services => RegisterFactory(services, factory));
			AttachRootPage(window);

			var modal = new ContentPage();
			await window.Navigation.PushModalAsync(modal);

			// Fell back to the built-in platform, which tracks the presentation itself.
			Assert.Same(modal, Assert.Single(Host(window).PlatformModalStack));

			var popped = await window.Navigation.PopModalAsync();

			Assert.Same(modal, popped);
			Assert.Empty(Host(window).PlatformModalStack);
		}

		[Fact]
		public void DestroyedWindowDoesNotResurrectThePlatform()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			((IWindow)window).Destroying();

			Assert.Equal(1, platform.DisposeCount);

			// Any lazy access after teardown must not build a new platform against a dying scope.
			ForceResolvePlatform(window);
			ForceResolvePlatform(window);
			_ = Host(window).IsWindowReady;

			Assert.Equal(1, factory.CallCount);
			Assert.Equal(1, platform.DisposeCount);
		}

		[Fact]
		public void DestroyedWindowRecreatesThePlatformOnlyWhenANewHandlerArrives()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var first = (RecordingModalNavigationPlatform)factory.Created[0];

			((IWindow)window).Destroying();
			ForceResolvePlatform(window);

			Assert.Equal(1, factory.CallCount);

			// An Android activity recreation attaches a new handler, which brings a new service scope and
			// is the only thing that lifts the terminal destroyed state.
			var secondHandler = Substitute.For<IElementHandler>();
			var secondContext = Substitute.For<IMauiContext>();
			var secondServices = Substitute.For<IServiceProvider>();
			RegisterFactory(secondServices, factory);
			secondContext.Services.Returns(secondServices);
			secondHandler.MauiContext.Returns(secondContext);

			window.Handler = secondHandler;
			ForceResolvePlatform(window);

			Assert.Equal(2, factory.CallCount);
			Assert.NotSame(first, factory.Created[1]);
		}

		[Fact]
		public async Task TeardownWithPresentedModalsDoesNotCallPopAndOnlyDisposes()
		{
			var (window, _, factory) = CreateWindowWithPlatform();
			AttachRootPage(window);
			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			await window.Navigation.PushModalAsync(new ContentPage());
			await window.Navigation.PushModalAsync(new ContentPage());

			Assert.Equal(2, Host(window).PlatformModalStack.Count);

			((IWindow)window).Destroying();

			// Documented contract: teardown does NOT pop still-presented modals. Dispose is solely
			// responsible for dismissing them.
			Assert.Empty(platform.Popped);
			Assert.Equal(1, platform.DisposeCount);
			Assert.Empty(Host(window).PlatformModalStack);
		}

		[Fact]
		public void LateResolutionDeliversPageAttachedExactlyOnce()
		{
			var factory = new StubModalNavigationPlatformFactory(host => new RecordingModalNavigationPlatform(host));

			// Page handler attaches while the window has no service scope, so the override cannot exist
			// yet and would otherwise never learn that the page is attached.
			var window = new Window();
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			Assert.Equal(0, factory.CallCount);

			var services = Substitute.For<IServiceProvider>();
			RegisterFactory(services, factory);
			var mauiContext = Substitute.For<IMauiContext>();
			mauiContext.Services.Returns(services);
			var handler = Substitute.For<IElementHandler>();
			handler.MauiContext.Returns(mauiContext);

			window.Handler = handler;
			ForceResolvePlatform(window);

			var platform = (RecordingModalNavigationPlatform)factory.Created[0];

			// Delivered by the late creation, and not doubled by the call that triggered it.
			Assert.Equal(1, platform.PageAttachedCount);

			// A genuinely new attachment still notifies.
			AttachRootPage(window);

			Assert.Equal(2, platform.PageAttachedCount);
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
			ForceResolvePlatform(window);

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
			Assert.True(host.IsWindowReady);
			Assert.False(host.IsBatchPopping);
			Assert.False(host.IsBatchPushing);

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
			ForceResolvePlatform(window);

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

		// The shape a backend would naturally write. It exists to prove IsWindowReady is recursion-free.
		sealed class SelfConsultingModalNavigationPlatform : IModalNavigationPlatform
		{
			readonly IModalNavigationHost _host;

			public SelfConsultingModalNavigationPlatform(IModalNavigationHost host)
			{
				_host = host;
			}

			public bool IsReady => _host.IsWindowReady;

			public Task PushModalAsync(Page modal, bool animated) => Task.CompletedTask;

			public Task PopModalAsync(Page modal, bool animated) => Task.CompletedTask;

			public void PageAttached()
			{
			}

			public void Dispose()
			{
			}
		}

		sealed class RecordingModalNavigationPlatform : IModalNavigationPlatform
		{
			public RecordingModalNavigationPlatform(IModalNavigationHost host)
			{
				Host = host;
			}

			public IModalNavigationHost Host { get; set; }

			public bool IsReady
			{
				get
				{
					IsReadyReadThreadIds.Add(Environment.CurrentManagedThreadId);
					return IsReadyValue;
				}
			}

			public bool IsReadyValue { get; set; } = true;

			public List<int> IsReadyReadThreadIds { get; } = new();

			public List<string> Operations { get; } = new();

			public List<ModalOperation> Pushed { get; } = new();

			public List<ModalOperation> Popped { get; } = new();

			public List<int> OperationThreadIds { get; } = new();

			public int PageAttachedCount { get; private set; }

			public int DisposeCount { get; private set; }

			public Action<Page, bool> PushBehavior { get; set; }

			public Action<Page, bool> PopBehavior { get; set; }

			public Func<Page, bool, Task> PushTaskBehavior { get; set; }

			public Func<Page, bool, Task> PopTaskBehavior { get; set; }

			public async Task PushModalAsync(Page modal, bool animated)
			{
				PushBehavior?.Invoke(modal, animated);
				if (PushTaskBehavior is not null)
					await PushTaskBehavior(modal, animated);

				OperationThreadIds.Add(Environment.CurrentManagedThreadId);
				Operations.Add($"Push:{Pushed.Count}");
				Pushed.Add(new ModalOperation(modal, animated, Host.CurrentPlatformPage, new List<Page>(Host.PlatformModalStack)));
			}

			public async Task PopModalAsync(Page modal, bool animated)
			{
				PopBehavior?.Invoke(modal, animated);
				if (PopTaskBehavior is not null)
					await PopTaskBehavior(modal, animated);

				OperationThreadIds.Add(Environment.CurrentManagedThreadId);
				Operations.Add($"Pop:{Pushed.Count - Popped.Count - 1}");
				Popped.Add(new ModalOperation(modal, animated, Host.CurrentPlatformPage, new List<Page>(Host.PlatformModalStack)));
			}

			public void PageAttached() => PageAttachedCount++;

			public void Dispose() => DisposeCount++;
		}
	}
}
