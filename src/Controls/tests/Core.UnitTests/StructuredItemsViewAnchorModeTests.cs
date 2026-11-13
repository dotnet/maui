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

		[Fact]
		public void TurningAnchoringOffClearsIsAnchored()
		{
			var collectionView = new CollectionView { AnchorMode = ScrollAnchorMode.End };
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

		[Fact]
		public void AnAnchorModeThatIncludesEndKeepsIsAnchored()
		{
			var collectionView = new CollectionView { AnchorMode = ScrollAnchorMode.End };
			collectionView.SetIsAnchored(true);

			// A flag this version doesn't define yet, alongside End.
			collectionView.AnchorMode = ScrollAnchorMode.End | (ScrollAnchorMode)1;

			Assert.True(collectionView.IsAnchored);
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
		public void ScrollToEndAsksThePlatformView(bool animate)
		{
			var collectionView = new CollectionView { ItemsSource = new[] { "a", "b", "c" } };
			var handler = new ScrollToEndHandlerStub(canScrollToEnd: true);
			collectionView.Handler = handler;
			var requests = RecordScrollToRequests(collectionView);

			collectionView.ScrollToEnd(animate);

			Assert.Equal(new[] { animate }, handler.ScrollsToEnd);
			Assert.Empty(requests);
		}

		[Fact]
		public void ScrollToEndScrollsToTheLastItemWhereThePlatformViewCant()
		{
			var collectionView = new CollectionView { ItemsSource = new[] { "a", "b", "c" } };
			var handler = new ScrollToEndHandlerStub(canScrollToEnd: false);
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

		sealed class ScrollToEndHandlerStub : ItemsViewHandlerStub, IScrollToEndHandler
		{
			readonly bool _canScrollToEnd;

			public ScrollToEndHandlerStub(bool canScrollToEnd)
			{
				_canScrollToEnd = canScrollToEnd;
			}

			public List<bool> ScrollsToEnd { get; } = new();

			bool IScrollToEndHandler.ScrollToEnd(bool animate)
			{
				ScrollsToEnd.Add(animate);
				return _canScrollToEnd;
			}
		}
	}
}
