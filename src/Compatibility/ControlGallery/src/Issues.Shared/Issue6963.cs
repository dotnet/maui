using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6963, "[Bug] CollectionView multiple pre-selection throws ArgumentOutOfRangeException when SelectedItems is bound to an ObservableCollection initialized inside the constructor.",
		PlatformAffected.iOS | PlatformAffected.UWP)]
	public class Issue6963 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.SelectionGalleries.SelectionSynchronization());
#endif
		}

#if UITEST
		[Test]
		public void SelectedItemsNotInSourceDoesNotCrash()
		{
			// If this page didn't crash, then we're good
			RunningApp.WaitForElement("FirstLabel");
		}
#endif
	}
}
