﻿using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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