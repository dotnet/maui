using System;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2294, "Scrolling view causes timer to stop incrementing", PlatformAffected.iOS)]
	public class Issue2294 : ContentPage
	{
		public Issue2294 ()
		{
			var labelUpdatedByTimer = new Label{ };
			var layout = new StackLayout {
				Children = {
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					labelUpdatedByTimer,
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
					new Label {Text = "lots of content to scroll"},
				},
			};
			var scroll = new ScrollView {
				Content = layout,
			};
			double counter = 0.0;
			Device.StartTimer (TimeSpan.FromSeconds (0.02), () => {
				counter += 0.02;
				labelUpdatedByTimer.Text = counter.ToString();
				return true;
			});
			Content = scroll;
		}
	}
}


