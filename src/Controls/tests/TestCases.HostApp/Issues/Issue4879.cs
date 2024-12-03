namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 4879, "4879 - ImageButtonPadding", PlatformAffected.UWP)]
	public class Issue4879 : TestContentPage
	{
		protected override async void Init()
		{
			await Navigation.PushModalAsync(new Issue4879Page());
		}

		public class Issue4879Page : ContentPage
		{
			public Issue4879Page()
			{
				Button b = new Button
				{
					HorizontalOptions = LayoutOptions.End,
					VerticalOptions = LayoutOptions.End,
					ImageSource = "coffee.png",
					Padding = new Thickness(10),
					BackgroundColor = Colors.Green,
					AutomationId = "TestReady"
				};
				// Add BorderWidth to ImageButtons to match border of Button and allow for easier size comparisons
				ImageButton ib1 = new ImageButton
				{
					HorizontalOptions = LayoutOptions.Start,
					VerticalOptions = LayoutOptions.End,
					BorderWidth = 2,
					Source = "coffee.png",
					Padding = new Thickness(10),
					BackgroundColor = Colors.Purple
				};
				ImageButton ib2 = new ImageButton
				{
					HorizontalOptions = LayoutOptions.End,
					VerticalOptions = LayoutOptions.Start,
					BorderWidth = 2,
					Source = "coffee.png",
					Padding = new Thickness(10),
					BackgroundColor = Colors.Red
				};
				Grid mainG = new Grid
				{
					ColumnDefinitions = {
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Star }
					},
					RowDefinitions = {
					new RowDefinition { Height = GridLength.Star },
					new RowDefinition { Height = GridLength.Star }
					}
				};

				// Green Button top left
				// Purple ImageButton top right to compare height
				// Red ImageButton bottom left to compare width
				mainG.Add(b, 0, 0);
				mainG.Add(ib1, 1, 0);
				mainG.Add(ib2, 0, 1);
				Content = mainG;
			}
		}
	}
}
