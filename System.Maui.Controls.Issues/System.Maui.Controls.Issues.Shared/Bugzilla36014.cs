using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 36014, "Picker Control Is Not Rendered Correctly", PlatformAffected.WinPhone)]
	public class Bugzilla36014 : TestContentPage
	{
		protected override void Init()
		{
			var picker = new Picker { Items = { "Leonardo", "Donatello", "Raphael", "Michaelangelo" } };
			var label = new Label
			{
				Text =
					"This test is successful if the picker below spans the width of the screen. If the picker is " 
					+ "just a sliver on the left edge of the screen, this test has failed."
			};

			Content = new StackLayout { Children = { label, picker } };
		}
	}
}
