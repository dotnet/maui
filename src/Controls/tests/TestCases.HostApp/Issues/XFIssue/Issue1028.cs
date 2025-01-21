namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 1028, "ViewCell in TableView raises exception - root page is ContentPage, Content is TableView", PlatformAffected.WinPhone)]

public class Issue1028 : TestContentPage
{
	// Issue1028, ViewCell with StackLayout causes exception when nested in a table section. This occurs when the app's root page is a ContentPage with a TableView.
	protected override void Init()
	{
		Content = new TableView
		{
			Root = new TableRoot("Table Title") {
				new TableSection ("Section 1 Title") {
					new ViewCell {
						View = new StackLayout {
							Children = {
								new Label {
									Text = "Custom Slider View:",
									AutomationId = "Custom Slider View:"
								},
							}
						}
					}
				}
			}
		};
	}
}
