namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6609, "[Bug, CollectionView] SelectionChangedCommand invoked before SelectedItem is set",
	PlatformAffected.All)]
public class Issue6609 : TestNavigationPage
{
	protected override void Init()
	{
		PushAsync(new CollectionViewGalleries.SelectionGalleries.SelectionChangedCommandParameter());
	}
}
