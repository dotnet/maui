using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Frame)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11430,
		"[Bug] [iOS] Button stays in Pressed state if the touch-up event occurs outside",
		PlatformAffected.iOS)]
	public class Issue11430 : TestContentPage
	{
		public Issue11430()
		{
		}

		protected override void Init()
		{
			Title = "Issue 11430";

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Tap a Button, drag your finger outside the Button and lift up your finger. If the Button state is Normal, the test has passed."
			};

			var button = new Button
			{
				Text = "Click Me"
			};

			layout.Children.Add(instructions);
			layout.Children.Add(button);

			Content = layout;

			button.Clicked += (sender, args) =>
			{
				DisplayAlert("Issue 11430", "Button Clicked.", "Ok");
			};
		}
	}
}
