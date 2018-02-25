using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1878, "[UWP] Setting SearchBar.CancelButtonColor affects all SearchBars on page", PlatformAffected.UWP)]
	public class GitHub1878 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label 
			{ 
				Text = "The SearchBars below should have different cancel button colors. " 
						+ "If they each have the same cancel button color, this test has failed."
			};

			var sb1 = new SearchBar { Text = "This should have a red cancel button." };
			var sb2 = new SearchBar { Text = "This should have a blue cancel button." };

			sb1.CancelButtonColor = Color.Red;
			sb2.CancelButtonColor = Color.Blue;

			layout.Children.Add(instructions);
			layout.Children.Add(sb1);
			layout.Children.Add(sb2);

			Content = layout;
		}
	}
}