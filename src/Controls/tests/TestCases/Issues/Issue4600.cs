using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4600, "[iOS] CollectionView crash with empty ObservableCollection", PlatformAffected.iOS)]
	public class Issue4600 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.ObservableCodeCollectionViewGallery(initialItems: 0));
#endif
		}
	}
}