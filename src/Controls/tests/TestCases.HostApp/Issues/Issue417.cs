namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 417, "Navigation.PopToRootAsync does nothing", PlatformAffected.Android)]
	public class Issue417 : NavigationPage
	{
		public Issue417() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			protected override async void OnAppearing()
			{
				base.OnAppearing();

				//Allow the OnAppearing method to return control to the UI thread before continuing with navigation.
				await Task.Yield();
				await Navigation.PushAsync(new FirstPage());
			}

			public class FirstPage : ContentPage
			{
				public FirstPage()
				{
					AutomationId = "FirstPage";
					Title = "First Page";
					BackgroundColor = Colors.Black;

					var nextPageBtn = new Button
					{
						AutomationId = "NextPage",
						Text = "Next Page"
					};

					nextPageBtn.Clicked += (s, e) => Navigation.PushAsync(new NextPage());

					Content = nextPageBtn;
				}

			}

			public class NextPage : ContentPage
			{
				public NextPage()
				{
					AutomationId = "SecondPage";
					Title = "Second Page";

					var nextPage2Btn = new Button
					{
						AutomationId = "NextPage2",
						Text = "Next Page 2"
					};

					nextPage2Btn.Clicked += (s, e) => Navigation.PushAsync(new NextPage2());
					BackgroundColor = Colors.Black;
					Content = nextPage2Btn;

				}
			}

			public class NextPage2 : ContentPage
			{
				public NextPage2()
				{
					AutomationId = "ThirdPage";
					Title = "Third Page";

					var popToRootButton = new Button
					{
						AutomationId = "PopToRoot",
						Text = "Pop to root"
					};

					popToRootButton.Clicked += (s, e) => Navigation.PopToRootAsync();
					BackgroundColor = Colors.Black;
					Content = popToRootButton;
				}
			}
		}
	}
}