namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34143, "Tab bar ghosting issue after navigating from modal via GoToAsync", PlatformAffected.iOS)]
public class Issue34143 : TestShell
{
	protected override void Init()
	{
		var homeShellContent = new ShellContent
		{
			Title = "Home",
			Route = "Issue34143Home",
			ContentTemplate = new DataTemplate(() => new Issue34143MainPage())
		};

		var tab1 = new Tab
		{
			Title = "Tab 1",
			Route = "Issue34143Tab1",
			AutomationId = "Issue34143Tab1",
			Icon = "coffee.png",
			Items =
			{
				new ShellContent
				{
					Route = "Issue34143Tab1Content",
					ContentTemplate = new DataTemplate(() => new Issue34143TabPage("Tab 1 Content"))
				}
			}
		};

		var tab2 = new Tab
		{
			Title = "Tab 2",
			Route = "Issue34143Tab2",
			AutomationId = "Issue34143Tab2",
			Icon = "coffee.png",
			Items =
			{
				new ShellContent
				{
					Route = "Issue34143Tab2Content",
					ContentTemplate = new DataTemplate(() => new Issue34143TabPage("Tab 2 Content"))
				}
			}
		};

		var tab3 = new Tab
		{
			Title = "Tab 3",
			Route = "Issue34143Tab3",
			AutomationId = "Issue34143Tab3",
			Icon = "coffee.png",
			Items =
			{
				new ShellContent
				{
					Route = "Issue34143Tab3Content",
					ContentTemplate = new DataTemplate(() => new Issue34143TabPage("Tab 3 Content"))
				}
			}
		};

		var tabBar = new TabBar
		{
			Items = { tab1, tab2, tab3 }
		};

		Items.Add(homeShellContent);
		Items.Add(tabBar);
	}

	public class Issue34143MainPage : ContentPage
	{
		public Issue34143MainPage()
		{
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Home Page",
						AutomationId = "Issue34143HomeLabel",
						HorizontalOptions = LayoutOptions.Center
					},
					new Button
					{
						Text = "Push Modal",
						AutomationId = "Issue34143PushModal",
						Command = new Command(async () =>
							await Shell.Current.Navigation.PushModalAsync(new Issue34143ModalPage()))
					}
				}
			};
		}
	}

	public class Issue34143ModalPage : ContentPage
	{
		public Issue34143ModalPage()
		{
			Title = "Modal";
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Modal Page",
						AutomationId = "Issue34143ModalLabel",
						HorizontalOptions = LayoutOptions.Center
					},
					new Button
					{
						Text = "Go to Tab Bar",
						AutomationId = "Issue34143GoToTabBar",
						Command = new Command(async () =>
							await Shell.Current.GoToAsync("//Issue34143Tab1"))
					}
				}
			};
		}
	}

	public class Issue34143TabPage : ContentPage
	{
		public Issue34143TabPage(string contentText)
		{
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = contentText,
						AutomationId = "Issue34143TabContent",
						HorizontalOptions = LayoutOptions.Center
					}
				}
			};
		}
	}
}
