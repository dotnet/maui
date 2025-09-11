namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 21640, "TabbedPage content was not updated", PlatformAffected.Android)]
	public partial class Issue21640 : TabbedPage
	{
		private Page _savedPage;
		public Issue21640()
		{
			var homePage = new ContentPage
			{
				Title = "Home",
				Content = new Button
				{
					Text = "Toggle Tab",
					Command = new Command(ToggleTab),
					AutomationId = "ToogleTabButton"
				}
			};

			var newPage = CreateNewPage();
			_savedPage = new NavigationPage(newPage) { Title = "Settings" };
			Children.Add(new NavigationPage(homePage) { Title = "Home" });
		}

		private async void ToggleTab()
		{
			if (Children.Count == 1)
			{
				await MainThread.InvokeOnMainThreadAsync(() =>
				{
					Children.Add(_savedPage);
				});
			}
			else if (Children.Count == 2)
			{
				_savedPage = Children[1];
				await MainThread.InvokeOnMainThreadAsync(() =>
				{
					Children.RemoveAt(1);
				});
			}
		}

		private Page CreateNewPage()
		{
			return new ContentPage
			{
				Title = "NewPage",
				Content = new VerticalStackLayout
				{
					Children =
					{
						new Label
						{
							Text = "Welcome to .NET MAUI!",
							VerticalOptions = LayoutOptions.Center,
							HorizontalOptions = LayoutOptions.Center,
							AutomationId = "label"
						}
					}
				}
			};
		}
	}
}