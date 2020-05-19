using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Bugzilla, 21780, "Windows Phone Editor control has black background with black text, background turns to white when editing", PlatformAffected.WinPhone)]
	public class Bugzilla21780 : ContentPage
	{
		public Bugzilla21780 ()
		{
			var label = new Label () { Text = "If text is visible in the editor below, this test has passed." };
			var editor = new Editor () { Text = "This text should be visible even if the editor does not have focus" };

			Content = new StackLayout() {
				Children = { label, editor }
			};
		}
	}
}
