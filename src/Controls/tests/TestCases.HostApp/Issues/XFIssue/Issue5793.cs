namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 5793, "[CollectionView/ListView] Not listening for Reset command", PlatformAffected.iOS | PlatformAffected.Android)]
class Issue5793 : TestNavigationPage
{
	protected override void Init()
	{

		PushAsync(new CollectionViewGalleries.ObservableCollectionResetGallery());
	}

}
