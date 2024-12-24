namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Shell Search Handler Item Sizing",
	PlatformAffected.All)]
public class ShellSearchHandlerItemSizing : TestShell
{
	protected override void Init()
	{
		ContentPage contentPage = new ContentPage();
		AddFlyoutItem(contentPage, "Main Page");

		Shell.SetSearchHandler(contentPage, new TestSearchHandler()
		{
			AutomationId = "SearchHandler"
		});

		contentPage.Content =
			new StackLayout()
			{
				new Label()
				{
					Text = "Type into the search handler to display a list. Each item should be measured to the size of the content",
					AutomationId="Instructions"
				}
			};
	}


	public class TestSearchHandler : SearchHandler
	{
		public TestSearchHandler()
		{
			ShowsResults = true;
			ItemsSource = Enumerable.Range(0, 100)
				.Select(_ => "searchresult")
				.ToList();
		}
	}
}
