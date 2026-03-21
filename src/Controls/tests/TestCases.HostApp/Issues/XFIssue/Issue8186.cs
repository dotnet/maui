namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8186, "[UWP] Setting IsRefreshing from OnAppearing on RefreshView crashes UWP",
	PlatformAffected.UWP)]
public class Issue8186 : TestNavigationPage
{
	RefreshView _refreshView;
	protected override void Init()
	{
		_refreshView = new RefreshView()
		{
			Content = new ScrollView()
			{
				Content = new StackLayout()
				{
					new Label()
					{
						Text = "If you are reading this and see a refresh circle test has succeeded",
						AutomationId = "Success"
					},
					new Button()
					{
						Text = "Push Page then return to this page.",
						Command = new Command(() =>
						{
							Navigation.PushAsync(new ContentPage()
							{
								SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container),
								Content = new Button()
								{
									Text = "Pop Page",
									AutomationId = "PopPage",
									Command = new Command(()=> Navigation.PopAsync())
								}
							});
						}),
						AutomationId = "PushPage"
					}
				}
			}
		};

		var page = new ContentPage() { Content = _refreshView };
		page.Appearing += (_, __) => _refreshView.IsRefreshing = true;
		page.Disappearing += (_, __) => _refreshView.IsRefreshing = false;
		PushAsync(page);
	}
}
