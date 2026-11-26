using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	[Collection(PageTransitionCollection.Name)]
	public class PageTransitionTests : BaseTestFixture
	{
		public PageTransitionTests()
		{
			PageTransitionResolver.ReducedMotionOverride = false;
		}

		protected override void Dispose(bool disposing)
		{
			PageTransitionResolver.ReducedMotionOverride = null;
			base.Dispose(disposing);
		}

		// ---- The transition types ----

		[Fact]
		public void BuiltInsDefaultTo300MillisecondsAndThePlatformCurve()
		{
			var slide = new SlideTransition();
			var scale = new ScaleTransition();
			var fade = new FadeTransition();

			Assert.Equal(TimeSpan.FromMilliseconds(300), slide.Duration);
			Assert.Equal(TimeSpan.FromMilliseconds(300), fade.Duration);
			Assert.Equal(TimeSpan.FromMilliseconds(300), scale.Duration);
			Assert.Null(slide.Easing);
			Assert.Null(slide.Background);
			Assert.Equal(SlideEdge.End, slide.From);
			Assert.Equal(0.92, scale.FromScale);
		}

		[Fact]
		public void SharedValuesCantBeChanged()
		{
			Assert.Throws<InvalidOperationException>(() => PageTransition.Platform.Duration = TimeSpan.FromSeconds(1));
			Assert.Throws<InvalidOperationException>(() => PageTransition.None.Easing = Easing.Linear);
			Assert.Equal(TimeSpan.Zero, PageTransition.None.Duration);
		}

		[Fact]
		public void NegativeDurationIsRejected()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new FadeTransition { Duration = TimeSpan.FromMilliseconds(-1) });
			Assert.Throws<ArgumentOutOfRangeException>(() => new ScaleTransition { FromScale = -0.5 });
		}

		[Theory]
		[InlineData(double.PositiveInfinity)]
		[InlineData(double.NegativeInfinity)]
		[InlineData(double.NaN)]
		[InlineData(-0.01)]
		public void FromScaleMustBeFiniteAndNotNegative(double value)
		{
			var scale = new ScaleTransition();
			Assert.Throws<ArgumentOutOfRangeException>(() => scale.FromScale = value);
			Assert.Equal(0.92, scale.FromScale);
		}

		[Theory]
		[InlineData(0.0)]
		[InlineData(1.5)]
		public void FromScaleAcceptsFiniteValues(double value)
		{
			Assert.Equal(value, new ScaleTransition { FromScale = value }.FromScale);
		}

		[Fact]
		public void SnapshotIsIsolatedFromLaterChanges()
		{
			var slide = new SlideTransition { From = SlideEdge.Bottom, Duration = TimeSpan.FromMilliseconds(500) };
			var snapshot = (SlideTransition)slide.Snapshot();

			slide.From = SlideEdge.Top;
			slide.Duration = TimeSpan.FromMilliseconds(100);

			Assert.NotSame(slide, snapshot);
			Assert.Equal(SlideEdge.Bottom, snapshot.From);
			Assert.Equal(TimeSpan.FromMilliseconds(500), snapshot.Duration);
			Assert.Throws<InvalidOperationException>(() => snapshot.From = SlideEdge.Start);
			Assert.Same(snapshot, snapshot.Snapshot());
		}

		[Theory]
		[InlineData("Platform", typeof(PageTransition), null)]
		[InlineData("None", typeof(PageTransition), null)]
		[InlineData("Fade", typeof(FadeTransition), null)]
		[InlineData("fade", typeof(FadeTransition), null)]
		[InlineData("Scale", typeof(ScaleTransition), null)]
		[InlineData("SlideFromEnd", typeof(SlideTransition), SlideEdge.End)]
		[InlineData("SlideFromStart", typeof(SlideTransition), SlideEdge.Start)]
		[InlineData("SlideFromTop", typeof(SlideTransition), SlideEdge.Top)]
		[InlineData(" SlideFromBottom ", typeof(SlideTransition), SlideEdge.Bottom)]
		public void ConverterParsesNames(string text, Type expectedType, SlideEdge? edge)
		{
			var converter = new PageTransitionTypeConverter();
			var result = Assert.IsAssignableFrom<PageTransition>(converter.ConvertFromInvariantString(text));

			Assert.True(expectedType.IsInstanceOfType(result));
			if (edge is { } expectedEdge)
			{
				Assert.Equal(expectedEdge, ((SlideTransition)result).From);
			}
		}

		[Fact]
		public void ConverterReturnsTheSharedPlatformAndNoneValues()
		{
			var converter = new PageTransitionTypeConverter();
			Assert.Same(PageTransition.Platform, converter.ConvertFromInvariantString("Platform"));
			Assert.Same(PageTransition.None, converter.ConvertFromInvariantString("None"));
		}

		[Fact]
		public void ConverterCreatesANewInstanceEachTime()
		{
			var converter = new PageTransitionTypeConverter();
			Assert.NotSame(converter.ConvertFromInvariantString("Fade"), converter.ConvertFromInvariantString("Fade"));
		}

		[Theory]
		[InlineData("Slide From End")]
		[InlineData("Zoom")]
		[InlineData("Fade(300)")]
		public void ConverterRejectsUnknownNames(string text)
		{
			var converter = new PageTransitionTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(text));
		}

		[Fact]
		public void ConverterRoundTripsNames()
		{
			var converter = new PageTransitionTypeConverter();
			Assert.Equal("SlideFromTop", converter.ConvertToInvariantString(new SlideTransition { From = SlideEdge.Top }));
			Assert.Equal("Fade", converter.ConvertToInvariantString(new FadeTransition()));
			Assert.Equal("None", converter.ConvertToInvariantString(PageTransition.None));
		}

		// ---- Resolution order ----

		[Fact]
		public async Task NothingSetRunsThePlatformAnimation()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());

			await nav.PushAsync(new ContentPage());

			Assert.Single(handler.Resolved);
			Assert.Same(PageTransition.Platform, handler.Last.Transition);
			Assert.True(handler.Last.IsPlatformDefault);
			Assert.Equal(NavigationTransitionKind.Push, handler.Last.Request.Kind);
		}

		[Fact]
		public async Task WindowSetsTheDefault()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			PageTransitions.SetTransition(nav.Window, new ScaleTransition());

			await nav.PushAsync(new ContentPage());

			Assert.IsType<ScaleTransition>(handler.Last.Transition);
		}

		[Fact]
		public async Task NavigationPageBeatsWindow()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			PageTransitions.SetTransition(nav.Window, new ScaleTransition());
			PageTransitions.SetTransition(nav, new FadeTransition());

			await nav.PushAsync(new ContentPage());

			Assert.IsType<FadeTransition>(handler.Last.Transition);
		}

		[Fact]
		public async Task PageBeatsNavigationPage()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			PageTransitions.SetTransition(nav, new FadeTransition());
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new SlideTransition { From = SlideEdge.Bottom });

			await nav.PushAsync(page);

			var slide = Assert.IsType<SlideTransition>(handler.Last.Transition);
			Assert.Equal(SlideEdge.Bottom, slide.From);
		}

		[Fact]
		public async Task PerNavigationArgumentBeatsPage()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new SlideTransition());

			await nav.Navigation.PushAsync(page, new ScaleTransition { FromScale = 0.5 });

			var scale = Assert.IsType<ScaleTransition>(handler.Last.Transition);
			Assert.Equal(0.5, scale.FromScale);
		}

		[Fact]
		public async Task ExplicitDontAnimateBeatsEverything()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			PageTransitions.SetTransition(nav, new FadeTransition());
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new SlideTransition());

			await nav.Navigation.PushAsync(page, false);

			Assert.Same(PageTransition.None, handler.Last.Transition);
			Assert.True(handler.Last.IsSuppressed);
		}

		[Fact]
		public async Task NoneArgumentNavigatesWithoutAnimation()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new SlideTransition());

			await nav.Navigation.PushAsync(page, PageTransition.None);

			Assert.True(handler.Last.IsSuppressed);
			Assert.Same(PageTransition.None, handler.Last.Transition);
		}

		[Fact]
		public async Task NoneArgumentIsWhatThePopReverses()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new FadeTransition());

			await nav.Navigation.PushAsync(page, PageTransition.None);
			await nav.Navigation.PopAsync(true);

			Assert.Same(PageTransition.None, PageTransitionResolver.GetShownTransition(page));
			Assert.Same(PageTransition.None, handler.Resolved[1].Transition);
			Assert.True(handler.Resolved[1].IsSuppressed);
		}

		[Fact]
		public void NoneArgumentForAModalIsWhatTheDismissReverses()
		{
			var modal = new ContentPage();
			PageTransitions.SetModalTransition(modal, new FadeTransition());
			var token = PageTransitionResolver.SetPending(modal, NavigationTransitionKind.PushModal, PageTransition.None);

			var push = PageTransitionResolver.Resolve(NavigationTransitionKind.PushModal, null, modal, animated: false);
			PageTransitionResolver.ClearPending(modal, token);
			var pop = PageTransitionResolver.Resolve(NavigationTransitionKind.PopModal, modal, null, animated: true);

			Assert.True(push.IsSuppressed);
			Assert.Same(PageTransition.None, pop.Transition);
		}

		[Fact]
		public async Task PerNavigationArgumentIsUsedOnce()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new SlideTransition());

			await nav.Navigation.PushAsync(page, new FadeTransition());
			await nav.PopAsync();
			await nav.PushAsync(page);

			Assert.Equal(3, handler.Resolved.Count);
			Assert.IsType<FadeTransition>(handler.Resolved[0].Transition);
			Assert.IsType<SlideTransition>(handler.Resolved[2].Transition);
		}

		[Fact]
		public async Task ExistingCallsStillBindToTheInstanceMethods()
		{
			// PushAsync(page, bool) is INavigation's own method: no transition argument is recorded for it.
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			INavigation navigation = nav.Navigation;

			await navigation.PushAsync(new ContentPage(), true);
			await navigation.PopAsync(true);

			Assert.Same(PageTransition.Platform, handler.Resolved[0].Transition);
			Assert.Same(PageTransition.Platform, handler.Resolved[1].Transition);
		}

		[Fact]
		public async Task TransitionIsSnapshottedWhenNavigationStarts()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			var slide = new SlideTransition { Duration = TimeSpan.FromMilliseconds(400) };
			var page = new ContentPage();
			PageTransitions.SetTransition(page, slide);

			await nav.PushAsync(page);
			slide.Duration = TimeSpan.FromMilliseconds(50);

			Assert.NotSame(slide, handler.Last.Transition);
			Assert.Equal(TimeSpan.FromMilliseconds(400), handler.Last.Transition.Duration);
		}

		// ---- Back navigation reverses what ran ----

		[Fact]
		public async Task PopReversesThePageTransition()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new SlideTransition { From = SlideEdge.Top });

			await nav.PushAsync(page);
			await nav.PopAsync();

			var pop = handler.Last;
			Assert.Equal(NavigationTransitionKind.Pop, pop.Request.Kind);
			Assert.True(pop.Request.IsBack);
			Assert.Same(page, pop.Request.From);
			Assert.Equal(SlideEdge.Top, Assert.IsType<SlideTransition>(pop.Transition).From);
		}

		[Fact]
		public async Task PopReversesAOneOffForwardOverride()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new SlideTransition());

			await nav.Navigation.PushAsync(page, new ScaleTransition());
			await nav.PopAsync();

			Assert.IsType<ScaleTransition>(handler.Last.Transition);
		}

		[Fact]
		public async Task PopIgnoresChangesMadeAfterThePush()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new FadeTransition());

			await nav.PushAsync(page);
			PageTransitions.SetTransition(page, new SlideTransition());
			PageTransitions.SetTransition(nav, new ScaleTransition());
			await nav.PopAsync();

			Assert.IsType<FadeTransition>(handler.Last.Transition);
		}

		[Fact]
		public async Task ExplicitPopArgumentBeatsTheRecordedTransition()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new FadeTransition());

			await nav.PushAsync(page);
			await nav.Navigation.PopAsync(new SlideTransition { From = SlideEdge.Bottom });

			Assert.Equal(SlideEdge.Bottom, Assert.IsType<SlideTransition>(handler.Last.Transition).From);
		}

		[Fact]
		public async Task UnanimatedPushStillPopsWithThePageTransition()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new FadeTransition());

			await nav.Navigation.PushAsync(page, false);
			await nav.Navigation.PopAsync(true);

			Assert.True(handler.Resolved[0].IsSuppressed);
			Assert.IsType<FadeTransition>(handler.Resolved[1].Transition);
		}

		[Fact]
		public async Task UnanimatedPopRunsNone()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new FadeTransition());

			await nav.PushAsync(page);
			await nav.Navigation.PopAsync(false);

			Assert.True(handler.Last.IsSuppressed);
		}

		[Fact]
		public void PageThatWasNeverShownPopsWithItsOwnTransition()
		{
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new ScaleTransition());

			var resolved = PageTransitionResolver.Resolve(NavigationTransitionKind.Pop, page, new ContentPage(), true);

			Assert.IsType<ScaleTransition>(resolved.Transition);
		}

		// ---- Flow direction ----

		[Fact]
		public async Task PopRunsInTheFlowDirectionThePageWasShownIn()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage { FlowDirection = FlowDirection.LeftToRight });
			var page = new ContentPage { FlowDirection = FlowDirection.RightToLeft };
			PageTransitions.SetTransition(page, new SlideTransition());

			await nav.PushAsync(page);
			page.FlowDirection = FlowDirection.LeftToRight;
			await nav.PopAsync();

			Assert.Equal(FlowDirection.RightToLeft, handler.Resolved[0].Request.FlowDirection);
			Assert.Equal(FlowDirection.RightToLeft, handler.Resolved[1].Request.FlowDirection);
		}

		[Fact]
		public void PopOfAPageNeverShownUsesTheOutgoingPagesFlowDirection()
		{
			var outgoing = new ContentPage { FlowDirection = FlowDirection.RightToLeft };
			var destination = new ContentPage { FlowDirection = FlowDirection.LeftToRight };

			var pop = PageTransitionResolver.Resolve(NavigationTransitionKind.Pop, outgoing, destination, true);

			Assert.Equal(FlowDirection.RightToLeft, pop.Request.FlowDirection);
		}

		// ---- Scoping of per-navigation arguments ----

		[Fact]
		public void StagedTransitionIsOnlyUsedByItsKindOfNavigation()
		{
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new SlideTransition());
			PageTransitionResolver.SetPending(page, NavigationTransitionKind.PushModal, new ScaleTransition());

			var push = PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, page, true);
			var modal = PageTransitionResolver.Resolve(NavigationTransitionKind.PushModal, null, page, true);

			Assert.IsType<SlideTransition>(push.Transition);
			Assert.IsType<ScaleTransition>(modal.Transition);
			Assert.False(PageTransitionResolver.HasPending(page));
		}

		[Fact]
		public async Task PopOnTheRootPageStagesNothing()
		{
			var root = new ContentPage();
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(root);

			await nav.Navigation.PopAsync(new FadeTransition());

			Assert.False(PageTransitionResolver.HasPending(root));
			Assert.Empty(handler.Resolved);
		}

		[Fact]
		public async Task UnconsumedArgumentIsClearedWhenTheNavigationEnds()
		{
			// No platform runs this modal, so nothing consumes the staged transition.
			var root = new ContentPage();
			_ = new TestWindow(root);
			var modal = new ContentPage();

			await root.Navigation.PushModalAsync(modal, new FadeTransition());

			Assert.False(PageTransitionResolver.HasPending(modal));
			Assert.Same(PageTransition.Platform, PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, modal, true).Transition);
		}

		[Fact]
		public void ClearingAnOldStagingKeepsANewerOne()
		{
			var page = new ContentPage();
			var first = PageTransitionResolver.SetPending(page, NavigationTransitionKind.Push, new FadeTransition());
			PageTransitionResolver.SetPending(page, NavigationTransitionKind.Push, new ScaleTransition());

			PageTransitionResolver.ClearPending(page, first);

			Assert.True(PageTransitionResolver.HasPending(page));
			Assert.IsType<ScaleTransition>(PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, page, true).Transition);
		}

		[Fact]
		public async Task FailedNavigationClearsItsArgument()
		{
			var (nav, _) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			var page = new ContentPage();
			await nav.PushAsync(page);

			// Pushing a page that is already on the stack throws.
			await Assert.ThrowsAnyAsync<Exception>(() => nav.Navigation.PushAsync(page, new FadeTransition()));

			Assert.False(PageTransitionResolver.HasPending(page));
		}

		// ---- Modals ----

		[Fact]
		public void ModalUsesModalTransitionNotTransition()
		{
			var modal = new ContentPage();
			PageTransitions.SetTransition(modal, new SlideTransition());
			PageTransitions.SetModalTransition(modal, new FadeTransition());

			var push = PageTransitionResolver.Resolve(NavigationTransitionKind.PushModal, new ContentPage(), modal, true);

			Assert.IsType<FadeTransition>(push.Transition);
		}

		[Fact]
		public void ModalWithoutModalTransitionUsesThePlatform()
		{
			var modal = new ContentPage();
			PageTransitions.SetTransition(modal, new SlideTransition());

			var push = PageTransitionResolver.Resolve(NavigationTransitionKind.PushModal, new ContentPage(), modal, true);

			Assert.Same(PageTransition.Platform, push.Transition);
		}

		[Fact]
		public void ModalFallsBackToTheWindow()
		{
			var root = new ContentPage();
			var window = new TestWindow(root);
			PageTransitions.SetModalTransition(window, new ScaleTransition());

			var push = PageTransitionResolver.Resolve(NavigationTransitionKind.PushModal, root, root, true);

			Assert.IsType<ScaleTransition>(push.Transition);
		}

		[Fact]
		public void ModalDismissReversesThePresentation()
		{
			var modal = new ContentPage();
			PageTransitionResolver.SetPending(modal, NavigationTransitionKind.PushModal, new SlideTransition { From = SlideEdge.Bottom });

			PageTransitionResolver.Resolve(NavigationTransitionKind.PushModal, null, modal, true);
			var pop = PageTransitionResolver.Resolve(NavigationTransitionKind.PopModal, modal, null, true);

			Assert.True(pop.Request.IsBack);
			Assert.Equal(SlideEdge.Bottom, Assert.IsType<SlideTransition>(pop.Transition).From);
		}

		// ---- Shell ----

		[Fact]
		public void GoToAsyncOverrideAppliesToShellPagesOnly()
		{
			var shell = new Shell();
			var root = new ContentPage();
			shell.Items.Add(new ShellContent { Content = root });

			var token = PageTransitionResolver.BeginShellOverride(shell, new ScaleTransition());
			try
			{
				var inside = PageTransitionResolver.Resolve(NavigationTransitionKind.Push, root, root, true);
				Assert.IsType<ScaleTransition>(inside.Transition);
			}
			finally
			{
				PageTransitionResolver.EndShellOverride(shell, token);
			}

			var after = PageTransitionResolver.Resolve(NavigationTransitionKind.Push, root, root, true);
			Assert.Same(PageTransition.Platform, after.Transition);
		}

		[Fact]
		public void ASecondGoToAsyncReplacesTheFirstOverride()
		{
			var shell = new Shell();
			var root = new ContentPage();
			shell.Items.Add(new ShellContent { Content = root });

			var first = PageTransitionResolver.BeginShellOverride(shell, new FadeTransition());
			var second = PageTransitionResolver.BeginShellOverride(shell, new ScaleTransition());

			// The first navigation finishing must not remove the override of the one still running.
			PageTransitionResolver.EndShellOverride(shell, first);
			Assert.IsType<ScaleTransition>(PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, root, true).Transition);

			PageTransitionResolver.EndShellOverride(shell, second);
			Assert.Same(PageTransition.Platform, PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, root, true).Transition);
		}

		[Fact]
		public async Task NavigationPagesInSeveralWindowsDontConsultShellCurrent()
		{
			// With two windows and no Shell, Shell.Current can't pick one; an ordinary push must not depend on it.
			var app = new TestApp();
			Application.Current = app;
			try
			{
				var (first, firstHandler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
				var (second, _) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
				app.AddWindow(first.Window);
				app.AddWindow(second.Window);

				var page = new ContentPage();
				PageTransitions.SetTransition(page, new FadeTransition());
				await first.PushAsync(page).WaitAsync(TimeSpan.FromSeconds(5));
				await first.PopAsync().WaitAsync(TimeSpan.FromSeconds(5));

				Assert.IsType<FadeTransition>(firstHandler.Resolved[0].Transition);
				Assert.IsType<FadeTransition>(firstHandler.Resolved[1].Transition);
			}
			finally
			{
				Application.Current = null;
			}
		}

		[Fact]
		public void AShellOverrideInAnotherWindowDoesntReachANavigationPage()
		{
			var shellRoot = new ContentPage();
			var shell = new Shell();
			shell.Items.Add(new ShellContent { Content = shellRoot });
			var app = new TestApp();
			Application.Current = app;
			var token = PageTransitionResolver.BeginShellOverride(shell, new ScaleTransition());
			try
			{
				app.AddWindow(new Window(shell));
				Assert.Same(shell, Shell.Current);

				// A page popped off an unrelated NavigationPage is already detached from it.
				var root = new ContentPage();
				_ = new NavigationPage(root);
				var detached = new ContentPage();

				var push = PageTransitionResolver.Resolve(NavigationTransitionKind.Push, root, detached, true);
				var pop = PageTransitionResolver.Resolve(NavigationTransitionKind.Pop, detached, root, true);

				Assert.Same(PageTransition.Platform, push.Transition);
				Assert.Same(PageTransition.Platform, pop.Transition);
			}
			finally
			{
				PageTransitionResolver.EndShellOverride(shell, token);
				Application.Current = null;
			}
		}

		[Fact]
		public void GoToAsyncOverrideReachesARoutedModalAndAStackInsideIt()
		{
			var shell = new Shell();
			shell.Items.Add(new ShellContent { Content = new ContentPage() });
			var window = new Window(shell);

			// Modals are children of the window, not of the Shell that routed to them.
			var modal = new ContentPage { Parent = window };
			var nestedRoot = new ContentPage();
			var modalNavigation = new NavigationPage(nestedRoot) { Parent = window };
			var nested = new ContentPage();

			var token = PageTransitionResolver.BeginShellOverride(shell, new ScaleTransition());
			try
			{
				Assert.IsType<ScaleTransition>(PageTransitionResolver.Resolve(NavigationTransitionKind.PushModal, null, modal, true).Transition);
				Assert.IsType<ScaleTransition>(PageTransitionResolver.Resolve(NavigationTransitionKind.Push, nested, nestedRoot, true).Transition);
			}
			finally
			{
				PageTransitionResolver.EndShellOverride(shell, token);
			}
		}

		[Fact]
		public void GoToAsyncOverrideDoesntReachAModalInAnotherWindow()
		{
			var shell = new Shell();
			shell.Items.Add(new ShellContent { Content = new ContentPage() });
			_ = new Window(shell);
			var otherWindow = new Window(new ContentPage());
			var modal = new ContentPage { Parent = otherWindow };

			var token = PageTransitionResolver.BeginShellOverride(shell, new ScaleTransition());
			try
			{
				Assert.Same(PageTransition.Platform, PageTransitionResolver.Resolve(NavigationTransitionKind.PushModal, null, modal, true).Transition);
			}
			finally
			{
				PageTransitionResolver.EndShellOverride(shell, token);
			}
		}

		[Fact]
		public void AFailedLaterGoToAsyncDoesntEraseASuspendedOne()
		{
			var shell = new Shell();
			var root = new ContentPage();
			shell.Items.Add(new ShellContent { Content = root });

			// The first navigation is suspended (deferred in Navigating) while a second one runs and fails.
			var first = PageTransitionResolver.BeginShellOverride(shell, new FadeTransition());
			var second = PageTransitionResolver.BeginShellOverride(shell, new ScaleTransition());
			PageTransitionResolver.EndShellOverride(shell, second);

			try
			{
				Assert.IsType<FadeTransition>(PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, root, true).Transition);
			}
			finally
			{
				PageTransitionResolver.EndShellOverride(shell, first);
			}

			Assert.Same(PageTransition.Platform, PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, root, true).Transition);
		}

		[Fact]
		public async Task OverlappingGoToAsyncRequestsKeepTheirOwnTransitions()
		{
			// Two requests on one Shell, each suspended inside its own navigation (its transition in force): each must
			// resolve its own pages with its own transition, whichever began last.
			var shell = new Shell();
			var root = new ContentPage();
			shell.Items.Add(new ShellContent { Content = root });
			var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var releaseSecond = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

			async Task<PageTransition> Navigate(PageTransition transition, Task release)
			{
				using var scope = new ShellTransitionScope(shell, transition);
				await release;
				return PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, root, true).Transition;
			}

			var first = Navigate(new FadeTransition(), releaseFirst.Task);
			var second = Navigate(new ScaleTransition(), releaseSecond.Task);

			// Neither request's transition leaks out of its own flow.
			Assert.Same(PageTransition.Platform, PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, root, true).Transition);

			releaseFirst.SetResult(true);
			Assert.IsType<FadeTransition>(await first.WaitAsync(TimeSpan.FromSeconds(5)));

			releaseSecond.SetResult(true);
			Assert.IsType<ScaleTransition>(await second.WaitAsync(TimeSpan.FromSeconds(5)));
		}

		[Fact]
		public async Task WorkStartedInsideAGoToAsyncDoesntInheritItsTransitionAfterItEnds()
		{
			var shell = new Shell();
			var root = new ContentPage();
			shell.Items.Add(new ShellContent { Content = root });
			var later = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			Task<PageTransition> handler;

			using (new ShellTransitionScope(shell, new FadeTransition()))
			{
				// Something the navigation starts (a handler) that navigates after the navigation has ended.
				handler = ResolveLater();
			}

			later.SetResult(true);
			Assert.Same(PageTransition.Platform, await handler.WaitAsync(TimeSpan.FromSeconds(5)));

			async Task<PageTransition> ResolveLater()
			{
				await later.Task;
				return PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, root, true).Transition;
			}
		}

		[Fact]
		public async Task AShellPushAppliedLaterOutsideTheRequestKeepsItsTransition()
		{
			// Android queues a navigation behind the one running and applies it later, from a fragment callback outside the
			// GoToAsync's flow: the transition staged on the page when the section requested it must still resolve.
			var root = new ContentPage();
			var section = new ShellSection { Items = { new ShellContent { Content = root } } };
			var shell = new Shell { Items = { new ShellItem { Items = { section } } } };
			var platform = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			Page pushed = null;
			((IShellSectionController)section).NavigationRequested += (_, e) =>
			{
				pushed = e.Page;
				e.Task = platform.Task;
			};

			Routing.RegisterRoute("pagetransitions-later", typeof(ContentPage));
			try
			{
				var navigation = shell.GoToAsync("pagetransitions-later", new ScaleTransition());

				// Resolved from this test's own flow, which the request's scope never reached.
				Assert.NotNull(pushed);
				Assert.IsType<ScaleTransition>(PageTransitionResolver.Resolve(NavigationTransitionKind.Push, root, pushed, true).Transition);

				platform.SetResult(true);
				await navigation.WaitAsync(TimeSpan.FromSeconds(5));
				Assert.False(PageTransitionResolver.HasPending(pushed));
			}
			finally
			{
				Routing.UnRegisterRoute("pagetransitions-later");
			}
		}

		[Fact]
		public async Task AnUnconsumedStagedShellTransitionIsClearedWhenTheNavigationEnds()
		{
			var root = new ContentPage();
			var section = new ShellSection { Items = { new ShellContent { Content = root } } };
			var shell = new Shell { Items = { new ShellItem { Items = { section } } } };
			var platform = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			Page pushed = null;
			((IShellSectionController)section).NavigationRequested += (_, e) =>
			{
				pushed = e.Page;
				e.Task = platform.Task;
			};

			Routing.RegisterRoute("pagetransitions-unconsumed", typeof(ContentPage));
			try
			{
				var navigation = shell.GoToAsync("pagetransitions-unconsumed", new ScaleTransition());

				Assert.True(PageTransitionResolver.HasPending(pushed));
				platform.SetResult(true);
				await navigation.WaitAsync(TimeSpan.FromSeconds(5));
				await Task.Yield();

				Assert.False(PageTransitionResolver.HasPending(pushed));
			}
			finally
			{
				Routing.UnRegisterRoute("pagetransitions-unconsumed");
			}
		}

		[Fact]
		public void AUserNavigationInsideAGoToAsyncIsItsOwnRequest()
		{
			// Navigation.PushAsync goes through Shell's navigation manager as a request of its own: started inside
			// another request's flow (a handler of it), it doesn't take that request's transition.
			var root = new ContentPage();
			var section = new ShellSection { Items = { new ShellContent { Content = root } } };
			var shell = new Shell { Items = { new ShellItem { Items = { section } } } };
			var page = new ContentPage();
			var staged = true;
			((IShellSectionController)section).NavigationRequested += (_, e) => staged = PageTransitionResolver.HasPending(e.Page);

			using (new ShellTransitionScope(shell, new ScaleTransition()))
			{
				_ = section.Navigation.PushAsync(page);
			}

			Assert.False(staged);
		}

		[Fact]
		public void ARunningRootReplacementEndsWhenAnotherPageIsAssignedDirectly()
		{
			var page = new ContentPage();
			var window = new Window(new ContentPage());
			var finished = 0;
			var running = RootPageTransition.Track(window, page, () => finished++);

			// Its own assignment doesn't end it.
			window.Page = page;
			Assert.Equal(0, finished);
			Assert.False(running.IsSuperseded);

			window.Page = new ContentPage();
			Assert.Equal(1, finished);
			Assert.True(running.IsSuperseded);

			// It stopped observing the window: later changes don't finish it again.
			window.Page = new ContentPage();
			Assert.Equal(1, finished);
		}

		[Fact]
		public void ARunningRootReplacementEndsWhenTheWindowIsDestroyed()
		{
			var page = new ContentPage();
			var window = new Window(new ContentPage());
			var finished = 0;
			var running = RootPageTransition.Track(window, page, () => finished++);

			((IWindow)window).Destroying();

			Assert.Equal(1, finished);
			Assert.True(running.IsSuperseded);
		}

		[Fact]
		public void AFinishedRootReplacementStopsObservingTheWindow()
		{
			var page = new ContentPage();
			var window = new Window(new ContentPage());
			var finished = 0;
			var running = RootPageTransition.Track(window, page, () => finished++);

			RootPageTransition.Untrack(window, running);
			window.Page = new ContentPage();
			((IWindow)window).Destroying();

			Assert.Equal(0, finished);
		}

		[Fact]
		public void ARequestWithoutATransitionMasksTheRequestItWasStartedFrom()
		{
			// A GoToAsync started from inside another (its Navigated handler runs before that request's scope ends) has
			// no transition of its own: it must not take the enclosing request's.
			var shell = new Shell();
			var root = new ContentPage();
			shell.Items.Add(new ShellContent { Content = root });

			using (new ShellTransitionScope(shell, new FadeTransition()))
			{
				using (new ShellTransitionScope(shell, null))
				{
					Assert.Same(PageTransition.Platform, PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, root, true).Transition);
					Assert.Null(PageTransitionResolver.StageShellOverride(shell, root, NavigationTransitionKind.Push));
				}

				// The enclosing request's transition is back once the inner one ends.
				Assert.IsType<FadeTransition>(PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, root, true).Transition);
			}
		}

		[Fact]
		public void AMaskOnlyHidesItsOwnShellsRequests()
		{
			var shell = new Shell();
			var root = new ContentPage();
			shell.Items.Add(new ShellContent { Content = root });
			var other = new Shell();

			using (new ShellTransitionScope(shell, new FadeTransition()))
			using (new ShellTransitionScope(other, null))
			{
				Assert.IsType<FadeTransition>(PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, root, true).Transition);
			}
		}

		sealed class ContextDroppingDispatcher : Microsoft.Maui.Dispatching.IDispatcher, Microsoft.Maui.Dispatching.IDispatcherProvider
		{
			// Runs every callback in the execution context captured when the dispatcher was made, as a dispatcher that
			// posts a bare callback (Android's Handler.Post) does: nothing of the caller's async flow comes along.
			readonly System.Threading.ExecutionContext _clean = System.Threading.ExecutionContext.Capture();

			public bool IsDispatchRequired => false;

			public bool Dispatch(Action action)
			{
				System.Threading.ExecutionContext.Run(_clean.CreateCopy(), _ => action(), null);
				return true;
			}

			public bool DispatchDelayed(TimeSpan delay, Action action) => Dispatch(action);

			public Microsoft.Maui.Dispatching.IDispatcherTimer CreateTimer() => throw new NotSupportedException();

			public Microsoft.Maui.Dispatching.IDispatcher GetForCurrentThread() => this;
		}

		[Fact]
		public async Task AGoToAsyncTransitionCrossesADispatchThatDoesntFlowTheContext()
		{
			Microsoft.Maui.Dispatching.DispatcherProvider.SetCurrent(new ContextDroppingDispatcher());

			var root = new ContentPage();
			var content = new ShellContent { Route = "content", Content = root };
			var section = new ShellSection { Route = "section", Items = { content } };
			var item = new ShellItem { Route = "item", Items = { section } };
			var shell = new Shell { Items = { item } };
			_ = new TestWindow(shell);

			// The detail was shown with its own Fade; going back to the root with Scale must run Scale.
			var detail = new ContentPage();
			PageTransitions.SetTransition(detail, new FadeTransition());
			await section.Navigation.PushAsync(detail);

			PageTransition popped = null;
			((IShellSectionController)section).NavigationRequested += (_, e) =>
			{
				if (e.RequestType is Microsoft.Maui.Controls.Internals.NavigationRequestType.Pop or Microsoft.Maui.Controls.Internals.NavigationRequestType.PopToRoot)
				{
					popped = PageTransitionResolver.Resolve(NavigationTransitionKind.Pop, detail, root, true).Transition;
				}
			};

			// An absolute route back to the root of the current section: Shell dispatches the section's navigation.
			await shell.GoToAsync("//item/section/content", new ScaleTransition()).WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Same(root, shell.CurrentPage);
			Assert.IsType<ScaleTransition>(popped);
		}

		[Fact]
		public void AnEarlierAssignmentFinishingDoesntEndAReplacementQueuedBehindIt()
		{
			// ReplacePageAsync(C) started from Window.PropertyChanging for B: C's assignment is queued behind B's, whose
			// change is raised (for B) before C's lands.
			var b = new ContentPage();
			var c = new ContentPage();
			var window = new Window(new ContentPage());
			var finished = 0;
			RootPageTransition.RunningTransition running = null;
			Microsoft.Maui.Controls.PropertyChangingEventHandler onChanging = null;
			onChanging = (_, e) =>
			{
				if (e.PropertyName == Window.PageProperty.PropertyName)
				{
					window.PropertyChanging -= onChanging;
					running = RootPageTransition.Track(window, c, () => finished++);
					window.Page = c;
				}
			};
			window.PropertyChanging += onChanging;
			var assigned = RootPageTransition.WhenAssignedAsync(window, c);

			window.Page = b;

			Assert.Same(c, window.Page);
			Assert.Equal(0, finished);
			Assert.False(running.IsSuperseded);
			Assert.True(assigned.IsCompleted);

			// Once its own page is in, a direct assignment still ends it.
			window.Page = new ContentPage();
			Assert.Equal(1, finished);
		}

		[Fact]
		public void WaitingForAnAssignmentIgnoresAnotherPagesChange()
		{
			var page = new ContentPage();
			var window = new Window(new ContentPage());
			var assigned = RootPageTransition.WhenAssignedAsync(window, page);

			window.Page = new ContentPage();
			Assert.False(assigned.IsCompleted);

			window.Page = page;
			Assert.True(assigned.IsCompleted);
		}

		[Fact]
		public async Task ADeferredModalKeepsItsPerNavigationTransition()
		{
			// The window has no platform yet, so the modal is queued for the sync to present later.
			var root = new ContentPage();
			var window = new TestWindow(root);
			var modal = new ContentPage();

			await root.Navigation.PushModalAsync(modal, new FadeTransition());

			Assert.IsType<FadeTransition>(window.ModalNavigationManager.GetDeferredTransition(modal));
		}

		[Fact]
		public async Task RootReplacementTakesTheWindowsFlowDirection()
		{
			var window = new TestWindow(new ContentPage()) { FlowDirection = FlowDirection.RightToLeft };
			var page = new ContentPage();

			await window.ReplacePageAsync(page, new SlideTransition());

			// Recorded before the page joined the window, from the direction it lays out in once it has.
			Assert.Equal(FlowDirection.RightToLeft, PageTransitionResolver.GetShownFlowDirection(page));
		}

		[Fact]
		public void ProvidersRunOnlyWhenThePlatformResultIsRead()
		{
			var provider = new RecordingProvider(_ => PlatformNavigationTransition.Suppress);
			var root = new ContentPage();
			var (nav, _) = PageTransitionTestHelpers.CreateNavigationPage(root, provider);
			var prepared = PageTransitionResolver.Prepare(NavigationTransitionKind.Push, null, root, true);

			// An override that owns the animation never reads it, so no provider runs.
			Assert.Empty(provider.Requests);
			Assert.False(prepared.HasResolvedPlatform);

			Assert.True(prepared.Platform.IsSuppress);
			Assert.True(prepared.Platform.IsSuppress);
			Assert.Single(provider.Requests);
		}

		[Fact]
		public async Task AFailedSecondRequestDoesntTakeAQueuedRequestsTransition()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			handler.HoldTransitions = true;
			var running = nav.PushAsync(new ContentPage());
			await handler.WaitForHoldAsync().WaitAsync(TimeSpan.FromSeconds(5));

			// Queued behind the running push, this one stages Fade for the page...
			var detail = new ContentPage();
			var queued = nav.Navigation.PushAsync(detail, new FadeTransition());

			// ...and a second request for the same page fails before it is consumed.
			await Assert.ThrowsAnyAsync<Exception>(() => nav.Navigation.PushAsync(detail, new ScaleTransition()));

			handler.HoldTransitions = false;
			handler.FinishTransition();
			await running.WaitAsync(TimeSpan.FromSeconds(5));
			await queued.WaitAsync(TimeSpan.FromSeconds(5));

			Assert.IsType<FadeTransition>(handler.Last.Transition);
			Assert.Same(detail, handler.Last.Request.To);
		}

		[Fact]
		public async Task ADeferredRoutedModalKeepsTheGoToAsyncTransition()
		{
			var shell = new Shell();
			shell.Items.Add(new ShellContent { Content = new ContentPage() });
			var window = new TestWindow(shell);
			var modal = new ContentPage();

			// GoToAsync's transition is in force while the routed modal is pushed, and gone once GoToAsync ends.
			using (new ShellTransitionScope(shell, new ScaleTransition()))
			{
				await window.Navigation.PushModalAsync(modal);
			}

			Assert.IsType<ScaleTransition>(window.ModalNavigationManager.GetDeferredTransition(modal));
		}

		[Fact]
		public void TabTransitionNearestSettingWins()
		{
			var content = new ShellContent { Content = new ContentPage() };
			var section = new ShellSection { Items = { content } };
			var item = new ShellItem { Items = { section } };
			var shell = new Shell { Items = { item } };

			PageTransitions.SetTabTransition(shell, new FadeTransition());
			Assert.IsType<FadeTransition>(PageTransitionResolver.ResolveTab(content, null, null, false).Transition);

			PageTransitions.SetTabTransition(section, new ScaleTransition());
			Assert.IsType<ScaleTransition>(PageTransitionResolver.ResolveTab(content, null, null, false).Transition);

			PageTransitions.SetTabTransition(content, new SlideTransition());
			var resolved = PageTransitionResolver.ResolveTab(content, null, null, reverse: true);
			Assert.IsType<SlideTransition>(resolved.Transition);
			Assert.Equal(NavigationTransitionKind.TabSwitch, resolved.Request.Kind);
			Assert.True(resolved.Request.IsBack);
		}

		// ---- Reduced motion ----

		[Fact]
		public async Task ReducedMotionRunsNoneAndSkipsProviders()
		{
			var provider = new RecordingProvider(_ => PlatformNavigationTransition.Platform);
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage(), provider);
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new SlideTransition());
			PageTransitionResolver.ReducedMotionOverride = true;

			await nav.PushAsync(page);

			Assert.Same(PageTransition.None, handler.Last.Transition);
			Assert.True(handler.Last.IsSuppressed);
			Assert.Empty(provider.Requests);
		}

		[Fact]
		public async Task ReducedMotionLeavesThePlatformAnimationToThePlatform()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			PageTransitionResolver.ReducedMotionOverride = true;

			await nav.PushAsync(new ContentPage());

			Assert.Same(PageTransition.Platform, handler.Last.Transition);
			Assert.True(handler.Last.IsPlatformDefault);
		}

		// ---- Completion ----

		[Fact]
		public async Task NavigationCompletesAndRaisesNavigatedToAfterTheTransition()
		{
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage());
			handler.HoldTransitions = true;
			var page = new ContentPage();
			var navigatedTo = 0;
			page.NavigatedTo += (_, _) => navigatedTo++;

			var push = nav.Navigation.PushAsync(page, new FadeTransition());
			await handler.WaitForHoldAsync().WaitAsync(TimeSpan.FromSeconds(5));

			Assert.False(push.IsCompleted);
			Assert.Equal(0, navigatedTo);

			handler.FinishTransition();
			await push.WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Equal(1, navigatedTo);
			Assert.Same(page, nav.CurrentPage);
		}

		// ---- Arguments ----

		[Fact]
		public async Task OverloadsRejectNull()
		{
			var nav = new ContentPage().Navigation;
			await Assert.ThrowsAsync<ArgumentNullException>(() => nav.PushAsync(null, new FadeTransition()));
			await Assert.ThrowsAsync<ArgumentNullException>(() => nav.PushAsync(new ContentPage(), (PageTransition)null));
			await Assert.ThrowsAsync<ArgumentNullException>(() => nav.PopAsync((PageTransition)null));
			await Assert.ThrowsAsync<ArgumentNullException>(() => ((Window)null).ReplacePageAsync(new ContentPage(), new FadeTransition()));
			Assert.Throws<ArgumentNullException>(() => PageTransitions.GetTransition(null));
		}

		[Fact]
		public async Task ReplacePageAsyncSetsThePageAndRecordsTheTransition()
		{
			var window = new TestWindow(new ContentPage());
			var next = new ContentPage();

			await window.ReplacePageAsync(next, new FadeTransition());

			Assert.Same(next, window.Page);
			Assert.IsType<FadeTransition>(PageTransitionResolver.GetShownTransition(next));
		}
	}

	[CollectionDefinition(Name, DisableParallelization = true)]
	public class PageTransitionCollection
	{
		public const string Name = "PageTransitions";
	}
}
