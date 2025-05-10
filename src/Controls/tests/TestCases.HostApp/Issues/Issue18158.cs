using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18158, "FlyoutPage does not respond to changes in the FlyoutLayoutBehavior property", PlatformAffected.iOS)]
public class Issue18158 : TestFlyoutPage
{
	Button _button;
	protected override void Init()
	{
		_button = new Button { Text = "Click", AutomationId = "Button" };
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;
		// Define the Flyout page
		Flyout = new ContentPage
		{
			Title = "Menu",

			Content = new StackLayout
			{
				Children =
				{
					_button,
				}
			}
		};

		_button.Clicked += (s, e) =>
		{
			if (this.FlyoutLayoutBehavior == FlyoutLayoutBehavior.Split)
			{
				this.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
			}
			else
			{
				this.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;
			}
		};


		// Define the Detail page
		Detail = new NavigationPage(new ContentPage
		{
			Title = "Detail Page",
			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "Welcome to the Detail Page!", AutomationId = "Label" },
				}
			}
		});

	}
}
