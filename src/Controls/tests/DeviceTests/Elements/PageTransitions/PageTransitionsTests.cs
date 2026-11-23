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

		void SetupBuilder(Func<NavigationTransitionRequest, PlatformNavigationTransition> provider = null)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler<Window, WindowHandlerStub>();
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Label, LabelHandler>();
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
