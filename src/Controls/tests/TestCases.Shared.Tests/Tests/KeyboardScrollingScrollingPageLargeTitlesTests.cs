#if IOS
using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class KeyboardScrollingScrollingPageLargeTitlesTests : CoreGalleryBasePageTest
	{
		const string KeyboardScrollingGallery = "Keyboard Scrolling Gallery - Scrolling Page / Large Titles";

		public override string GalleryPageName => KeyboardScrollingGallery;

		public KeyboardScrollingScrollingPageLargeTitlesTests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(KeyboardScrollingGallery);
		}

		/*
		[Test]
		public void EntriesScrollingPageTest()
		{
			KeyboardScrolling.EntriesScrollingTest(App, KeyboardScrollingGallery);
		}

		[Test]
		public void EditorsScrollingPageTest()
		{
			KeyboardScrolling.EditorsScrollingTest(App, KeyboardScrollingGallery);
		}

		[Test]
		public void EntryNextEditorTest()
		{
			KeyboardScrolling.EntryNextEditorScrollingTest(App, KeyboardScrollingGallery);
		}
		*/
	}
}
#endif