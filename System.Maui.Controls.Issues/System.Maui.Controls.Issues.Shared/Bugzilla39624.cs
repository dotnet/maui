using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 39624, "CarouselPage.Children Appear Out of Order", PlatformAffected.WinRT)]
	public class Bugzilla39624 : TestCarouselPage
	{
		protected override void Init ()
		{
			var instructions =
				"Flip through each page of the carousel from 1 to 5; the pages should display in order. Then flip backward to page 1; if any of the pages display out of order, the test has failed.";

			Children.Add (GeneratePage ("Page 1", Color.Red, instructions));
			Children.Add (GeneratePage ("Page 2", Color.Green, instructions));
			Children.Add (GeneratePage ("Page 3", Color.Blue, instructions));
			Children.Add (GeneratePage ("Page 4", Color.Purple, instructions));
			Children.Add (GeneratePage ("Page 5", Color.Black, instructions));

			CurrentPageChanged += (sender, args) => Debug.WriteLine (CurrentPage.Title);
		}

		ContentPage GeneratePage (string title, Color color, string instructions)
		{
			var page = new ContentPage {
				Content = new StackLayout {
					Children = {
						new Label { Text = title, FontSize = 24, TextColor = Color.White },
						new Label { Text = instructions, TextColor = Color.White }
					}
				},
				BackgroundColor = color,
				Title = title
			};

			return page;
		}
	}
}
