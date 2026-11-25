using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.PageTransitions)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public partial class PageTransitionsTests : ControlsHandlerTestBase
	{
		static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(400);

		readonly List<NavigationTransitionRequest> _requests = new();

		void SetupBuilder(Func<NavigationTransitionRequest, PlatformNavigationTransition> provider = null, Action<IMauiHandlersCollection> configureHandlers = null)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					// First: the defaults below only fill in what the test didn't register.
					configureHandlers?.Invoke(handlers);
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler<Window, WindowHandlerStub>();
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<BoxView, Microsoft.Maui.Controls.Handlers.BoxViewHandler>();
					handlers.AddHandler<Layout, LayoutHandler>();
#if ANDROID
					// The Shell handlers an app registers by default on Android (SetupShellHandlers maps Shell to
					// the legacy renderer).
					handlers.AddHandler<Shell, Microsoft.Maui.Controls.Handlers.ShellHandler>();
					handlers.AddHandler<ShellItem, Microsoft.Maui.Controls.Handlers.ShellItemHandler>();
					handlers.AddHandler<ShellSection, Microsoft.Maui.Controls.Handlers.ShellSectionHandler>();
#endif
					SetupShellHandlers(handlers);
				});

				// Records every request MAUI resolves; declines unless the test supplies an answer.
				builder.Services.AddSingleton<INavigationTransitionProvider>(new DelegateProvider(request =>
				{
					_requests.Add(request);
					return provider?.Invoke(request);
				}));
			});
		}

		static ContentPage CreatePage(string text) => new ContentPage { Content = new Label { Text = text } };

		[Fact(DisplayName = "Preset push and pop complete after the transition with both pages intact")]
		public async Task PresetPushAndPopCompleteAfterTheTransition()
		{
			SetupBuilder();
			var root = CreatePage("Root");
			var detail = CreatePage("Detail");
			PageTransitions.SetTransition(detail, new SlideTransition { Duration = TransitionDuration });
			var navPage = new NavigationPage(root);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (_) =>
			{
				await OnNavigatedToAsync(root);

				var push = Stopwatch.StartNew();
				await navPage.PushAsync(detail);
				push.Stop();

				Assert.Same(detail, navPage.CurrentPage);
				Assert.Equal(2, navPage.Navigation.NavigationStack.Count);
				AssertCompletedAfterTransition(push.Elapsed);
				AssertPageAtRest(detail);

				var pop = Stopwatch.StartNew();
				await navPage.PopAsync();
				pop.Stop();

				Assert.Same(root, navPage.CurrentPage);
				Assert.Single(navPage.Navigation.NavigationStack);
				AssertCompletedAfterTransition(pop.Elapsed);
				AssertPageAtRest(root);
			});

			Assert.Equal(2, _requests.Count);
			Assert.Equal(NavigationTransitionKind.Push, _requests[0].Kind);
			Assert.Same(detail, _requests[0].To);
			Assert.Equal(NavigationTransitionKind.Pop, _requests[1].Kind);
			Assert.Same(detail, _requests[1].From);
			Assert.True(_requests[1].IsBack);
			Assert.IsType<SlideTransition>(_requests[1].Transition);
		}

		[Fact(DisplayName = "Pages that set nothing keep the platform animation")]
		public async Task PlatformDefaultIsUnchanged()
		{
			SetupBuilder();
			var root = CreatePage("Root");
			var detail = CreatePage("Detail");
			var navPage = new NavigationPage(root);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (_) =>
			{
				await OnNavigatedToAsync(root);
				await navPage.PushAsync(detail);
				await OnNavigatedToAsync(detail);
				await navPage.PopAsync();

				Assert.Same(root, navPage.CurrentPage);
				AssertPageAtRest(root);
			});

			Assert.All(_requests, r => Assert.Same(PageTransition.Platform, r.Transition));
		}

		[Fact(DisplayName = "A provider that suppresses the animation navigates without one")]
		public async Task ProviderCanSuppressTheAnimation()
		{
			SetupBuilder(_ => PlatformNavigationTransition.Suppress);
			var root = CreatePage("Root");
			var detail = CreatePage("Detail");
			PageTransitions.SetTransition(detail, new FadeTransition { Duration = TimeSpan.FromSeconds(2) });
			var navPage = new NavigationPage(root);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (_) =>
			{
				await OnNavigatedToAsync(root);
				var push = Stopwatch.StartNew();
				await navPage.PushAsync(detail);
				push.Stop();

				Assert.Same(detail, navPage.CurrentPage);
				// The 2 second fade never ran.
				Assert.True(push.Elapsed < TimeSpan.FromSeconds(1.5), $"Push took {push.Elapsed}");
			});

			Assert.Single(_requests);
		}

		[Fact(DisplayName = "A navigation started during a transition leaves the stack consistent")]
		public async Task SecondNavigationDuringATransitionLeavesTheStackConsistent()
		{
			SetupBuilder();
			var root = CreatePage("Root");
			var first = CreatePage("First");
			var second = CreatePage("Second");
			PageTransitions.SetTransition(first, new SlideTransition { Duration = TransitionDuration });
			PageTransitions.SetTransition(second, new ScaleTransition { Duration = TransitionDuration });
			var navPage = new NavigationPage(root);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (_) =>
			{
				await OnNavigatedToAsync(root);

				var pushFirst = navPage.PushAsync(first);
				var pushSecond = navPage.PushAsync(second);
				await Task.WhenAll(pushFirst, pushSecond).WaitAsync(TimeSpan.FromSeconds(10));

				Assert.Equal(new Page[] { root, first, second }, navPage.Navigation.NavigationStack);
				Assert.Same(second, navPage.CurrentPage);
				AssertPageAtRest(second);
			});
		}

		[Fact(DisplayName = "Per-navigation overloads run their transition and pop reverses it")]
		public async Task PerNavigationOverloadIsReversedOnPop()
		{
			SetupBuilder();
			var root = CreatePage("Root");
			var detail = CreatePage("Detail");
			PageTransitions.SetTransition(detail, new FadeTransition());
			var navPage = new NavigationPage(root);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (_) =>
			{
				await OnNavigatedToAsync(root);
				await navPage.Navigation.PushAsync(detail, new SlideTransition { From = SlideEdge.Bottom, Duration = TransitionDuration });
				await navPage.PopAsync();
			});

			Assert.Equal(SlideEdge.Bottom, Assert.IsType<SlideTransition>(_requests[0].Transition).From);
			Assert.Equal(SlideEdge.Bottom, Assert.IsType<SlideTransition>(_requests[1].Transition).From);
		}

