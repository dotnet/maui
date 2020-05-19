using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 33890, "Setting CancelButtonColor does not have any effect", PlatformAffected.iOS)]
	public class Bugzilla33890 : TestContentPage
	{
		protected override void Init ()
		{
			Label header = new Label {
				Text = "Search Bar",
				FontAttributes = FontAttributes.Bold,
				FontSize = 50,
				HorizontalOptions = LayoutOptions.Center
			};

			SearchBar searchBar = new SearchBar {
				Placeholder = "Enter anything",
				CancelButtonColor = Color.Red
			};

			Label reproSteps = new Label {
				Text =
					"Tap on the search bar and enter some text. The 'Cancel' button should appear. If the 'Cancel' button is not red, this is broken.",
				HorizontalOptions = LayoutOptions.Center
			};

			Content = new StackLayout {
				Children = {
					header,
					searchBar,
					reproSteps
				}
			};
		}
	}
}