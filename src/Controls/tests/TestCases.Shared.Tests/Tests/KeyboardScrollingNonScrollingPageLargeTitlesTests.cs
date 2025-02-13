﻿#if IOS
using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.Layout)]
	public class KeyboardScrollingNonScrollingPageLargeTitlesTests : CoreGalleryBasePageTest
	{
		const string KeyboardScrollingGallery = "Keyboard Scrolling Gallery - NonScrolling Page / Large Titles";
		public KeyboardScrollingNonScrollingPageLargeTitlesTests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(KeyboardScrollingGallery);
		}

		[Test, Retry(2)]
		public void EntriesScrollingPageTest()
		{
			KeyboardScrolling.EntriesScrollingTest(App, KeyboardScrollingGallery);
		}

		[Test, Retry(2)]
		public void EditorsScrollingPageTest()
		{
			KeyboardScrolling.EditorsScrollingTest(App, KeyboardScrollingGallery);
		}

		[Test, Retry(2)]
		public void EntryNextEditorTest()
		{
			KeyboardScrolling.EntryNextEditorScrollingTest(App, KeyboardScrollingGallery);
		}
	}
}
#endif