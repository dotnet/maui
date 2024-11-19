namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7240, "[Android] Shell content layout hides navigated to page", PlatformAffected.Android)]
public class Issue7240 : TestShell
{
	const string Success = "Page Count:3";
	const string ClickMe = "ClickMe";

	int pageCount = 1;
	protected override void Init()
	{
		Func<ContentPage> createNewPage = null;
		createNewPage = () =>
			   new ContentPage()
			   {
				   Content = new StackLayout()
				   {
						new Button()
						{
							Text = "Click me and you should see a new page with this same button in the same place",
							AutomationId = ClickMe,
							Command = new Command(() =>
							{
								pageCount++;
								Navigation.PushAsync(createNewPage());
							})
						},
						new Label()
						{
							Text = $"Page Count:{pageCount}",
							AutomationId=$"Page Count:{pageCount}"
						}
					}
			   };

		AddContentPage(createNewPage());
	}
}
