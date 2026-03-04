namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 29923, "Removed page handlers not disconnected when using Navigation.RemovePage()", PlatformAffected.All)]
public partial class Issue29923 : Shell
{
	public Issue29923()
	{
		var rootPage = new ContentPage
		{
			Title = "Handler Disconnection Test",
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(30, 0),
				Spacing = 25,
				Children =
				{
					new Button
					{
						Text = "Test Page Modal",
						AutomationId = "NavigateToTestPageModalButton",
						Command = new Command(async () =>
						{
							var navPage = new NavigationPage();
							var page1 = new Issue29923TestPage();
							var page2 = new Issue29923TestPage2();

							await navPage.PushAsync(page1);
							await navPage.PushAsync(page2);

							await Navigation.PushModalAsync(navPage);
						})
					},
					new Button
					{
						Text = "Test Page",
						AutomationId = "NavigateToTestPageButton",
						Command = new Command(async () =>
						{
							var page1 = new Issue29923TestPage();
							var page2 = new Issue29923TestPage2();

							await Navigation.PushAsync(page1);
							await Navigation.PushAsync(page2);
						})
					}
				}
			}
		};

		Items.Add(new ShellContent
		{
			Title = "Handler Test",
			Content = rootPage
		});
	}

	public class Issue29923TestPage : ContentPage
	{
		public Issue29923TestPage()
		{
			Title = "Test Page";

			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 10,
			};
		}
	}

	public class Issue29923TestPage2 : ContentPage
	{
		private Label _handlerStatusLabel;

		public Issue29923TestPage2()
		{
			Title = "Test Page 2";

			_handlerStatusLabel = new Label
			{
				Text = "",
				AutomationId = "HandlerStatusLabel",
				FontAttributes = FontAttributes.Bold
			};

			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 10,
				Children =
				{
					new Label
					{
						Text = "Test Page Removal Scenarios",
						FontSize = 18,
						FontAttributes = FontAttributes.Bold
					},
					new Button
					{
						Text = "Remove Test Page 1",
						AutomationId = "RemoveTestPage1Button",
						Command = new Command(RemoveFirstPage)
					},
					new Button
					{
						Text = "Pop Modal",
						AutomationId = "PopModalButton",
						Command = new Command(() => Navigation.PopModalAsync())
					},
					new HorizontalStackLayout
					{
						Children =
						{
							new Label { Text = "Is Handler still available:", FontAttributes = FontAttributes.Bold },
							_handlerStatusLabel
						}
					}
				}
			};
		}

		private void RemoveFirstPage()
		{
			var navigationStack = Navigation.NavigationStack;
			if (navigationStack.Count == 0)
				return;

			var pageToRemove = navigationStack.FirstOrDefault();
			if (pageToRemove == null && navigationStack.Count > 1)
				pageToRemove = navigationStack[1];

			if (pageToRemove != null)
			{
				Navigation.RemovePage(pageToRemove);
				_handlerStatusLabel.Text = (pageToRemove.Handler != null).ToString();
			}
		}
	}
}
