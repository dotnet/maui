using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43783, "[WP8.1] Most Device Styles do not render correctly in Windows Phone 8.1 (RT) applications", PlatformAffected.WinPhone)]
	public class Bugzilla43783 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Device";

			Content = new StackLayout
			{
				Children = {
					new Label { Margin = new Thickness(10), Text = "The Labels for Body, Caption, List Item, List Item Detail, and No Style should not all look the same. If all of them look the same, this test has failed."},
					new Label { Text = "Title style", Style = Device.Styles.TitleStyle },
					new Label { Text = "Subtitle style", Style = Device.Styles.SubtitleStyle },
					new Label { Text = "Body style", Style = Device.Styles.BodyStyle },
					new Label { Text = "Caption style", Style = Device.Styles.CaptionStyle },
					new Label { Text = "List item text style", Style = Device.Styles.ListItemTextStyle },
					new Label { Text = "List item detail text style", Style = Device.Styles.ListItemDetailTextStyle },
					new Label { Text = "No style" }
				}
			};
		}

	}
}