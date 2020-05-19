using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44584,
		"UWP - Editor: changing the background color will only take effect after the entry gained focus")]
	public class Bugzilla44584 : TestContentPage
	{
		protected override void Init()
		{
			var instructions = new Label
			{
				Text = @"
Tap the first button once to turn the Entry background color to Green. Tap the Entry to focus it; the background should remain green; if it does not, the test has failed. 
Tap the second button once to turn the Editor background color to Green. Tap the Editor to focus it; the background should remain green; if it does not, the test has failed." 
			};

			var entryButton = new Button { Text = "Toggle Entry Background (Green/Default)" };
			var entry = new Entry();

			entryButton.Clicked +=
				(sender, args) => { entry.BackgroundColor = entry.BackgroundColor != Color.Green ? Color.Green : Color.Default; };

			var editorButton = new Button { Text = "Toggle Editor Background (Green/Default)" };
			var editor = new Editor()
			{
				HeightRequest = 80
			};

			editorButton.Clicked +=
				(sender, args) => { editor.BackgroundColor = editor.BackgroundColor != Color.Green ? Color.Green : Color.Default; };

			var layout = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = { instructions, entryButton, entry, editorButton, editor }
			};

			Content = layout;
		}
	}
}
