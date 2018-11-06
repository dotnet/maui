using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1025, "StackLayout broken when image missing", PlatformAffected.iOS, NavigationBehavior.PushModalAsync)]
	public class Issue1025 : ContentPage
	{
		public Issue1025 ()
		{
			BackgroundColor = Color.FromHex("#dae1eb");
			Content = new StackLayout {
				Orientation = StackOrientation.Vertical,
				Children = {
					new Image {},
					new Label {Text = "Lorem ipsum dolor" },
					new Label {Text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."},
					new Button {BackgroundColor = Color.FromHex ("#fec240"), Text = "Create an account" },
					new Button {BackgroundColor = Color.FromHex ("#04acdb"), Text = "Login" },
				}
			};
		}
	}	
}
