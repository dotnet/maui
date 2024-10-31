namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 13476, "Shell Title View Test",
		PlatformAffected.iOS)]
	public class Issue13476 : TestShell
	{
		protected override void Init()
		{
			AddTopTab(createContentPage("title 1"), "page 1");
			AddTopTab(createContentPage("title 2"), "page 2");

			ContentPage createContentPage(string titleView)
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
					Shell.SetTitleView(page,
						new Grid()
						{
							BackgroundColor = Colors.PaleGoldenrod,
							AutomationId = "TitleViewId",
							Children = { new Label() { Text = titleView, VerticalTextAlignment = TextAlignment.End } }
						});
				}

				return page;
			}
		}
	}
}