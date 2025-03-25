namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1426, "SetHasNavigationBar screen height wrong", PlatformAffected.iOS)]
	public class Issue1426 : NavigationPage
	{
		public Issue1426() : base(new Issue1426TestTabbedPage())
		{
		}

		public class Issue1426TestTabbedPage : TestTabbedPage
		{
			protected override void Init()
			{
				Children.Add(new NavigationPage(new HomePage()) { Title = "Home", BarBackgroundColor = Colors.Red });
			}

			class HomePage : ContentPage
			{
				public HomePage()
				{
					Title = "Home";
					var grd = new Grid { BackgroundColor = Colors.Brown };
					grd.RowDefinitions.Add(new RowDefinition());
					grd.RowDefinitions.Add(new RowDefinition());
					grd.RowDefinitions.Add(new RowDefinition());

					var boxView = new BoxView { BackgroundColor = Colors.Blue };
					grd.Add(boxView, 0, 0);
					var btn = new Button()
					{
						BackgroundColor = Colors.Pink,
						Text = "NextButtonID",
						AutomationId = "NextButtonID",
						Command = new Command(async () =>
						{
							var btnPop = new Button { Text = "PopButtonId", AutomationId = "PopButtonId", Command = new Command(async () => await Navigation.PopAsync()) };
							var page = new ContentPage
							{
								Title = "Detail",
								Content = btnPop,
								BackgroundColor = Colors.Yellow
							};
							//This breaks layout when you pop!
							NavigationPage.SetHasNavigationBar(page, false);
							await Navigation.PushAsync(page);
						})
					};

					grd.Add(btn, 0, 1);
					var image = new Image() { Source = "coffee.png", AutomationId = "CoffeeImageId", BackgroundColor = Colors.Yellow, HeightRequest = 56, WidthRequest = 56 };
					image.VerticalOptions = LayoutOptions.End;
					grd.Add(image, 0, 2);
					Content = grd;

				}
			}
		}
	}
}