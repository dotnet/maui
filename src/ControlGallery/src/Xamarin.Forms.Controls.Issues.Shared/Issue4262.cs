using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4262, "Label HorizontalTextAlignment=\"Center\" not working in conjunction with LineHeight on iOS", PlatformAffected.iOS)]
	public class Issue4262 : ContentPage
	{
		public Issue4262()
		{
			var label = new Label() { Text = "This is center aligned&#x0a;line 2.", HorizontalTextAlignment = TextAlignment.Center };
			var label2 = new Label() { Text = "If this is not center aligned, this test has failed.", HorizontalTextAlignment = TextAlignment.Center, LineHeight = 1.5 };

			Content = new StackLayout()
			{
				Children = { label, label2 },
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
		}
	}
}