using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5951, "App Crashes On Shadow Effect's OnDetached On Button That's Never Visible", PlatformAffected.iOS)]
	public class Issue5951 : Microsoft.Maui.Controls.NavigationPage
	{
		public Issue5951() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				var instructionsLabel = new Label { Text = "Press the push page button. If everything works, you'll see the word success below.", FontSize = 16 };

				var resultsLabel = new Label { Text = string.Empty, FontSize = 16 };

				var pushButton = new Button
				{
					AutomationId = "Push page",
					Text = "Push page",
					Command = new Command(async () =>
					{
						var shadowPage = new PageWithShadowButton();

						await Navigation.PushAsync(shadowPage);

						try
						{
							await shadowPage.Navigation.PopAsync();

							resultsLabel.Text = "Success";
						}
						catch (NullReferenceException)
						{
							resultsLabel.Text = "Error";
						}
					})
				};

				Content = new StackLayout
				{
					Children = {
					instructionsLabel,
		  			resultsLabel,
					pushButton
				}
				};


			}
		}

		[Preserve(AllMembers = true)]
		public class PageWithShadowButton : ContentPage
		{
			public PageWithShadowButton()
			{
				var shadowButton = new Button { Text = "Never Visible", IsVisible = false };

				shadowButton.On<iOS>()
					.SetIsShadowEnabled(true)
					.SetShadowColor(Colors.Black)
					.SetShadowOffset(new Size(10, 10))
					.SetShadowOpacity(0.2);

				Content = new StackLayout
				{
					Children = { shadowButton }
				};
			}
		}
	}
}