using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1025, "StackLayout broken when image missing", PlatformAffected.iOS, NavigationBehavior.PushModalAsync)]
	public class Issue1025 : ContentPage
	{
		public Issue1025()
		{
			var instructions = new Label
			{
				Text = "The StackLayout below should contain two buttons and some text. " +
				"If the StackLayout appears empty, this test has failed."
			};

			var layout = new StackLayout
			{
				BackgroundColor = Color.FromHex("#dae1eb"),
				Orientation = StackOrientation.Vertical,
				Children = {
					new Image {},
					new Label {Text = "Lorem ipsum dolor" },
					new Label {Text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."},
					new Button {BackgroundColor = Color.FromHex ("#fec240"), Text = "Create an account" },
					new Button {BackgroundColor = Color.FromHex ("#04acdb"), Text = "Login" },
				}
			};

			Content = new StackLayout
			{
				Children = { instructions, layout }
			};
		}
	}
}
