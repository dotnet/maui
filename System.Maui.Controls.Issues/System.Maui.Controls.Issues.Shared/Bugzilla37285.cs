using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 37285, "Possible to enter text into Picker control", PlatformAffected.iOS)]
	public class Bugzilla37285 : TestContentPage
	{
		const string Instructions = "On iOS, focus the Picker below and type with a hardware keyboard. If text appears in the Picker text view, this test has failed. Note that Windows will allow you to select items with the keyboard, but the text you type will not appear in the text view. Also note that Android will allow you to select an item using the arrow and enter keys, but again, no text will appear in the text view.";

		protected override void Init()
		{
			var picker = new Picker { ItemsSource = Enumerable.Range(0, 100).Select(c => c.ToString()).ToList() };

			var stack = new StackLayout { Children = { new Label { Text = Instructions }, picker } };

			Content = stack;
		}
	}
}