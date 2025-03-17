namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2272, "Entry text updating set focus on the beginning of text not the end of it", PlatformAffected.Android)]
public class Issue2272 : TestContentPage
{
	protected override void Init()
	{
		var userNameEditor = new Entry() { AutomationId = "userNameEditorEmptyString", Text = "userNameEditorEmptyString" };
		userNameEditor.Focused += (sender, args) =>
		{
			userNameEditor.Text = "focused";
		};

		Content = new StackLayout
		{
			Spacing = 10,
			Children = { userNameEditor }
		};
	}
}