using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5535, "CollectionView: Swapping EmptyViews has no effect",
		PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue5535 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.EmptyViewGalleries.EmptyViewSwapGallery());
#endif
		}
	}
}