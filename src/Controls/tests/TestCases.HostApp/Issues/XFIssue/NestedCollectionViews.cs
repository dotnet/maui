namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6620, "[iOS] Crash when creating a CollectionView inside a CollectionView",
	PlatformAffected.iOS | PlatformAffected.UWP)]
public class NestedCollectionViews : TestNavigationPage
{
	protected override void Init()
	{
		PushAsync(new CollectionViewGalleries.NestedGalleries.NestedCollectionViewGallery());
	}
}
