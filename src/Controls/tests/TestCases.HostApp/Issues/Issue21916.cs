namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 21916, "Page and control Unloaded events firing on iOS when navigating to another page", PlatformAffected.iOS)]
	public class Issue21916 : Shell
	{
		public Issue21916()
		{
			CurrentItem = new Issue21916_MainPage();
		}

		public class Issue21916_MainPage : ContentPage
		{
			int mainPageLoadedCount = 0;
			int mainPageUnloadedCount = 0;
			Label label;
			bool isNavigated;
			public Issue21916_MainPage()
			{
				var stack = new StackLayout();
				label = new Label();
				var Button = new Button();
				Button.Text = "Click to navigate new page";
				Button.AutomationId = "Button";
				Button.Clicked += Clicked;
				stack.Children.Add(Button);
				stack.Children.Add(label);
				this.Content = stack;
				this.Loaded += MainPage_Loaded;
				this.Unloaded += MainPage_UnLoaded;
			}

			private async void Clicked(object sender, EventArgs e)
			{
				isNavigated = true;
				await Navigation.PushAsync(new Issue21916_NewPage());
			}

			private void MainPage_UnLoaded(object sender, EventArgs e)
			{
				mainPageUnloadedCount++;
			}

			private void MainPage_Loaded(object sender, EventArgs e)
			{
				if (isNavigated)
				{
					mainPageLoadedCount++;
					label.Text = $"Unloaded event triggered {mainPageUnloadedCount} times\nLoaded event triggered {mainPageLoadedCount} times";
				}
			}
		}

		public class Issue21916_NewPage : ContentPage
		{
			public Issue21916_NewPage()
			{
				var stack = new StackLayout();
				var Button = new Button();
				Button.Text = "Click to navigate main page";
				Button.AutomationId = "Button";
				Button.Clicked += Clicked;
				stack.Children.Add(Button);
				this.Content = stack;
			}

			private void Clicked(object sender, EventArgs e)
			{
				Shell.Current.GoToAsync("..");
			}
		}
	}
}