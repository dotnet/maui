using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 33523, "OnBackButtonPressed not firing for Shell Navigation Bar button in .NET 10 SR2", PlatformAffected.Android)]
	public class Issue33523 : Shell
	{
		public Issue33523()
		{
			var mainPage = new ContentPage
			{
				Title = "Main Page",
				Content = new VerticalStackLayout
				{
					Children =
					{
						new Label
						{
							Text = "Click the button to navigate to TestPage",
							AutomationId = "MainPageLabel"
						},
						new Button
						{
							Text = "Navigate to TestPage",
							AutomationId = "NavigateButton",
							Command = new Command(async () => await Shell.Current.GoToAsync("TestPage"))
						}
					}
				}
			};

			Items.Add(new ShellContent { Content = mainPage, Route = "MainPage" });
			
			Routing.RegisterRoute("TestPage", typeof(TestPage));
		}

		class TestPage : ContentPage
		{
			private Label _statusLabel;

			public TestPage()
			{
				Title = "Test Page";

				_statusLabel = new Label
				{
					Text = "OnBackButtonPressed not called",
					AutomationId = "StatusLabel"
				};

				Content = new VerticalStackLayout
				{
					Children =
					{
						new Label
						{
							Text = "Click the Navigation Bar back button (←)",
							AutomationId = "InstructionLabel"
						},
						_statusLabel
					}
				};
			}

			protected override bool OnBackButtonPressed()
			{
				// Update the label synchronously
				_statusLabel.Text = "OnBackButtonPressed was called";
				
				// Return true to prevent navigation (so we can verify the label changed)
				return true;
			}
		}
	}
}
