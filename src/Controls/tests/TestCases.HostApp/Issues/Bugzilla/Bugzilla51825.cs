namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 51825, "[iOS] Korean input in SearchBar doesn't work", PlatformAffected.iOS)]
public class Bugzilla51815 : TestContentPage
{
	protected override void Init()
	{
		var sb = new SearchBar { AutomationId = "Bugzilla51825SearchBar" };
		var text = new Label { AutomationId = "Bugzilla51825Label" };
		sb.TextChanged += (sender, e) =>
		{
			text.Text = sb.Text;
		};

		Content = new StackLayout
		{
			Children =
			{
				sb,
				new Button
				{
					AutomationId = "Bugzilla51825Button",
					Text = "Change SearchBar text",
					Command = new Command(() =>
					{
						sb.Text = "Test";
					})
				},
				text,
				new Label
				{
					Text = "The label above should match the text in the SearchBar; " +
						"additionally, typing Korean characters should properly combine them."
				}
			}
		};
	}
}