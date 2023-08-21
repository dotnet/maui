//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

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
	[Issue(IssueTracker.Github, 5793, "[CollectionView/ListView] Not listening for Reset command",
		PlatformAffected.iOS | PlatformAffected.Android)]
	class Issue5793 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.ObservableCollectionResetGallery());
#endif
		}

#if UITEST
		[Test]
		public void Reset()
		{
			RunningApp.WaitForElement("Reset");

			// Verify the item is there
			RunningApp.WaitForElement("cover1.jpg, 0");

			// Clear the collection
			RunningApp.Tap("Reset");	

			// Verify the item is gone
			RunningApp.WaitForNoElement("cover1.jpg, 0");
		}
#endif
	}
}
