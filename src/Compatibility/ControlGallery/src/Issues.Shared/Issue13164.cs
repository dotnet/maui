using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Shape)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 13164, "[Bug] Shapes broken in Xamarin if used within NavigationPage once and navigated back",
		PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue13164 : TestContentPage
	{
		Issue13164SecondPage _issue13164SecondPage;

		public Issue13164()
		{
		}

		protected override void Init()
		{
			Title = "Issue 13164";

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Tap the Button to navigate to the details page with some Shapes."
			};

			var navigateButton = new Button
			{
				Text = "Navigate"
			};

			layout.Children.Add(instructions);
			layout.Children.Add(navigateButton);

			Content = layout;

			navigateButton.Clicked += (sender, args) =>
			{
				if (_issue13164SecondPage == null)
				{
					_issue13164SecondPage = new Issue13164SecondPage();
				}

				var navPage = new NavigationPage(_issue13164SecondPage);
				Navigation.PushAsync(navPage);
			};
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue13164SecondPage : ContentPage
	{
		public Issue13164SecondPage()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Navigate back, and navigate again to this page several times. If shapes are always rendered, the test has passed."
			};

			var ellipse = new Ellipse
			{
				HorizontalOptions = LayoutOptions.Start,
				Stroke = Brush.DarkBlue,
				Fill = Brush.BlueViolet,
				HeightRequest = 50,
				WidthRequest = 100
			};

			var rectangle = new Shapes.Rectangle
			{
				HorizontalOptions = LayoutOptions.Start,
				StrokeThickness = 3,
				Stroke = Brush.DarkOliveGreen,
				Fill = Brush.Orange,
				HeightRequest = 80,
				WidthRequest = 120
			};

			layout.Children.Add(instructions);
			layout.Children.Add(ellipse);
			layout.Children.Add(rectangle);

			Content = layout;
		}
	}
}