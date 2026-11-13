using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class StructuredItemsViewAnchorModeTests : BaseTestFixture
	{
		[Fact]
		public void AnchorModeIsNoneAndNotAnchoredByDefault()
		{
			var collectionView = new CollectionView();

			Assert.Equal(ScrollAnchorMode.None, collectionView.AnchorMode);
			Assert.False(collectionView.IsAnchored);
		}

		[Fact]
		public void SettingAnchorModeDoesNotAnchorAViewWithoutAPlatformView()
		{
			var collectionView = new CollectionView { AnchorMode = ScrollAnchorMode.End };

			Assert.Equal(ScrollAnchorMode.End, collectionView.AnchorMode);
			Assert.False(collectionView.IsAnchored);
		}

		[Fact]
		public void IsAnchoredIsReadOnly()
		{
			var collectionView = new CollectionView();

			// Writing a read-only property is logged and ignored.
			collectionView.SetValue(StructuredItemsView.IsAnchoredProperty, true);

			Assert.True(StructuredItemsView.IsAnchoredProperty.IsReadOnly);
			Assert.False(collectionView.IsAnchored);
		}

		[Theory]
		[InlineData(ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.End)]
		[InlineData(ScrollAnchorMode.Start | ScrollAnchorMode.End)]
		public void TurningAnchoringOffClearsIsAnchored(ScrollAnchorMode mode)
		{
			var collectionView = new CollectionView { AnchorMode = mode };
			collectionView.SetIsAnchored(true);

			var changes = 0;
			collectionView.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == StructuredItemsView.IsAnchoredProperty.PropertyName)
				{
					changes++;
				}
			};

			collectionView.AnchorMode = ScrollAnchorMode.None;

			Assert.False(collectionView.IsAnchored);
			Assert.Equal(1, changes);
		}

		[Theory]
		[InlineData(ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.End)]
		[InlineData(ScrollAnchorMode.Start | ScrollAnchorMode.End)]
		public void AnAnchorModeThatIncludesAnEdgeKeepsIsAnchored(ScrollAnchorMode mode)
		{
			var collectionView = new CollectionView { AnchorMode = ScrollAnchorMode.End };
			collectionView.SetIsAnchored(true);

			// The platform view reports where it is once it starts over with the new mode.
			collectionView.AnchorMode = mode;

			Assert.True(collectionView.IsAnchored);
		}

		[Fact]
		public void AnAnchorModeWithOnlyFlagsThisVersionDoesNotDefineClearsIsAnchored()
		{
			var collectionView = new CollectionView { AnchorMode = ScrollAnchorMode.End };
			collectionView.SetIsAnchored(true);

			collectionView.AnchorMode = (ScrollAnchorMode)4;

			Assert.False(collectionView.IsAnchored);
		}

		[Fact]
		public void AnchorModeCanBeBound()
		{
			var collectionView = new CollectionView { BindingContext = new { Mode = ScrollAnchorMode.End } };

			collectionView.SetBinding(StructuredItemsView.AnchorModeProperty, "Mode");

			Assert.Equal(ScrollAnchorMode.End, collectionView.AnchorMode);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void ScrollToStartAsksThePlatformView(bool animate)
		{
			var collectionView = new CollectionView { ItemsSource = new[] { "a", "b", "c" } };
			var handler = new ScrollToEdgeHandlerStub(canScrollToEdge: true);
			collectionView.Handler = handler;
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToStart(animate);

			Assert.Equal(new[] { animate }, handler.ScrollsToStart);
			Assert.Empty(handler.ScrollsToEnd);
			Assert.Empty(requests);
		}

		[Fact]
		public void ScrollToStartScrollsToTheFirstItemWhereThePlatformViewCant()
		{
			var collectionView = new CollectionView { ItemsSource = new[] { "a", "b", "c" } };
			var handler = new ScrollToEdgeHandlerStub(canScrollToEdge: false);
			collectionView.Handler = handler;
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToStart(animate: false);

			Assert.Equal(new[] { false }, handler.ScrollsToStart);
			var request = Assert.Single(requests);
			Assert.Equal(ScrollToMode.Position, request.Mode);
			Assert.Equal(0, request.Index);
			Assert.Equal(-1, request.GroupIndex);
			Assert.Equal(ScrollToPosition.Start, request.ScrollToPosition);
			Assert.False(request.IsAnimated);
		}

		[Fact]
		public void ScrollToStartScrollsToTheFirstItemOfAnySequenceWhereThePlatformViewCant()
		{
			// Such as on Windows: its handler doesn't scroll to the start of the content.
			var collectionView = new CollectionView { ItemsSource = Enumerable.Range(0, 5).Select(index => index * 2) };
			collectionView.Handler = new ItemsViewHandlerStub();
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToStart();

			var request = Assert.Single(requests);
			Assert.Equal(0, request.Index);
			Assert.True(request.IsAnimated);
		}

		[Fact]
		public void ScrollToStartWithoutAHandlerDoesNothing()
		{
			var collectionView = new CollectionView { ItemsSource = new[] { "a", "b", "c" } };
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToStart();

			Assert.Empty(requests);
		}

		[Fact]
		public void ScrollToStartOfAnEmptyListDoesNothing()
		{
			var collectionView = new CollectionView { ItemsSource = new List<string>() };
			collectionView.Handler = new ItemsViewHandlerStub();
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToStart();

			Assert.Empty(requests);
		}

		[Fact]
		public void ScrollToStartOfAGroupedListScrollsToTheFirstItemOfTheFirstGroupWithItems()
		{
			var collectionView = new CollectionView
			{
				IsGrouped = true,
				ItemsSource = new List<List<string>> { new(), new() { "a", "b" }, new() { "c" } },
			};
			collectionView.Handler = new ItemsViewHandlerStub();
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToStart(animate: false);

			var request = Assert.Single(requests);
			Assert.Equal(0, request.Index);
			Assert.Equal(1, request.GroupIndex);
			Assert.Equal(ScrollToPosition.Start, request.ScrollToPosition);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void ScrollToEndAsksThePlatformView(bool animate)
		{
			var collectionView = new CollectionView { ItemsSource = new[] { "a", "b", "c" } };
			var handler = new ScrollToEdgeHandlerStub(canScrollToEdge: true);
			collectionView.Handler = handler;
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToEnd(animate);

			Assert.Equal(new[] { animate }, handler.ScrollsToEnd);
			Assert.Empty(handler.ScrollsToStart);
			Assert.Empty(requests);
		}

		[Fact]
		public void ScrollToEndScrollsToTheLastItemWhereThePlatformViewCant()
		{
			var collectionView = new CollectionView { ItemsSource = new[] { "a", "b", "c" } };
			var handler = new ScrollToEdgeHandlerStub(canScrollToEdge: false);
			collectionView.Handler = handler;
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToEnd(animate: false);

			Assert.Equal(new[] { false }, handler.ScrollsToEnd);
			var request = Assert.Single(requests);
			Assert.Equal(ScrollToMode.Position, request.Mode);
			Assert.Equal(2, request.Index);
			Assert.Equal(-1, request.GroupIndex);
			Assert.Equal(ScrollToPosition.End, request.ScrollToPosition);
			Assert.False(request.IsAnimated);
		}

		[Fact]
		public void ScrollToEndScrollsToTheLastItemOfAnySequenceWhereThePlatformViewCant()
		{
			// Such as on Windows: its handler doesn't scroll to the end of the content.
			var collectionView = new CollectionView { ItemsSource = Enumerable.Range(0, 5).Select(index => index * 2) };
			collectionView.Handler = new ItemsViewHandlerStub();
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToEnd();

			var request = Assert.Single(requests);
			Assert.Equal(4, request.Index);
			Assert.True(request.IsAnimated);
		}

		[Fact]
		public void ScrollToEndWithoutAHandlerDoesNothing()
		{
			// As ScrollTo: there's nothing to scroll yet.
			var collectionView = new CollectionView { ItemsSource = new[] { "a", "b", "c" } };
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToEnd();

			Assert.Empty(requests);
		}

		[Fact]
		public void ScrollToEndOfAnEmptyListDoesNothing()
		{
			var collectionView = new CollectionView { ItemsSource = new List<string>() };
			collectionView.Handler = new ItemsViewHandlerStub();
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToEnd();

			Assert.Empty(requests);
		}

		[Fact]
		public void ScrollToEndOfAGroupedListScrollsToTheLastItemOfTheLastGroupWithItems()
		{
			var collectionView = new CollectionView
			{
				IsGrouped = true,
				ItemsSource = new List<List<string>> { new() { "a", "b" }, new() { "c", "d", "e" }, new() },
			};
			collectionView.Handler = new ItemsViewHandlerStub();
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToEnd(animate: false);

			var request = Assert.Single(requests);
			Assert.Equal(2, request.Index);
			Assert.Equal(1, request.GroupIndex);
			Assert.Equal(ScrollToPosition.End, request.ScrollToPosition);
		}

		static List<ScrollToRequestEventArgs> RecordScrollToRequests(ItemsView itemsView)
		{
			var requests = new List<ScrollToRequestEventArgs>();
			itemsView.ScrollToRequested += (_, e) => requests.Add(e);
			return requests;
		}

		class ItemsViewHandlerStub : ViewHandler<CollectionView, object>
		{
			public ItemsViewHandlerStub() : base(new PropertyMapper<IView>())
			{
			}

			protected override object CreatePlatformView() => new();
		}

		sealed class ScrollToEdgeHandlerStub : ItemsViewHandlerStub, IScrollToEdgeHandler
		{
			readonly bool _canScrollToEdge;

			public ScrollToEdgeHandlerStub(bool canScrollToEdge)
			{
				_canScrollToEdge = canScrollToEdge;
			}

			public List<bool> ScrollsToStart { get; } = new();

			public List<bool> ScrollsToEnd { get; } = new();

			bool IScrollToEdgeHandler.ScrollToStart(bool animate)
			{
				ScrollsToStart.Add(animate);
				return _canScrollToEdge;
			}

			bool IScrollToEdgeHandler.ScrollToEnd(bool animate)
			{
				ScrollsToEnd.Add(animate);
				return _canScrollToEdge;
			}
		}
	}
}
