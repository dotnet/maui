namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 5949, "CollectionView cannot access a disposed object.",
	PlatformAffected.iOS)]
public class Issue5949 : TestContentPage
{
	protected override void Init()
	{
		Appearing += Issue5949Appearing;
	}

	private void Issue5949Appearing(object sender, EventArgs e)
	{
		Application.Current.MainPage = new Issue5949_1();
	}
}