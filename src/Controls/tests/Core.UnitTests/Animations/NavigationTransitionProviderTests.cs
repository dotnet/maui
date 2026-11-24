using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	[Collection(PageTransitionCollection.Name)]
	public class NavigationTransitionProviderTests : BaseTestFixture
	{
		public NavigationTransitionProviderTests()
		{
			PageTransitionResolver.ReducedMotionOverride = false;
		}

		protected override void Dispose(bool disposing)
		{
			PageTransitionResolver.ReducedMotionOverride = null;
			PageTransitionResolver.BuiltInOverride = null;
			base.Dispose(disposing);
		}

		[Fact]
		public async Task ProvidersAreConsultedInRegistrationOrderAndTheFirstAnswerWins()
		{
			var first = new RecordingProvider(_ => PlatformNavigationTransition.Suppress);
			var second = new RecordingProvider(_ => PlatformNavigationTransition.Platform);
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage(), first, second);

			await nav.PushAsync(new ContentPage());

			Assert.Single(first.Requests);
			Assert.Empty(second.Requests);
			Assert.True(handler.Last.IsSuppressed);
		}

		[Fact]
		public async Task DecliningPassesToTheNextProvider()
		{
			var declining = new RecordingProvider(_ => null);
			var handling = new RecordingProvider(_ => PlatformNavigationTransition.Suppress);
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage(), declining, handling);

			await nav.PushAsync(new ContentPage());

			Assert.Single(declining.Requests);
			Assert.Single(handling.Requests);
			Assert.True(handler.Last.IsSuppressed);
		}

		[Fact]
		public async Task PlatformStopsTheChainBeforeTheBuiltIns()
		{
			var builtIn = new RecordingProvider(_ => PlatformNavigationTransition.Suppress);
			PageTransitionResolver.BuiltInOverride = builtIn;
			var keepPlatform = new RecordingProvider(_ => PlatformNavigationTransition.Platform);
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage(), keepPlatform);
			var page = new ContentPage();
			PageTransitions.SetTransition(page, new FadeTransition());

			await nav.PushAsync(page);

			Assert.True(handler.Last.IsPlatformDefault);
			Assert.Empty(builtIn.Requests);
		}

		[Fact]
		public async Task WhenEveryProviderDeclinesTheBuiltInsRun()
		{
			var builtIn = new RecordingProvider(_ => PlatformNavigationTransition.Suppress);
			PageTransitionResolver.BuiltInOverride = builtIn;
			var declining = new RecordingProvider(_ => null);
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage(), declining);

			await nav.PushAsync(new ContentPage());

			Assert.Single(declining.Requests);
			Assert.Single(builtIn.Requests);
			Assert.True(handler.Last.IsSuppressed);
		}

		[Fact]
		public async Task ProvidersReceiveTheResolvedTransitionAndBothPages()
		{
			var provider = new RecordingProvider(_ => null);
			var root = new ContentPage();
			var (nav, _) = PageTransitionTestHelpers.CreateNavigationPage(root, provider);
			var page = new ContentPage();

			await nav.Navigation.PushAsync(page, new SlideTransition { From = SlideEdge.Top });
			await nav.PopAsync();

			var push = provider.Requests[0];
			Assert.Equal(NavigationTransitionKind.Push, push.Kind);
			Assert.Same(root, push.From);
			Assert.Same(page, push.To);
			Assert.False(push.IsBack);
			Assert.False(push.IsInteractive);
			Assert.Equal(SlideEdge.Top, Assert.IsType<SlideTransition>(push.Transition).From);

			var pop = provider.Requests[1];
			Assert.Equal(NavigationTransitionKind.Pop, pop.Kind);
			Assert.Same(page, pop.From);
			Assert.Same(root, pop.To);
			Assert.True(pop.IsBack);
			Assert.Equal(SlideEdge.Top, Assert.IsType<SlideTransition>(pop.Transition).From);
		}

		[Fact]
		public async Task ProvidersAreConsultedForThePlatformDefault()
		{
			// A library can animate pages that set nothing.
			var provider = new RecordingProvider(_ => PlatformNavigationTransition.Suppress);
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage(), provider);

			await nav.PushAsync(new ContentPage());

			Assert.Same(PageTransition.Platform, provider.Requests[0].Transition);
			Assert.True(handler.Last.IsSuppressed);
		}

		[Fact]
		public async Task AnExplicitNoAnimationNeverReachesAProvider()
		{
			var provider = new RecordingProvider(_ => PlatformNavigationTransition.Platform);
			var (nav, handler) = PageTransitionTestHelpers.CreateNavigationPage(new ContentPage(), provider);

			await nav.Navigation.PushAsync(new ContentPage(), false);

			Assert.Empty(provider.Requests);
			Assert.True(handler.Last.IsSuppressed);
		}

		[Fact]
		public void RequestFlowDirectionFollowsThePageShown()
		{
			var page = new ContentPage { FlowDirection = FlowDirection.RightToLeft };
			_ = new TestWindow(page);

			var resolved = PageTransitionResolver.Resolve(NavigationTransitionKind.Push, null, page, true);

			Assert.Equal(FlowDirection.RightToLeft, resolved.Request.FlowDirection);
		}

		// ---- Built-in transitions ----

		[Fact]
		public void BuiltInsKeepThePlatformAnimationWhenNothingIsSet()
		{
			var request = PageTransitionResolver.Resolve(NavigationTransitionKind.Push, new ContentPage(), new ContentPage(), true).Request;

			Assert.Null(BuiltInNavigationTransitions.GetEffectiveTransition(request, out var heroes));
			Assert.Empty(heroes);
		}

		[Fact]
		public void BuiltInsCrossfadeWhenOnlySharedElementsAreSet()
		{
			var (from, to) = SharedTransitionTests.PagesWith("photo", "photo");
			var request = PageTransitionResolver.Resolve(NavigationTransitionKind.Push, from, to, true).Request;

			var transition = BuiltInNavigationTransitions.GetEffectiveTransition(request, out var heroes);

			Assert.IsType<FadeTransition>(transition);
			Assert.Single(heroes);
		}

		[Fact]
		public void SharedElementsDontFlyUnderReducedMotion()
		{
			PageTransitionResolver.ReducedMotionOverride = true;
			var (from, to) = SharedTransitionTests.PagesWith("photo", "photo");
			PageTransitions.SetTransition(to, new SlideTransition());
			var request = PageTransitionResolver.Resolve(NavigationTransitionKind.Push, from, to, true).Request;

			Assert.Null(BuiltInNavigationTransitions.GetEffectiveTransition(request, out var heroes));
			Assert.Empty(heroes);
		}

		[Fact]
		public void OnlySharedElementsUnderReducedMotionKeepThePlatformTransition()
		{
			PageTransitionResolver.ReducedMotionOverride = true;
			var (from, to) = SharedTransitionTests.PagesWith("photo", "photo");
			var request = PageTransitionResolver.Resolve(NavigationTransitionKind.Push, from, to, true).Request;

			Assert.Null(BuiltInNavigationTransitions.GetEffectiveTransition(request, out var heroes));
			Assert.Empty(heroes);
		}

		[Fact]
		public void SharedElementsDontFlyForModals()
		{
			var (from, to) = SharedTransitionTests.PagesWith("photo", "photo");
			PageTransitions.SetModalTransition(to, new FadeTransition());
			var request = PageTransitionResolver.Resolve(NavigationTransitionKind.PushModal, from, to, true).Request;

			BuiltInNavigationTransitions.GetEffectiveTransition(request, out var heroes);

			Assert.Empty(heroes);
		}
		[Fact]
		public void APresetLooksForSharedElementsOnlyWhereItsPlatformFliesThem()
		{
			var (from, to) = SharedTransitionTests.PagesWith("photo", "photo");
			PageTransitions.SetTransition(to, new SlideTransition());
			var request = PageTransitionResolver.Resolve(NavigationTransitionKind.Push, from, to, true).Request;

			Assert.IsType<SlideTransition>(BuiltInNavigationTransitions.GetEffectiveTransition(request, out var flown));
			Assert.Single(flown);

			// WinUI hosts no flights: the preset runs without the pages being searched.
			Assert.IsType<SlideTransition>(BuiltInNavigationTransitions.GetEffectiveTransition(request, out var skipped, presetsCarryHeroes: false));
			Assert.Empty(skipped);
		}

		[Fact]
		public void SharedElementsStillTurnAPlatformNavigationIntoTheCrossfadeWhereNoPresetFliesThem()
		{
			var (from, to) = SharedTransitionTests.PagesWith("photo", "photo");
			var request = PageTransitionResolver.Resolve(NavigationTransitionKind.Push, from, to, true).Request;

			Assert.IsType<FadeTransition>(BuiltInNavigationTransitions.GetEffectiveTransition(request, out _, presetsCarryHeroes: false));
		}
	}
}
