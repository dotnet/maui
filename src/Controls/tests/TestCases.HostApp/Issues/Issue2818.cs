using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Preserve(AllMembers = true)]
[Issue(IssueTracker.Github, 2818, "Right-to-Left FlyoutPage in Xamarin.Forms Hamburger icon issue", PlatformAffected.Android)]
public class Issue2818 : TestFlyoutPage
{

	protected override void Init()
	{
		FlowDirection = FlowDirection.RightToLeft;
		Flyout = new ContentPage
		{
			Title = "Flyout",
			BackgroundColor = Colors.SkyBlue,
			IconImageSource = "menuIcon",
			Content = new StackLayout()
			{
				Children =
				{
					new Button()
					{
						Text = "If you can see me the test has passed",
						AutomationId = "CloseRootView",
						Command = new Command(() => IsPresented = false)
					}
				},
				AutomationId = "RootLayout"
			},
			Padding = new Thickness(0, 42, 0, 0)
		};

		Detail = new NavigationPage(new ContentPage
		{
			Title = "Detail",
			Content = new StackLayout
			{
				Children = {
					new Label
					{
						Text = "The page must be with RightToLeft FlowDirection. Hamburger icon in main page must be going to right side. There should be visible text inside the Flyout View"
					},
					new Button
					{
						Text = "Set RightToLeft",
						Command = new Command(() => FlowDirection = FlowDirection.RightToLeft),
						AutomationId = "ShowRightToLeft"
					},
					new Button
					{
						Text = "Set LeftToRight",
						Command = new Command(() => FlowDirection = FlowDirection.LeftToRight),
						AutomationId = "ShowLeftToRight"
					},
					new Button
					{
						Text = "Open Flyout View",
						Command = new Command(() => IsPresented = true),
						AutomationId = "OpenRootView"
					},
					new Label()
					{
						Text = DeviceInfo.Idiom.ToString(),
						AutomationId = "Idiom"
					}
				}
			}
		});
	}
}
