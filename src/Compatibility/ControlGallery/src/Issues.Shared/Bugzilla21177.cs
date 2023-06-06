using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
#if UITEST
using Xamarin.UITest.iOS;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 21177, "Using a UICollectionView in a ViewRenderer results in issues with selection")]
	public class Bugzilla21177 : TestContentPage
	{
		public class CollectionView : View
		{
			public event EventHandler<int> ItemSelected;

			public void InvokeItemSelected(int index)
			{
				if (ItemSelected != null)
				{
					ItemSelected.Invoke(this, index);
				}
			}
		}

		protected override void Init()
		{
			var view = new CollectionView() { AutomationId = "view" };
			view.ItemSelected += View_ItemSelected;
			Content = view;
		}

		private void View_ItemSelected(object sender, int e)
		{
			DisplayAlert("Success", "Success", "Cancel");
		}

#if UITEST && __IOS__
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Bugzilla21177Test()
		{
			RunningApp.WaitForElement(q => q.Marked("#1"));
			RunningApp.Tap(q => q.Marked("#1"));
			RunningApp.WaitForElement(q => q.Marked("Success"));
			RunningApp.Tap(q => q.Marked("Cancel"));
		}
#endif
	}
}
