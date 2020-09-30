using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 4539135, "CollectionView: Grouping", PlatformAffected.All)]
	public class CollectionViewGrouping : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			Device.SetFlags(new List<string>(Device.Flags ?? new List<string>()) { "CollectionView_Experimental" });

			PushAsync(new GalleryPages.CollectionViewGalleries.GroupingGalleries.ObservableGrouping());
#endif
		}

#if UITEST
		[Test]
		public void RemoveSelectedItem()
		{
			RunningApp.WaitForElement("Hawkeye");
			RunningApp.Tap("Hawkeye");	
			RunningApp.Tap("RemoveItem");
			RunningApp.WaitForNoElement("Hawkeye");
		}

		[Test]
		public void AddItem()
		{
			RunningApp.WaitForElement("Hawkeye");
			RunningApp.Tap("Hawkeye");
			RunningApp.Tap("AddItem");
			RunningApp.WaitForElement("Spider-Man");
		}

		[Test]
		public void ReplaceItem()
		{
			RunningApp.WaitForElement("Iron Man");
			RunningApp.Tap("Iron Man");
			RunningApp.Tap("ReplaceItem");
			RunningApp.WaitForNoElement("Iron Man");
			RunningApp.WaitForElement("Spider-Man");
		}

		[Test]
		public void RemoveGroup()
		{
			RunningApp.WaitForElement("Avengers");
			RunningApp.Tap("RemoveGroup");
			RunningApp.WaitForNoElement("Avengers");
		}

		[Test]
		public void AddGroup()
		{
			RunningApp.WaitForElement("AddGroup");
			RunningApp.Tap("AddGroup");
			RunningApp.WaitForElement("Excalibur");
		}

		[Test]
		public void ReplaceGroup()
		{
			RunningApp.WaitForElement("Fantastic Four");
			RunningApp.Tap("ReplaceGroup");
			RunningApp.WaitForElement("Alpha Flight");
		}

		[Test]
		public void MoveGroup()
		{
			RunningApp.WaitForElement("MoveGroup");
			RunningApp.Tap("MoveGroup");
		}
#endif
	}
}
