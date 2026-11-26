using System;
using Microsoft.Maui.Controls.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	public class SharedTransitionTests : BaseTestFixture
	{
		internal static (ContentPage From, ContentPage To) PagesWith(string fromName, string toName)
		{
			var source = new BoxView();
			SharedTransition.SetName(source, fromName);
			var target = new BoxView();
			SharedTransition.SetName(target, toName);
			return (new ContentPage { Content = new VerticalStackLayout { source } }, new ContentPage { Content = new Grid { target } });
		}

		static ContentPage ListPage(params (string Name, string Group, BoxView View)[] items)
		{
			var layout = new VerticalStackLayout();
			foreach (var (name, group, view) in items)
			{
				SharedTransition.SetName(view, name);
				SharedTransition.SetGroup(view, group);
				layout.Add(view);
			}

			return new ContentPage { Content = layout };
		}

		static ContentPage DetailPage(string name, string group, BoxView view)
		{
			SharedTransition.SetName(view, name);
			var page = new ContentPage { Content = new ScrollView { Content = view } };
			SharedTransition.SetGroup(page, group);
			return page;
		}

		[Fact]
		public void TheCrossfadeHeroesFlyOverIsWhatThePopReversesEvenWithoutAMatch()
		{
			PageTransitionResolver.ReducedMotionOverride = false;
			try
			{
				var (list, detail) = PagesWith("photo", "photo");

				// Only shared-element names are set: the push crossfades so the hero has something to fly over.
				var push = PageTransitionResolver.Resolve(NavigationTransitionKind.Push, list, detail, true);
				Assert.IsType<FadeTransition>(BuiltInNavigationTransitions.GetEffectiveTransition(push.Request, out var pushHeroes));
				Assert.Single(pushHeroes);

				// The list page's hero is hidden before going back: no pair, but the pop still reverses the crossfade.
				((VisualElement)((VerticalStackLayout)list.Content)[0]).IsVisible = false;
				var pop = PageTransitionResolver.Resolve(NavigationTransitionKind.Pop, detail, list, true);

				Assert.IsType<FadeTransition>(pop.Transition);
				Assert.IsType<FadeTransition>(BuiltInNavigationTransitions.GetEffectiveTransition(pop.Request, out var popHeroes));
				Assert.Empty(popHeroes);
			}
			finally
			{
				PageTransitionResolver.ReducedMotionOverride = null;
			}
		}

		[Fact]
		public void ElementsWithTheSameNameArePaired()
		{
			var (from, to) = PagesWith("photo", "photo");

			var pairs = SharedTransition.FindPairs(from, to);

			var pair = Assert.Single(pairs);
			Assert.Same(((VerticalStackLayout)from.Content)[0], pair.From);
			Assert.Same(((Grid)to.Content)[0], pair.To);
		}

		[Fact]
		public void DifferentNamesAreNotPaired()
		{
			var (from, to) = PagesWith("photo", "avatar");

			Assert.Empty(SharedTransition.FindPairs(from, to));
		}

		[Fact]
		public void NullOrSamePagesHaveNoPairs()
		{
			var (from, to) = PagesWith("photo", "photo");

			Assert.Empty(SharedTransition.FindPairs(null, to));
			Assert.Empty(SharedTransition.FindPairs(from, null));
			Assert.Empty(SharedTransition.FindPairs(from, from));
		}

		[Fact]
		public void HiddenElementsDontTakePart()
		{
			var (from, to) = PagesWith("photo", "photo");
			((VisualElement)((VerticalStackLayout)from.Content)[0]).IsVisible = false;

			Assert.Empty(SharedTransition.FindPairs(from, to));
		}

		[Fact]
		public void ElementsInsideAHiddenLayoutDontTakePart()
		{
			var (from, to) = PagesWith("photo", "photo");
			((VisualElement)from.Content).IsVisible = false;

			Assert.Empty(SharedTransition.FindPairs(from, to));
		}

		[Fact]
		public void GroupPicksTheListItemGoingForward()
		{
			var a = new BoxView();
			var b = new BoxView();
			var c = new BoxView();
			var list = ListPage(("photo", "1", a), ("photo", "2", b), ("photo", "3", c));
			var target = new BoxView();
			var detail = DetailPage("photo", "2", target);

			var pair = Assert.Single(SharedTransition.FindPairs(list, detail));

			Assert.Same(b, pair.From);
			Assert.Same(target, pair.To);
		}

		[Fact]
		public void GroupPicksTheListItemGoingBack()
		{
			var a = new BoxView();
			var b = new BoxView();
			var list = ListPage(("photo", "1", a), ("photo", "2", b));
			var source = new BoxView();
			var detail = DetailPage("photo", "1", source);

			var pair = Assert.Single(SharedTransition.FindPairs(detail, list));

			Assert.Same(source, pair.From);
			Assert.Same(a, pair.To);
		}

		[Fact]
		public void ListWithoutAMatchingGroupDoesntMorph()
		{
			var list = ListPage(("photo", "1", new BoxView()), ("photo", "2", new BoxView()));
			var detail = DetailPage("photo", "9", new BoxView());

			Assert.Empty(SharedTransition.FindPairs(list, detail));
		}

		[Fact]
		public void ListWithoutADetailGroupDoesntMorph()
		{
			var list = ListPage(("photo", "1", new BoxView()), ("photo", "2", new BoxView()));
			var detail = DetailPage("photo", null, new BoxView());

			Assert.Empty(SharedTransition.FindPairs(list, detail));
		}

		[Fact]
		public void TwoListItemsInTheSameGroupAreAmbiguous()
		{
			var list = ListPage(("photo", "2", new BoxView()), ("photo", "2", new BoxView()));
			var detail = DetailPage("photo", "2", new BoxView());

			Assert.Empty(SharedTransition.FindPairs(list, detail));
		}

		[Fact]
		public void ListsOnBothSidesAreAmbiguous()
		{
			var left = ListPage(("photo", "1", new BoxView()), ("photo", "2", new BoxView()));
			var right = ListPage(("photo", "1", new BoxView()), ("photo", "2", new BoxView()));
			SharedTransition.SetGroup(left, "1");
			SharedTransition.SetGroup(right, "1");

			Assert.Empty(SharedTransition.FindPairs(left, right));
		}

		[Fact]
		public void SeveralNamesPairIndependently()
		{
			var photo = new BoxView();
			var title = new Label();
			SharedTransition.SetName(photo, "photo");
			SharedTransition.SetName(title, "title");
			var from = new ContentPage { Content = new VerticalStackLayout { photo, title } };

			var photo2 = new BoxView();
			var title2 = new Label();
			var unrelated = new Label();
			SharedTransition.SetName(photo2, "photo");
			SharedTransition.SetName(title2, "title");
			SharedTransition.SetName(unrelated, "price");
			var to = new ContentPage { Content = new VerticalStackLayout { title2, unrelated, photo2 } };

			var pairs = SharedTransition.FindPairs(from, to);

			Assert.Equal(2, pairs.Count);
			Assert.Contains(pairs, p => ReferenceEquals(p.From, photo) && ReferenceEquals(p.To, photo2));
			Assert.Contains(pairs, p => ReferenceEquals(p.From, title) && ReferenceEquals(p.To, title2));
		}

		[Fact]
		public void AccessorsRejectNull()
		{
			Assert.Throws<ArgumentNullException>(() => SharedTransition.GetName(null));
			Assert.Throws<ArgumentNullException>(() => SharedTransition.SetGroup(null, "x"));
		}
	}
}
