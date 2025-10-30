using System.ComponentModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Entry = Microsoft.Maui.Controls.Entry;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 30818, "ToolbarItem color with custom BarTextColor not working", PlatformAffected.iOS)]
	public class Issue30818 : NavigationPage
	{

		public Issue30818() : base(new MainPage())
		{
		}

		class MainPage : ContentPage
		{
			public MainPage()
			{
				Title = "Main Page";

				var toolBarItem = new ToolbarItem
				{
					Text = "Bank",
					IconImageSource = "bank.png",
					AutomationId = "BankToolbarItem",
					Order = ToolbarItemOrder.Primary,
					Priority = 0
				};

				// ToolbarItem with Bank.png icon
				ToolbarItems.Add(toolBarItem);

				var setRedButton = new Button
				{
					Text = "Set BarTextColor Red",
					AutomationId = "SetRedButton"
				};

				var setGreenButton = new Button
				{
					Text = "Set BarTextColor Green",
					AutomationId = "SetGreenButton"
				};

				var setResetButton = new Button
				{
					Text = "Reset BarTextColor",
					AutomationId = "SetResetButton"
				};

				// There is a known issue with setting the BarTextColor on iOS from NULL to a color won't work
				// This will get fixed in a followup issue
				// https://github.com/dotnet/maui/issues/31111
				setRedButton.Clicked += (_, __) =>
				{
					ToolbarItems.Remove(toolBarItem);
					if (Parent is NavigationPage nav)
					{
						nav.BarTextColor = Microsoft.Maui.Graphics.Colors.Red;
					}
					ToolbarItems.Add(toolBarItem);
				};

				setGreenButton.Clicked += (_, __) =>
				{
					// https://github.com/dotnet/maui/issues/31111
					ToolbarItems.Remove(toolBarItem);
					if (Parent is NavigationPage nav)
					{
						nav.BarTextColor = Microsoft.Maui.Graphics.Colors.Green;
					}
					ToolbarItems.Add(toolBarItem);
				};

				setResetButton.Clicked += (_, __) =>
				{
					// https://github.com/dotnet/maui/issues/31111
					ToolbarItems.Remove(toolBarItem);
					if (Parent is NavigationPage nav)
					{
						nav.BarTextColor = null;
					}
					ToolbarItems.Add(toolBarItem);
				};

				Content = new StackLayout
				{
					Padding = 20,
					Children =
					{
						setRedButton,
						setGreenButton,
						setResetButton
					}
				};
			}
		}
	}
}