#if IOS || MACCATALYST || ANDROID
		// Shared elements fly on iOS, Mac Catalyst and Android; WinUI's navigation transitions don't host them.
		[Fact(DisplayName = "A shared element flies during the transition and is restored after it")]
		public async Task SharedElementFliesAndIsRestored()
		{
			SetupBuilder();
			var source = new BoxView { Color = Graphics.Colors.SteelBlue, HeightRequest = 80 };
			var target = new BoxView { Color = Graphics.Colors.SteelBlue, HeightRequest = 200 };
			SharedTransition.SetName(source, "photo");
			SharedTransition.SetName(target, "photo");
			var root = new ContentPage { Content = new VerticalStackLayout { source } };
			var detail = new ContentPage { Content = new VerticalStackLayout { target } };
			PageTransitions.SetTransition(detail, new SlideTransition { Duration = TimeSpan.FromSeconds(1) });
			var navPage = new NavigationPage(root);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (_) =>
			{
				await OnLoadedAsync(source);

				var push = navPage.PushAsync(detail);

				// While it flies, the real source is hidden under its snapshot.
				var hidden = false;
				var watch = Stopwatch.StartNew();
#if ANDROID
				var picturesSeen = 0;
#endif
				while (!hidden && watch.Elapsed < TimeSpan.FromMilliseconds(800))
				{
#if ANDROID
					picturesSeen = Math.Max(picturesSeen, PageTransitionAnimations.HeroFlights.ActivePictures);
#endif
					hidden = !NativeShown(source);
					if (!hidden)
					{
						await Task.Yield();
					}
				}

				await push.WaitAsync(TimeSpan.FromSeconds(10));

				Assert.True(hidden, "The source element was never hidden: no shared-element flight ran.");
#if ANDROID
				Assert.True(picturesSeen > 0, "No flight picture was shown.");
#endif

				// The flight lands with the page transition; allow its end action a moment to run.
				var landing = Stopwatch.StartNew();
				while ((!NativeShown(source) || !NativeShown(target)) && landing.Elapsed < TimeSpan.FromMilliseconds(500))
				{
					await Task.Yield();
				}

				Assert.True(NativeShown(source), "The source was left hidden.");
				Assert.True(NativeShown(target), "The target was left hidden.");
				Assert.Equal(1, NativeAlpha(source), 3);
				Assert.Equal(1, NativeAlpha(target), 3);
#if ANDROID
				// The flight's pictures are gone from the window overlay.
				Assert.Equal(0, PageTransitionAnimations.HeroFlights.ActivePictures);
#endif
			});
		}

		[Fact(DisplayName = "Changing the destination's Opacity during a flight doesn't reveal it under the snapshot")]
		public async Task OpacityChangedDuringAFlightDoesntRevealTheDestination()
		{
			SetupBuilder();
			var source = new BoxView { Color = Graphics.Colors.SteelBlue, HeightRequest = 80 };
			var target = new BoxView { Color = Graphics.Colors.SteelBlue, HeightRequest = 200 };
			SharedTransition.SetName(source, "photo");
			SharedTransition.SetName(target, "photo");
			var root = new ContentPage { Content = new VerticalStackLayout { source } };
			var detail = new ContentPage { Content = new VerticalStackLayout { target } };
			PageTransitions.SetTransition(detail, new SlideTransition { Duration = TimeSpan.FromSeconds(1.5) });
			var navPage = new NavigationPage(root);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (_) =>
			{
				await OnLoadedAsync(source);
				var push = navPage.PushAsync(detail);

				// Wait for the flight to hide the destination, then change its Opacity (as a binding would).
				var watch = Stopwatch.StartNew();
				while (NativeShown(target) && watch.Elapsed < TimeSpan.FromMilliseconds(1000))
				{
					await Task.Yield();
				}

				Assert.False(NativeShown(target), "The destination was never hidden: no flight ran.");
				target.Opacity = 0.25;

				// For the rest of the flight the real destination stays hidden under its snapshot.
				var revealed = false;
				while (!push.IsCompleted && !revealed)
				{
					revealed = NativeShown(target);
					await Task.Yield();
				}

				await push.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.False(revealed, "The destination showed under its snapshot during the flight.");

				// Once landed, it shows its current Opacity.
				var landing = Stopwatch.StartNew();
				while (!NativeShown(target) && landing.Elapsed < TimeSpan.FromMilliseconds(500))
				{
					await Task.Yield();
				}

				Assert.True(NativeShown(target));
				Assert.Equal(0.25, NativeAlpha(target), 3);
			});
		}

		// Whether the element's native view is on screen at all: not hidden and not fully transparent.
		static bool NativeShown(VisualElement element)
		{
			var handler = (IPlatformViewHandler)element.Handler;
			var view = handler.ContainerView ?? handler.PlatformView;
#if IOS || MACCATALYST
			return !view.Hidden && view.Alpha > 0;
#else
			return view.Visibility == global::Android.Views.ViewStates.Visible && view.Alpha > 0;
#endif
		}

		static double NativeAlpha(VisualElement element)
		{
			var handler = (IPlatformViewHandler)element.Handler;
			var view = handler.ContainerView ?? handler.PlatformView;
#if IOS || MACCATALYST
			return (double)view.Alpha;
#else
			return view.Alpha;
#endif
		}
