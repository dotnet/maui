using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;

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
		public void TestSwipeViewBindingContextChangedEvent()
		{
			var swipeView = new SwipeView();
			bool passed = false;
			swipeView.BindingContextChanged += (sender, args) => passed = true;

			swipeView.BindingContext = new object();

			if (!passed)
			{
				throw new XunitException("The BindingContextChanged event was not fired.");
			}
		}

		[Fact]
		public void TestContentBindingContextChangedEvent()
		{
			var content = new Label();
			var swipeView = new SwipeView
			{
				Content = content
			};

			bool passed = false;
			content.BindingContextChanged += (sender, args) => passed = true;

			swipeView.BindingContext = new object();

			if (!passed)
			{
				throw new XunitException("The BindingContextChanged event was not fired.");
			}
		}

		[Fact]
		public void TestTemplatedContentBindingContextChangedEvent()
		{
			var content = new Label();
			var swipeView = new SwipeView();

			swipeView.ControlTemplate = new ControlTemplate(() => content);

			bool passed = false;
			content.BindingContextChanged += (sender, args) => passed = true;

			swipeView.BindingContext = new object();

			if (!passed)
			{
				throw new XunitException("The BindingContextChanged event was not fired.");
			}
		}

		[Fact]
		public void ClearRemovesLogicalChildren()
		{
			var swipeView = new SwipeView();

			swipeView.LeftItems = new SwipeItems
			{
				new SwipeItem(),
				new SwipeItem(),
				new SwipeItem(),
				new SwipeItem(),
				new SwipeItem()
			};

			swipeView.LeftItems.Clear();
			Assert.Empty((swipeView.LeftItems as IVisualTreeElement).GetVisualChildren());
		}

		[Fact]
		public void TestContentBindingContextPropagatesToNewSwipeItems()
		{
			var swipeView = new SwipeView();

			swipeView.LeftItems = new SwipeItems
			{
				new SwipeItem()
			};
			swipeView.RightItems = new SwipeItems
			{
				new SwipeItem()
			};
			swipeView.TopItems = new SwipeItems
			{
				new SwipeItem()
			};
			swipeView.BottomItems = new SwipeItems
			{
				new SwipeItem()
			};

			swipeView.BindingContext = new object();
			Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.LeftItems[0]).BindingContext);
			Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.RightItems[0]).BindingContext);
			Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.TopItems[0]).BindingContext);
			Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.BottomItems[0]).BindingContext);
		}

		[Fact]
		public void TestContentBindingContextPropagatesToPassedInSwipeItem()
		{
			var swipeView = new SwipeView();

			swipeView.LeftItems = new SwipeItems(new[] { new SwipeItem() });

			swipeView.BindingContext = new object();
			Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.LeftItems[0]).BindingContext);
		}

		[Fact]
		public void TestContentBindingContextPropagatesToAddedSwipeItems()
		{
			var swipeView = new SwipeView();

			swipeView.LeftItems.Add(new SwipeItem());
			swipeView.RightItems.Add(new SwipeItem());
			swipeView.TopItems.Add(new SwipeItem());
			swipeView.BottomItems.Add(new SwipeItem());

			swipeView.BindingContext = new object();
			Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.LeftItems[0]).BindingContext);
			Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.RightItems[0]).BindingContext);
			Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.TopItems[0]).BindingContext);
			Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.BottomItems[0]).BindingContext);
		}

		[Fact]
		public void BindingContextTransfersToNewSetOfSwipeItems()
		{
			var bc1 = new object();
			var bc2 = new object();

			var swipeView = new SwipeView();
			var leftItems = swipeView.LeftItems;
			var swipeItem = new SwipeItem();
			leftItems.Add(swipeItem);
			swipeView.BindingContext = bc1;

			Assert.Equal(leftItems.BindingContext, bc1);
			Assert.Equal(swipeItem.BindingContext, bc1);

			Assert.Equal(leftItems, swipeView.LeftItems);
			Assert.Contains(leftItems, (swipeView as IVisualTreeElement).GetVisualChildren());
			Assert.Equal(swipeItem, (leftItems as IVisualTreeElement).GetVisualChildren()[0]);

			var leftItems2 = new SwipeItems();
			leftItems2.Add(swipeItem);
			swipeView.LeftItems = leftItems2;

			// now that this isn't the logical child of SwipeItems the parent should be null
			Assert.Null(leftItems.Parent);

			// The parent on swipeItem should now be leftItems2 because that's been set on SwipeView
			Assert.NotSame(swipeItem.Parent, leftItems);
			Assert.NotSame(leftItems.Parent, swipeView);

			Assert.Equal(swipeItem.Parent, leftItems2);
			Assert.Equal(leftItems2.Parent, swipeView);

			swipeView.BindingContext = bc2;
			Assert.Equal(leftItems2.BindingContext, bc2);
			Assert.Equal(swipeItem.BindingContext, bc2);
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
