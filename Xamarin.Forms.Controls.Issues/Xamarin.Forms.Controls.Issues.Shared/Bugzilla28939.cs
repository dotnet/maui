using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls.Issues
{
	[Issue (IssueTracker.Bugzilla, 28939, " Entry Control loses cursor position to either beginning or end of input ",
		PlatformAffected.WinPhone)]
	public class Bugzilla28939 : TestContentPage
	{
		protected override void Init ()
		{
			Content = new StackLayout {
				Children = {
					new Label {
						Text = @"Enter the text ""testing"" in the Entry Control below. Move the cursor between the 'e' and the 's'. Type the letter 'a'. If the cursor is positioned after the 'a', the test has passed."
					},
					new Entry()
				}
			};
		}
	}
}
