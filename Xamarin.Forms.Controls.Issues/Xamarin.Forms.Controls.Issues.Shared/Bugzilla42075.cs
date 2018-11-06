using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 42075, "IllegalStateException - Fragment does not have a view", PlatformAffected.Android)]
	public class Bugzilla42075 : TestTabbedPage
	{
		protected override void Init()
		{
			Title = "Outer";

			const string text = @"To run this test, you'll need to have an emulator or device in Developer mode, with the ""Don't Keep Activities"" setting turned on.
Hit the Home button to dismiss the application. Then bring up the Overview (recent apps) screen and select the Control Gallery.
If the application crashes with ""Java.Lang.IllegalStateException: Fragment does not have a view"", this test has failed. If the application does not crash or crashes with a different exception, this test has passed.";

			var directions = new ContentPage
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = text
						}
					}
				}
			};

			var tabbedPage = new TabbedPage() {Title = "Inner"};
			tabbedPage.Children.Add(new ContentPage());

			Children.Add(directions);
			Children.Add(tabbedPage);
		}
	}
}
