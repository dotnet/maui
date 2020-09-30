using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9210, "[Bug] iOS keyboard case flickers when switching entries", PlatformAffected.iOS)]
	public class Issue9210 : TestContentPage
	{
		const string IssueInstructions = "1) Tap into one of the Entries.\n2) Tap into the other Entry.\n3) If things are working, the keyboard should not flicker.";

		protected override void Init()
		{
			var stackLayout = new StackLayout();
			stackLayout.Children.Add(new Label() { Text = IssueInstructions });
			stackLayout.Children.Add(new Entry() { Text = "Some demo text." });
			stackLayout.Children.Add(new Entry() { Text = "Some other demo text." });

			Content = stackLayout;
		}
	}
}