#endif

#if ANDROID
		[Fact(DisplayName = "A navigation with a longer entrance than exit completes after the entrance")]
		public async Task LongerEntranceThanExitCompletesAfterTheEntrance()
		{
			// A provider's animation: a long entrance and a short exit.
			SetupBuilder(_ => new PlatformNavigationTransition(enter => new global::Android.Views.Animations.AlphaAnimation(enter ? 0.2f : 1f, 1f)
			{
				Duration = enter ? 1500 : 100,
			}));

			var root = CreatePage("Root");
			var detail = CreatePage("Detail");
			var navigatedTo = false;
			detail.NavigatedTo += (_, _) => navigatedTo = true;
			var navPage = new NavigationPage(root);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (_) =>
			{
				await OnNavigatedToAsync(root);

				var push = Stopwatch.StartNew();
				var pushTask = navPage.PushAsync(detail);
				await Task.WhenAny(pushTask, Task.Delay(900));

				// The outgoing page went long ago; the entrance is still running.
				Assert.False(pushTask.IsCompleted, $"Push completed after {push.Elapsed}, before the 1500 ms entrance ended.");
				Assert.False(navigatedTo);

				await pushTask.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.True(push.Elapsed >= TimeSpan.FromMilliseconds(1300), $"Push completed after {push.Elapsed}.");
				Assert.True(navigatedTo);
			});
		}
