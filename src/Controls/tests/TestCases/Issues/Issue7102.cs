using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7102, "[Bug] CollectionView Header cause delay to adding items.",
		PlatformAffected.Android)]
	public class Issue7102 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new GalleryPages.CollectionViewGalleries.ObservableCodeCollectionViewGallery(grid: false));
		}
	}
}