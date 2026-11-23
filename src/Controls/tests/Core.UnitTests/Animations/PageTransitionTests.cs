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