#endif

#if ANDROID
		[Fact(DisplayName = "A modal transition's Background is shown behind the modal while it animates")]
		public async Task ModalTransitionBackgroundIsShown()
		{
			SetupBuilder();
			var root = CreatePage("Root");
			var modal = CreatePage("Modal");
			PageTransitions.SetModalTransition(modal, new ScaleTransition
			{
				FromScale = 0.5,
				Duration = TimeSpan.FromSeconds(1),
				Background = new SolidColorBrush(Graphics.Colors.Red),
			});

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(root), async (_) =>
			{
				await OnLoadedAsync(root.Content);

				var push = root.Navigation.PushModalAsync(modal);
				var seen = false;
				var watch = Stopwatch.StartNew();
				while (!seen && watch.Elapsed < TimeSpan.FromMilliseconds(900))
				{
					seen = BackdropColor(modal) == global::Android.Graphics.Color.Red.ToArgb();
					if (!seen)
					{
						await Task.Yield();
					}
				}

				await push.WaitAsync(TimeSpan.FromSeconds(10));

				Assert.True(seen, "The transition's Background never showed behind the modal.");
				Assert.NotEqual(global::Android.Graphics.Color.Red.ToArgb(), BackdropColor(modal));
			});
		}

		static int? BackdropColor(Page page)
			=> ((page.Handler as IPlatformViewHandler)?.PlatformView?.RootView?.Background as global::Android.Graphics.Drawables.ColorDrawable)?.Color.ToArgb();

		[Fact(DisplayName = "A root replacement started during another settles on the newest root")]
		public async Task OverlappingRootReplacementsSettle()
		{
			SetupBuilder();
			var first = CreatePage("First");
			var second = CreatePage("Second");
			var third = CreatePage("Third");
			var window = new Window(first);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				await OnLoadedAsync(first.Content);
				var content = handler.PlatformView.Window?.DecorView?.FindViewById(global::Android.Resource.Id.Content);
				Assert.NotNull(content);

				// The second replacement starts from the first one's page assignment.
				Task nested = null;
				window.PropertyChanged += (_, e) =>
				{
					if (e.PropertyName == Window.PageProperty.PropertyName && ReferenceEquals(window.Page, second) && nested is null)
					{
						nested = window.ReplacePageAsync(third, new FadeTransition { Duration = TimeSpan.FromMilliseconds(400) });
					}
				};

				var outer = window.ReplacePageAsync(second, new SlideTransition { Duration = TimeSpan.FromMilliseconds(800) });
				await outer.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.NotNull(nested);
				await nested.WaitAsync(TimeSpan.FromSeconds(10));

				Assert.Same(third, window.Page);
				Assert.Equal(0, RootPageTransition.ActivePictures);
				Assert.Equal(0, content.TranslationX);
				Assert.Equal(1, content.ScaleX);
				Assert.Equal(1, content.Alpha);
			});
		}
#endif

		[Fact(DisplayName = "An unanimated push is recorded, so an animated pop reverses what the page was shown with")]
		public async Task UnanimatedPushIsRecordedForThePop()
		{
			SetupBuilder();
			var root = CreatePage("Root");
			var detail = CreatePage("Detail");
			PageTransitions.SetTransition(detail, new FadeTransition { Duration = TransitionDuration });
			var navPage = new NavigationPage(root);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (_) =>
			{
				await OnNavigatedToAsync(root);
				await navPage.PushAsync(detail, false);
				await OnNavigatedToAsync(detail);

				// Changed after the push: the pop still reverses the Fade the page was shown with.
				PageTransitions.SetTransition(detail, new SlideTransition());
				await navPage.PopAsync(true);
				Assert.Same(root, navPage.CurrentPage);
			});

			var pop = Assert.Single(_requests, r => r.Kind == NavigationTransitionKind.Pop);
			Assert.IsType<FadeTransition>(pop.Transition);
		}

