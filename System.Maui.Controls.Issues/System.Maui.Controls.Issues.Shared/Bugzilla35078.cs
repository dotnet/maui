using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls.Issues
{
	[Issue (IssueTracker.Bugzilla, 35078,
		"Checking IsInvokeRequired on WinRT when off the dispatcher thread causes a null reference exception",
		PlatformAffected.WinRT)]
	public class Bugzilla35078 : TestContentPage
	{
		protected override void Init ()
		{
			var button = new Button { Text = "Go" };

			var instructions = new Label {
				Text =
					"Click the 'Go' button. If the application crashes or a label with the text 'Sucess' does not appear, the test has failed."
			};

			var success = new Label ();

			button.Clicked += (sender, args) => {
				Task.Run (() => {
					bool invokeRequired = Device.IsInvokeRequired;
					if (invokeRequired) {
						Device.BeginInvokeOnMainThread (() => success.Text = "Success");
					}
				});
			};

			Content = new StackLayout { Children = { instructions, button, success } };
		}
	}
}
