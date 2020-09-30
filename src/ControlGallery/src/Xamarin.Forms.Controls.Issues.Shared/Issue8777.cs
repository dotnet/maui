using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.SwipeView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8777, "SwipeView crash if Text not is set on SwipeItem", PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue8777 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 8777";

			var layout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var instructions = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Swipe to the right, if can open the SwipeView the test has passed."
			};

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				IconImageSource = "calculator.png"
			};

			swipeItem.Invoked += (sender, e) => { DisplayAlert("SwipeView", "Invoked", "Ok"); };

			var swipeItems = new SwipeItems { swipeItem };

			swipeItems.Mode = SwipeMode.Reveal;

			var swipeContent = new Grid
			{
				BackgroundColor = Color.Gray
			};

			var swipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right (No Text)"
			};

			swipeContent.Children.Add(swipeLabel);

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = swipeContent
			};

			layout.Children.Add(instructions);
			layout.Children.Add(swipeView);

			Content = layout;
		}
	}
}