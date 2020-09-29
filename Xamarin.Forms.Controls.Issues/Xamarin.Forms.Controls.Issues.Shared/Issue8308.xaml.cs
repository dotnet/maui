using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 8308,
		"[Bug] [iOS] Cannot access a disposed object. Object name: 'GroupableItemsViewController`1",
		PlatformAffected.iOS)]
	public partial class Issue8308 : TestShell
	{
		public Issue8308()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
		}

#if UITEST
		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.CollectionView)]
		public void NavigatingBackToCollectionViewShouldNotCrash()
		{
			RunningApp.WaitForElement("Instructions");

			TapInFlyout("Page 2");
			RunningApp.WaitForElement("Instructions2");

			TapInFlyout("Page 1");
			RunningApp.WaitForElement("Instructions");
		}

#endif
	}
}