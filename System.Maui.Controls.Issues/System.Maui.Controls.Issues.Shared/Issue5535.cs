using System;
using System.Collections.Generic;
using System.Text;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
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

#if UITEST
		[Test]
		public void SwappingEmptyViews()
		{
			RunningApp.WaitForElement("FilterItems");
			RunningApp.Tap("FilterItems");	
			RunningApp.EnterText("abcdef");
		
			// Default empty view
			RunningApp.WaitForElement("Nothing to see here.");

			RunningApp.Tap("ToggleEmptyView");	

			// Other empty view
			RunningApp.WaitForElement("No results matched your filter.");
		}
#endif
	}
}