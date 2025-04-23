namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 11111111, "CollectionView Scroll To Grouped Item", PlatformAffected.All)]
public class ScrollToGroup : TestNavigationPage
{
	protected override void Init()
	{
		PushAsync(new CollectionViewGalleries.ScrollToGalleries.ScrollToGroup());
	}
}
