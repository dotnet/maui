using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using VisualElement = Microsoft.Maui.Controls.VisualElement;

namespace Maui.Controls.Sample.Issues;

[Preserve(AllMembers = true)]
[Issue(IssueTracker.None, 0, "Shell Flyout Header Footer",
	PlatformAffected.All)]

public class HeaderFooterShellFlyout : TestShell
{
	protected override void Init()
	{
		var page = new ContentPage();

		AddFlyoutItem(page, "Flyout Item");
		page.Content = new StackLayout()
		{
			Children =
			{
				new Label()
				{
					Text = "Open the Flyout and Toggle the Header and Footer. If it changes after each click test has passed",
					AutomationId = "PageLoaded"
				}
			}
		};

		Items.Add(new MenuItem()
		{
			Text = "Toggle Header/Footer Template",
			Command = new Command(() =>
			{
				if (FlyoutHeaderTemplate == null)
				{
					FlyoutHeaderTemplate = new DataTemplate(() =>
					{
						return new Label() { Text = "Header Template" };
					});

					FlyoutFooterTemplate = new DataTemplate(() =>
					{
						return new Label() { Text = "Footer Template" };
					});
				}
				else if (FlyoutHeaderTemplate != null)
				{
					FlyoutHeaderTemplate = null;
					FlyoutFooterTemplate = null;
				}
			}),
			AutomationId = "ToggleHeaderFooterTemplate"
		});

		Items.Add(new MenuItem()
		{
			Text = "Toggle Header/Footer View",
			Command = new Command(() =>
			{
				if (FlyoutHeader != null)
				{
					FlyoutHeader = null;
					FlyoutFooter = null;
				}
				else
				{
					FlyoutHeader = new StackLayout()
					{
						Children = {
							new Label() { Text = "Header" }
						},
						AutomationId = "Header View"
					};

					FlyoutFooter = new StackLayout()
					{
						Orientation = StackOrientation.Horizontal,
						Children = {
							new Label() { Text = "Footer" }
						},
						AutomationId = "Footer View"
					};
				}
			}),
			AutomationId = "ToggleHeaderFooter"
		});

		Items.Add(new MenuItem()
		{
			Text = "Resize Header/Footer",
			Command = new Command(async () =>
			{
				FlyoutHeaderTemplate = null;
				FlyoutFooterTemplate = null;
				if (FlyoutHeader == null)
				{
					FlyoutHeader = new StackLayout()
					{
						Children = {
							new Label() { Text = "Header" }
						},
						AutomationId = "Header View"
					};

					FlyoutFooter = new StackLayout()
					{
						Children = {
							new Label() { Text = "Footer" }
						},
						AutomationId = "Footer View"
					};

					await Task.Delay(10);
				}

				var headerLabel = (VisualElement)FlyoutHeader;
				var footerLabel = (VisualElement)FlyoutFooter;
				headerLabel.BackgroundColor = Colors.LightBlue;

				if (footerLabel is not null)
					footerLabel.BackgroundColor = Colors.LightBlue;

				if (headerLabel.HeightRequest == 60)
				{
					headerLabel.HeightRequest = 200;

					if (footerLabel is not null)
						footerLabel.HeightRequest = 200;
				}
				else
				{
					headerLabel.HeightRequest = 60;

					if (footerLabel is not null)
						footerLabel.HeightRequest = 60;
				}
			}),
			AutomationId = "ResizeHeaderFooter"
		});

		if (DeviceInfo.Platform == DevicePlatform.iOS)
		{
			Items.Add(new MenuItem()
			{
				Text = "Zero Margin Header Test",
				Command = new Command(() =>
				{
					FlyoutHeader =
						new StackLayout()
						{
							AutomationId = "ZeroMarginLayout",
							Margin = 0,
							Children =
							{
								new Label() { Text = "Header View" }
							},
							BackgroundColor = Colors.Purple,
							IgnoreSafeArea = true
						};

					FlyoutHeaderTemplate = null;
					FlyoutBehavior = FlyoutBehavior.Locked;
				}),
				AutomationId = "ZeroMarginHeader"
			});
		}
	}
}
