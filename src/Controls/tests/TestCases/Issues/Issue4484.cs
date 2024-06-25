using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 4484, "[Android] ImageButton inside NavigationView.TitleView throw exception during device rotation",
		PlatformAffected.Android)]
	public class Issue4484 : NavigationPage
	{
		public Issue4484() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{

			public MainPage()
			{
				ContentPage page = new ContentPage();

				NavigationPage.SetTitleView(page,
					new StackLayout()
					{
						Orientation = StackOrientation.Horizontal,
						Children =
						{
						new Button(){ ImageSource = "coffee.png", AutomationId="bank"},
						new Image(){Source = "coffee.png"},
						new ImageButton{Source = "coffee.png"}
						}
					});

				page.Content = new StackLayout()
				{
					Children =
				{
					new Label()
					{
						Text = "You should see 3 images. Rotate device. If it doesn't crash then test has passed",
						AutomationId = "Instructions"
					}
				}
				};

				Navigation.PushAsync(page);
			}
		}
	}
}