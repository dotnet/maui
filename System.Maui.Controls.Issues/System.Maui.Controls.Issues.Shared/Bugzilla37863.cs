using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 37863, "Password is readable when Entry.IsEnabled is false ",
		PlatformAffected.WinPhone)]
	public class Bugzilla37863 : TestContentPage
	{
		protected override void Init ()
		{
			var label = new Label {
				Text =
					"Click the button to toggle IsEnabled on the password entry below. The actual password text should never show. If the text shows, the test has failed."
			};
			var entry = new Entry { IsPassword = true, Text = "swordfish" };
			var button = new Button { Text = "Toggle IsEnabled" };

			button.Clicked += (sender, args) => { entry.IsEnabled = !entry.IsEnabled; };

			Content = new StackLayout {
				Children = { label, entry, button }
			};
		}
	}
}
