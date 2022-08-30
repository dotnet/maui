using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class SwipeViewTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			var swipeView = new SwipeView();

			Assert.Empty(swipeView.LeftItems);
			Assert.Empty(swipeView.TopItems);
			Assert.Empty(swipeView.RightItems);
			Assert.Empty(swipeView.BottomItems);
		}

		[Fact]
		public void TestDefaultSwipeItems()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "Text"
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItem
			};

			Assert.Equal(SwipeMode.Reveal, swipeView.LeftItems.Mode);
			Assert.Equal(SwipeBehaviorOnInvoked.Auto, swipeView.LeftItems.SwipeBehaviorOnInvoked);
		}

		[Fact]
		public void TestSwipeItemsExecuteMode()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "Text"
			};

			var swipeItems = new SwipeItems
			{
				Mode = SwipeMode.Execute
			};

			swipeItems.Add(swipeItem);

			swipeView.LeftItems = swipeItems;

			Assert.Equal(SwipeMode.Execute, swipeView.LeftItems.Mode);
		}

		[Fact]
		public void TestSwipeItemsSwipeBehaviorOnInvoked()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "Text"
			};

			var swipeItems = new SwipeItems
			{
				SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close
			};

			swipeItems.Add(swipeItem);

			swipeView.LeftItems = swipeItems;

			Assert.Equal(SwipeBehaviorOnInvoked.Close, swipeView.LeftItems.SwipeBehaviorOnInvoked);
		}

		[Fact]
		public void TestLeftItems()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "Text"
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItem
			};

			Assert.NotEmpty(swipeView.LeftItems);
		}

		[Fact]
		public void TestRightItems()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "Text"
			};

			swipeView.RightItems = new SwipeItems
			{
				swipeItem
			};

			Assert.NotEmpty(swipeView.RightItems);
		}

		[Fact]
		public void TestTopItems()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "Text"
			};

			swipeView.TopItems = new SwipeItems
			{
				swipeItem
			};

			Assert.NotEmpty(swipeView.TopItems);
		}

		[Fact]
		public void TestBottomItems()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "Text"
			};

			swipeView.BottomItems = new SwipeItems
			{
				swipeItem
			};

			Assert.NotEmpty(swipeView.BottomItems);
		}

		[Fact]
		public void TestProgrammaticallyOpen()
		{
			bool isOpen = false;

			var swipeView = new SwipeView();

			swipeView.OpenRequested += (sender, args) =>
			{
				isOpen = true;
			};

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "Text"
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItem
			};

			swipeView.Open(OpenSwipeItem.LeftItems);

			Assert.True(isOpen);
		}

		[Fact]
		public void TestProgrammaticallyClose()
		{
			bool isOpen = false;

			var swipeView = new SwipeView();

			swipeView.OpenRequested += (sender, args) => isOpen = true;
			swipeView.CloseRequested += (sender, args) => isOpen = false;

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "Text"
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItem
			};

			swipeView.Open(OpenSwipeItem.LeftItems);

			swipeView.Close();

			Assert.False(isOpen);
		}

		[Fact]
		public void TestSwipeItemView()
		{
			var swipeView = new SwipeView();

			var swipeItemViewContent = new Grid();
			swipeItemViewContent.BackgroundColor = Colors.Red;

			swipeItemViewContent.Children.Add(new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "SwipeItemView"
			});

			var swipeItemView = new SwipeItemView
			{
				Content = swipeItemViewContent
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItemView
			};

			Assert.NotNull(swipeItemView);
			Assert.NotNull(swipeItemView.Content);
			Assert.NotEmpty(swipeView.LeftItems);
		}
	}
}
