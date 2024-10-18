namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24862, "Android - picker on hidden page opens after back navigation", PlatformAffected.Android)]
	public class Issue24862 : NavigationPage
	{
		public Issue24862() : base(new MainPage())
		{

		}
	}

	public class MainPage : ContentPage
	{
		public MainPage()
		{
			var mainButton = new Button
			{
				AutomationId = "MainButton",
				Text = "Go to first modal page",
				HorizontalOptions = LayoutOptions.Fill
			};
			mainButton.Clicked += MainButton_Clicked;

			var picker = new Picker
			{
				Title = "Select a monkey"
			};

			picker.ItemsSource = new List<string>
			{
				"Baboon",
				"Capuchin Monkey",
				"Blue Monkey",
				"Squirrel Monkey",
				"Golden Lion Tamarin",
				"Howler Monkey",
				"Japanese Macaque"
			};

			var stackLayout = new VerticalStackLayout
			{
				Padding = new Thickness(30, 0),
				Spacing = 25,
				Children = { mainButton, picker }
			};

			var scrollView = new ScrollView
			{
				Content = stackLayout
			};

			Content = scrollView;
		}

		private async void MainButton_Clicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new FirstPage());
		}
	}

	public class FirstPage : ContentPage
	{
		public FirstPage()
		{
			Shell.SetPresentationMode(this, PresentationMode.Modal);
			Title = "FirstPage";

			var mainButton = new Button
			{
				AutomationId = "FirstPageButton",
				Text = "Go to second modal page"
			};
			mainButton.Clicked += mainButton_Clicked;

			var stackLayout = new VerticalStackLayout();
			stackLayout.Children.Add(mainButton);

			Content = stackLayout;
		}

		private async void mainButton_Clicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new SecondPage());
		}
	}

	public class SecondPage : ContentPage
	{
		Entry _entry;
		public SecondPage()
		{
			Shell.SetPresentationMode(this, PresentationMode.Modal);
			Title = "SecondPage";

		    _entry = new Entry();

			var backButton = new Button
			{
				AutomationId = "GoBackButton",
				Text = "Go back"
			};
			backButton.Clicked += BackButton_Clicked;

			var stackLayout = new VerticalStackLayout
			{
				Margin = new Thickness(20),
				Spacing = 20,
				Children = { _entry, backButton }
			};

			Content = stackLayout;
		}

		override protected async void OnAppearing()
		{
			await Task.Delay(500);

			_entry.Focus();

			base.OnAppearing();
		}

		private async void BackButton_Clicked(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}
	}
}
