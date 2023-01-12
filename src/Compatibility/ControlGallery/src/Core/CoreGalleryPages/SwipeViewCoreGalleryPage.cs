using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	internal class SwipeViewCoreGalleryPage : CoreGalleryPage<SwipeView>
	{
		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override bool SupportsScroll
		{
			get { return false; }
		}

		protected override void InitializeElement(SwipeView element)
		{
			element.HeightRequest = 60;
			element.BackgroundColor = Colors.LightGray;
			element.Content = GetSwipeContent(SwipeDirection.Left);
			element.LeftItems = GetRevealSwipeItems();
			element.RightItems = GetExecuteSwipeItems();
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var rightItemsContainer = new ValueViewContainer<SwipeView>(Test.SwipeView.RightItems, new SwipeView { RightItems = GetRevealSwipeItems(), Content = GetSwipeContent(SwipeDirection.Right), HeightRequest = 60, BackgroundColor = Colors.LightPink }, "RightItems", value => value.ToString());
			var topItemsContainer = new ValueViewContainer<SwipeView>(Test.SwipeView.TopItems, new SwipeView { TopItems = GetRevealSwipeItems(), Content = GetSwipeContent(SwipeDirection.Up), HeightRequest = 60, BackgroundColor = Colors.LightSkyBlue }, "TopItems", value => value.ToString());
			var bottomItemsContainer = new ValueViewContainer<SwipeView>(Test.SwipeView.BottomItems, new SwipeView { BottomItems = GetRevealSwipeItems(), Content = GetSwipeContent(SwipeDirection.Down), HeightRequest = 60, BackgroundColor = Colors.LightGray }, "BottomItems", value => value.ToString());

			Add(rightItemsContainer);
			Add(topItemsContainer);
			Add(bottomItemsContainer);
		}

		internal SwipeItems GetRevealSwipeItems()
		{
			var addSwipeItem = new SwipeItem { BackgroundColor = Colors.Green, Text = "Add", IconImageSource = "coffee.png" };
			addSwipeItem.Invoked += (sender, e) =>
			{
				DisplayAlert("SwipeView", "Add Invoked", "OK");
			};

			var modifySwipeItem = new SwipeItem { BackgroundColor = Colors.Orange, Text = "Modify", IconImageSource = "calculator.png" };

			modifySwipeItem.Invoked += (sender, e) =>
			{
				DisplayAlert("SwipeView", "Modify Invoked", "OK");
			};

			var swipeItems = new SwipeItems
			{
				addSwipeItem,
				modifySwipeItem
			};

			swipeItems.Mode = SwipeMode.Reveal;

			return swipeItems;
		}

		internal SwipeItems GetExecuteSwipeItems()
		{
			var deleteSwipeItem = new SwipeItem { BackgroundColor = Colors.Red, Text = "Delete", IconImageSource = "coffee.png" };

			deleteSwipeItem.Invoked += (sender, e) =>
			{
				DisplayAlert("SwipeView", "Delete Invoked", "OK");
			};

			var swipeItems = new SwipeItems
			{
				deleteSwipeItem
			};

			swipeItems.Mode = SwipeMode.Execute;

			return swipeItems;
		}

		internal Grid GetSwipeContent(SwipeDirection swipeDirection)
		{
			var content = new Grid
			{
				BackgroundColor = Colors.LightGoldenrodYellow
			};

			var info = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			switch (swipeDirection)
			{
				case SwipeDirection.Down:
					info.Text = "Swipe to the Top";
					break;
				case SwipeDirection.Left:
					info.Text = "Swipe to the Right";
					break;
				case SwipeDirection.Right:
					info.Text = "Swipe to the Left";
					break;
				case SwipeDirection.Up:
					info.Text = "Swipe to the Bottom";
					break;
			}

			content.Children.Add(info);

			return content;
		}
	}
}