namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 26477, "SearchHandler Fails to Display Results on Windows", PlatformAffected.UWP)]
    public class Issue26477 : TestShell
    {
        protected override void Init()
        {
            ContentPage contentPage = new ContentPage();
            AddFlyoutItem(contentPage, "Main Page");

			SearchHandler searchHandler = new SearchHandler()
			{
				ShowsResults = true,
				ItemsSource = Enumerable.Range(0, 100)
				.Select(_ => "searchresult")
				.ToList()
			};

            Shell.SetSearchHandler(contentPage, searchHandler);

            contentPage.Content =
                new StackLayout()
                {
                new Label()
                {
                    Text = "Type into the search handler to display a list.",
                    AutomationId="Label"
                }

                };
        }
    }
}