namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 11244, "[Bug] BackButtonBehavior no longer displays on the first routed page in 4.7",
	PlatformAffected.iOS)]
public class Issue11244 : TestShell
{
	protected async override void Init()
	{
		var page1 = AddContentPage<TabBar, Tab>();
		ContentPage page = new ContentPage()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "The app bar should have text instead of a hamburger"
					},
					new Button()
					{
						Text = "Run test again",
						Command = new Command(async () =>
						{
							CurrentItem = page1;
							await Task.Delay(1000);
							await GoToAsync("//MainPage");
						})
					}
				}
			}
		};

		Shell.SetBackButtonBehavior(page,
			new BackButtonBehavior()
			{
				TextOverride = "Logout",
				Command = new Command(() => { })
			});

		var page2 = AddContentPage<TabBar, Tab>(page);
		page2.Route = "MainPage";
		await Task.Delay(1000);
		// Shell's GoToAsync uses URI-based routing instead of the traditional navigation stack, which can result in an invisible navigation bar in the UI.
		// await GoToAsync("//MainPage");
		await Shell.Current.Navigation.PushAsync(page);
	}
}
