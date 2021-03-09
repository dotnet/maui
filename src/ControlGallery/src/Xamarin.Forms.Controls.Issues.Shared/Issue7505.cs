using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7505, "Icons from Toolbaritem are not displayed on UWP if starts on second monitor", PlatformAffected.UWP)]
	public class Issue7505 : FlyoutPage
	{
		public Issue7505()
		{
			Flyout = new ContentPage { Title = "master" };
			Detail = CreateDetailPage("Don't look here, look at the toolbar!");
		}

		static Page CreateDetailPage(string text)
		{
			var page = new ContentPage
			{
				Title = text,
				Content = new StackLayout
				{
					Children = {
						new Label {
							Text = "Pre-req: Have a multi-monitor setup with different resolutions. Then, start this app, drag it onto the secondary monitor and keep restarting it until you see the toolbar icons disappear. If they don't disappear, this works! Yes, it's a fun one.",
							VerticalOptions = LayoutOptions.CenterAndExpand,
							HorizontalOptions = LayoutOptions.CenterAndExpand,
						}
					}
				}
			};

			var tbiBank = new ToolbarItem { Command = new Command(() => { }), IconImageSource = "bank.png" };
			var tbiCalc = new ToolbarItem { Command = new Command(() => { }), IconImageSource = "calculator.png" };
			var tbiXam = new ToolbarItem { Command = new Command(() => { }), IconImageSource = "xamarinlogo.png" };
			var tbiXamSecondary = new ToolbarItem { Command = new Command(() => { }), IconImageSource = "xamarinlogo.png", Order = ToolbarItemOrder.Secondary };
			var tbiCalcSecondary = new ToolbarItem { Command = new Command(() => { }), IconImageSource = "calculator.png", Order = ToolbarItemOrder.Secondary };


			page.ToolbarItems.Add(tbiBank);
			page.ToolbarItems.Add(tbiCalc);
			page.ToolbarItems.Add(tbiXam);
			page.ToolbarItems.Add(tbiXamSecondary);
			page.ToolbarItems.Add(tbiCalcSecondary);

			return new NavigationPage(page);
		}
	}
}
