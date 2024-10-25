namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1146, "Disabled Switch in Button Gallery not rendering on all devices", PlatformAffected.Android)]
	public class Issue1146 : TestContentPage
	{
		protected override void Init()
		{
			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Padding = new Size(20, 20),
					Children = {
						new StackLayout {
							Orientation = StackOrientation.Horizontal,
							Children= {
								new Switch() { IsEnabled = false , AutomationId="switch"},
							},
						},
					}
				}
			};
		}
	}
}
