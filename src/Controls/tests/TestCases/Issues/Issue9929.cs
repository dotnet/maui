using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9929, "[Bug] NSInternalInconsistencyException when trying to run XamarinTV on iOS",
		PlatformAffected.iOS)]
	public class Issue9929 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new CollectionViewGalleries.SpacingGalleries.SpacingGallery(new GridItemsLayout(3, ItemsLayoutOrientation.Vertical)));
		}
	}
}
