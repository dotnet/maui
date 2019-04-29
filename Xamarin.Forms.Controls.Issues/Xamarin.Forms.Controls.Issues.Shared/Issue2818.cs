using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2818, "Right-to-Left MasterDetail in Xamarin.Forms Hamburger icon issue", PlatformAffected.Android)]
	public class Issue2818 : MasterDetailPage
	{
		public Issue2818()
		{
			FlowDirection = FlowDirection.RightToLeft;

			Master = new ContentPage
			{
				Title = "Master",
				BackgroundColor = Color.SkyBlue,
				IconImageSource = "menuIcon"
			};

			Detail = new NavigationPage(new ContentPage
			{
				Title = "Detail",
				Content = new StackLayout
				{
					Children = {
						new Label
						{
							Text = "The page must be with RightToLeft FlowDirection. Hamburger icon in main page must be going to right side."
						},
						new Button
						{
							Text = "Set RightToLeft",
							Command = new Command(() => FlowDirection = FlowDirection.RightToLeft)
						},
						new Button
						{
							Text = "Set LeftToRight",
							Command = new Command(() => FlowDirection = FlowDirection.LeftToRight)
						}
					}
				}
			});
		}
	}
}
