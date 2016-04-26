using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 35127, "It is possible to craft a page such that it will never display on Windows")]
	public class Bugzilla35127 : TestContentPage
	{
		protected override void Init ()
		{
			Content = new StackLayout {
				Children = {
					new Label { Text = "See me?" },
					new ScrollView {
						IsVisible = false,
						Content = new Button { Text = "Click Me?" }
					}
				}
			};
		}
	}
}