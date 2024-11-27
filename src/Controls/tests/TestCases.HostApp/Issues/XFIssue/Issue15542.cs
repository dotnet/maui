namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 15542, "[Bug] Shell.TitleView does not render on iOS 16", PlatformAffected.iOS)]
public class Issue15542 : TestShell
{
	protected override void Init()
	{
		AddTopTab(createContentPage("title 1"), "page 1");
		AddTopTab(createContentPage("title 2"), "page 2");
		AddTopTab(createContentPage("title 3"), "page 3");

		static ContentPage createContentPage(string titleView)
		{
			Label safeArea = new Label();
			ContentPage page = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "If the TitleView is not visible the test has failed.",
							AutomationId = "Instructions"
						},
						safeArea
					}
				}
			};

			if (!string.IsNullOrWhiteSpace(titleView))
			{
				SetTitleView(page,
					new Grid()
					{
						BackgroundColor = Colors.Red,
						AutomationId = "TitleViewId",
						Children = { new Label() { Text = titleView, VerticalTextAlignment = TextAlignment.End } }
					});
			}

			return page;
		}
	}
}