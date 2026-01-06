using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 33382, "[iOS] AppInfo.ShowSettingsUI only opens settings app", PlatformAffected.iOS)]
	public partial class Issue33382 : ContentPage
	{
		public Issue33382()
		{
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
				{
					new Label
					{
						Text = "AppInfo.ShowSettingsUI Test",
						FontSize = 20,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center,
						AutomationId = "TitleLabel"
					},
					new Label
					{
						Text = "Tap the button below to test ShowSettingsUI.\n\n" +
						       "Expected: Settings app opens AND navigates to this app's settings page.\n\n" +
						       "Bug: On iOS 26, only opens Settings app without navigating to app settings.",
						HorizontalOptions = LayoutOptions.Center,
						AutomationId = "InstructionLabel"
					},
					new Button
					{
						Text = "Test ShowSettingsUI",
						AutomationId = "TestButton",
						Command = new Command(() =>
						{
							AppInfo.ShowSettingsUI();
						})
					},
					new Label
					{
						Text = "After tapping, verify Settings app opens AND shows this app's settings page.",
						HorizontalOptions = LayoutOptions.Center,
						TextColor = Colors.Red,
						FontAttributes = FontAttributes.Bold,
						AutomationId = "ResultLabel"
					}
				}
			};
		}
	}
}