#if !WINDOWS
		// Windows modals keep their platform presentation; modal transitions are wired on iOS, Mac Catalyst and Android.
		[Fact(DisplayName = "Modal present and dismiss with a transition complete")]
		public async Task ModalPresentAndDismissComplete()
		{
			SetupBuilder();
			var root = CreatePage("Root");
			var modal = CreatePage("Modal");
			modal.BackgroundColor = Graphics.Colors.White;
			PageTransitions.SetModalTransition(modal, new SlideTransition { From = SlideEdge.Bottom, Duration = TransitionDuration });

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(root), async (_) =>
			{
				await OnLoadedAsync(root.Content);

				var push = Stopwatch.StartNew();
				await root.Navigation.PushModalAsync(modal);
				push.Stop();
				await OnLoadedAsync(modal.Content);

				Assert.Single(root.Navigation.ModalStack);
				AssertCompletedAfterTransition(push.Elapsed);
				AssertPageAtRest(modal);

				await root.Navigation.PopModalAsync();

				Assert.Empty(root.Navigation.ModalStack);
				AssertPageAtRest(root);
			});

			Assert.Equal(NavigationTransitionKind.PushModal, _requests[0].Kind);
			Assert.Equal(NavigationTransitionKind.PopModal, _requests[1].Kind);
			Assert.IsType<SlideTransition>(_requests[1].Transition);
		}

#endif

		[Fact(DisplayName = "Shell GoToAsync with a transition pushes and pops")]
		public async Task ShellGoToAsyncWithATransition()
		{
			SetupBuilder();
			Routing.RegisterRoute("pagetransitions-detail", typeof(ContentPage));
			try
			{
				var root = CreatePage("Root");
				var shell = new Shell { CurrentItem = root };

				await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(shell), async (_) =>
				{
					await OnNavigatedToAsync(root);
					await shell.GoToAsync("pagetransitions-detail", new SlideTransition { Duration = TransitionDuration });
					var detail = shell.CurrentPage;
					Assert.NotSame(root, detail);
					await OnNavigatedToAsync(detail);
					AssertPageAtRest(detail);

					await shell.GoToAsync("..");
					Assert.Same(root, shell.CurrentPage);
					AssertPageAtRest(root);
				});

				Assert.Contains(_requests, r => r.Kind == NavigationTransitionKind.Push && r.Transition is SlideTransition);
				Assert.Contains(_requests, r => r.Kind == NavigationTransitionKind.Pop && r.Transition is SlideTransition);
			}
			finally
			{
				Routing.UnRegisterRoute("pagetransitions-detail");
			}
		}

		static void AssertCompletedAfterTransition(TimeSpan elapsed)
		{
#if IOS || MACCATALYST || ANDROID
			// The navigation task completes when the transition does. (WinUI's transitions have their own fixed length.)
			Assert.True(elapsed >= TransitionDuration * 0.8, $"Navigation completed after {elapsed}, before the {TransitionDuration} transition ended.");
#endif
		}

		static void AssertPageAtRest(Page page)
		{
			// Once a transition ends, no pose is left on the page's view.
			var view = (page.Handler as IPlatformViewHandler)?.PlatformView;
			Assert.NotNull(view);
#if IOS || MACCATALYST
			Assert.True(view.Transform.IsIdentity, $"Transform left at {view.Transform}");
			Assert.Equal(1, (double)view.Alpha, 3);
			Assert.NotNull(view.Window);
#elif ANDROID
			Assert.Equal(0, view.TranslationX);
			Assert.Equal(0, view.TranslationY);
			Assert.Equal(1, view.ScaleX);
			Assert.Equal(1, view.Alpha);
			Assert.True(view.IsAttachedToWindow);
#endif
		}

		sealed class DelegateProvider : INavigationTransitionProvider
		{
			readonly Func<NavigationTransitionRequest, PlatformNavigationTransition> _resolve;

			public DelegateProvider(Func<NavigationTransitionRequest, PlatformNavigationTransition> resolve) => _resolve = resolve;

			public PlatformNavigationTransition GetTransition(NavigationTransitionRequest request) => _resolve(request);
		}
	}
}
