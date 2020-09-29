using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class SwipeViewTests : BaseTestFixture
	{
		[Test]
		public void TestConstructor()
		{
			var swipeView = new SwipeView();

			Assert.AreEqual(0, swipeView.LeftItems.Count);
			Assert.AreEqual(0, swipeView.TopItems.Count);
			Assert.AreEqual(0, swipeView.RightItems.Count);
			Assert.AreEqual(0, swipeView.BottomItems.Count);
		}

		[Test]
		public void TestDefaultSwipeItems()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				Text = "Text"
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItem
			};

			Assert.AreEqual(SwipeMode.Reveal, swipeView.LeftItems.Mode);
			Assert.AreEqual(SwipeBehaviorOnInvoked.Auto, swipeView.LeftItems.SwipeBehaviorOnInvoked);
		}

		[Test]
		public void TestSwipeItemsExecuteMode()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				Text = "Text"
			};

			var swipeItems = new SwipeItems
			{
				Mode = SwipeMode.Execute
			};

			swipeItems.Add(swipeItem);

			swipeView.LeftItems = swipeItems;

			Assert.AreEqual(SwipeMode.Execute, swipeView.LeftItems.Mode);
		}

		[Test]
		public void TestSwipeItemsSwipeBehaviorOnInvoked()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				Text = "Text"
			};

			var swipeItems = new SwipeItems
			{
				SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close
			};

			swipeItems.Add(swipeItem);

			swipeView.LeftItems = swipeItems;

			Assert.AreEqual(SwipeBehaviorOnInvoked.Close, swipeView.LeftItems.SwipeBehaviorOnInvoked);
		}

		[Test]
		public void TestLeftItems()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				Text = "Text"
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItem
			};

			Assert.AreNotEqual(0, swipeView.LeftItems.Count);
		}

		[Test]
		public void TestRightItems()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				Text = "Text"
			};

			swipeView.RightItems = new SwipeItems
			{
				swipeItem
			};

			Assert.AreNotEqual(0, swipeView.RightItems.Count);
		}

		[Test]
		public void TestTopItems()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				Text = "Text"
			};

			swipeView.TopItems = new SwipeItems
			{
				swipeItem
			};

			Assert.AreNotEqual(0, swipeView.TopItems.Count);
		}

		[Test]
		public void TestBottomItems()
		{
			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				Text = "Text"
			};

			swipeView.BottomItems = new SwipeItems
			{
				swipeItem
			};

			Assert.AreNotEqual(0, swipeView.BottomItems.Count);
		}

		[Test]
		public void TestProgramaticallyOpen()
		{
			bool isOpen = false;

			var swipeView = new SwipeView();

			swipeView.OpenRequested += (sender, args) =>
			{
				isOpen = true;
			};

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				Text = "Text"
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItem
			};

			swipeView.Open(OpenSwipeItem.LeftItems);

			Assert.IsTrue(isOpen);
		}

		[Test]
		public void TestProgramaticallyClose()
		{
			bool isOpen = false;

			var swipeView = new SwipeView();

			swipeView.OpenRequested += (sender, args) => isOpen = true;
			swipeView.CloseRequested += (sender, args) => isOpen = false;

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				Text = "Text"
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItem
			};

			swipeView.Open(OpenSwipeItem.LeftItems);

			swipeView.Close();

			Assert.IsFalse(isOpen);
		}

		[Test]
		public void TestSwipeItemView()
		{
			var swipeView = new SwipeView();

			var swipeItemViewContent = new Grid();
			swipeItemViewContent.BackgroundColor = Color.Red;

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
			Assert.AreNotEqual(0, swipeView.LeftItems.Count);
		}
	}
}