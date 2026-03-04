namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 11962, "[iOS] Cannot access a disposed object. Object name: 'WkWebViewRenderer",
		PlatformAffected.iOS, isInternetRequired: true)]
	public class Issue11962 : TestShell
	{
		protected override void Init()
		{
			ContentPage contentPage = new ContentPage()
			{
				Content = new Button()
				{
					Text = "Go to the next page and back twice. If app doesn't crash test has passed.",
					Command = new Command(async () =>
					{
						await GoToAsync("//user");
					}),
					AutomationId = "NextButton"
				}
			};

#pragma warning disable CS0618 // Type or member is obsolete
			ContentPage webViewPage = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new WebView()
						{
							Source = new HtmlWebViewSource { Html = GetHtml("string some Text") },
							VerticalOptions = LayoutOptions.StartAndExpand
						},
						new Button()
						{
							Text = "Go Back",
							Command = new Command(async () =>
							{
								await GoToAsync("//usersearch");
							}),
							AutomationId = "BackButton"
						}
					}
				}
			};
#pragma warning restore CS0618 // Type or member is obsolete


			AddFlyoutItem(contentPage, "User Search").Route = "usersearch";
			AddFlyoutItem(webViewPage, "Web View Page").Route = "user";
		}

		string GetHtml(string uid)
		{
			var htmlHeader = @"<header><meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, minimum-scale=1.0, user-scalable=no'></header>";
			if ("UserOne".Equals(uid, System.StringComparison.OrdinalIgnoreCase))
				return htmlHeader + @"<h3>Welcome <b style=""color:red;"">User One</b>,</h3><div>Displaying Html message</div>";
			return htmlHeader + @"<h3>Welcome <b style=""color:blue;"">User Two</b>,</h3><div>Displaying Html message</div>";
		}
	}
}
