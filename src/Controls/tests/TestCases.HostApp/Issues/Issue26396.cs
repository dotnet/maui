namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26396, "Setting Window.TitleBar to null does not remove the customization", PlatformAffected.UWP)]
	public class Issue26396 : Shell
	{
		public Issue26396()
		{
			var firstFlyoutItem = new FlyoutItem
			{
				Title = "No TitleBar"
			};

			firstFlyoutItem.Items.Add(new ShellContent
			{
				ContentTemplate = new DataTemplate(() => new _26396FirstPage()),
				Route = "FirstPage"
			});

			Items.Add(firstFlyoutItem);

			var secondFlyoutItem = new FlyoutItem
			{
				Title = "TitleBar"
			};

			secondFlyoutItem.Items.Add(new ShellContent
			{
				ContentTemplate = new DataTemplate(() => new _26396SecondPage()),
				Route = "SecondPage"
			});

			Items.Add(secondFlyoutItem);

			FlyoutBehavior = FlyoutBehavior.Locked;
		}
	}

	public class _26396FirstPage : ContentPage
	{
		public _26396FirstPage()
		{
			Title = "First Page";
			var firstPageButton = new Button
			{
				Text = "Go to Second Page",
				AutomationId = "FirstPageButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			firstPageButton.Clicked += (sender, args) =>
			{
				Shell.Current.GoToAsync("//SecondPage");
			};

			Content = new StackLayout
			{
				Children = { firstPageButton }
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Application.Current.Windows[0].TitleBar = null;
		}
	}

	public class _26396SecondPage : ContentPage
	{
		public _26396SecondPage()
		{
			Title = "Second Page";
			var secondPageButton = new Button
			{
				AutomationId = "SecondPageButton",
				Text = "Go to First Page",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			secondPageButton.Clicked += (sender, args) =>
			{
				Shell.Current.GoToAsync("//FirstPage");
			};

			Content = new StackLayout
			{
				Children = { secondPageButton }
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Window.TitleBar = new CustomTitleBar();
		}
	}

	public class CustomTitleBar : TitleBar
	{
		public CustomTitleBar()
		{
			BackgroundColor = Colors.Blue;
			ForegroundColor = Colors.White;
			HeightRequest = 48;

			var leadingGrid = new Grid
			{
				Margin = new Thickness(20, 0, 0, 0),
				ColumnDefinitions = new ColumnDefinitionCollection
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Auto }
				},
				ColumnSpacing = 15,
				VerticalOptions = LayoutOptions.Center
			};

			var image = new Image
			{
				HeightRequest = 16,
				WidthRequest = 16,
				Source = "dotnet_bot.png"
			};

			var appTitleLabel = new Label
			{
				Text = "MyApp",
				FontSize = 18,
				TextColor = Colors.White,
				VerticalTextAlignment = TextAlignment.Center
			};

			var titleLabel = new Label
			{
				Text = "XAML",
				FontSize = 18,
				TextColor = Colors.White,
				VerticalTextAlignment = TextAlignment.Center
			};

			leadingGrid.Add(image, 0, 0);
			leadingGrid.Add(appTitleLabel, 1, 0);
			leadingGrid.Add(titleLabel, 2, 0);

			LeadingContent = leadingGrid;

			var searchBar = new SearchBar
			{
				BackgroundColor = Colors.White,
				HorizontalOptions = LayoutOptions.Fill,
				MaximumWidthRequest = 300,
				Placeholder = "Search",
				VerticalOptions = LayoutOptions.Center
			};

			Content = searchBar;

			var imageButton = new ImageButton
			{
				BackgroundColor = Colors.Transparent,
				BorderWidth = 0,
				HeightRequest = 36,
				WidthRequest = 36,
				Source = new FontImageSource
				{
					FontFamily = "FAS",
					Glyph = "\uf013",
					Size = 18,
					Color = Colors.White
				}
			};

			TrailingContent = imageButton;
		}
	}
}
