#if IOS
using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class KeyboardScrollingNonScrollingPageLargeTitlesTests : CoreGalleryBasePageTest
	{
		const string KeyboardScrollingGallery = "Keyboard Scrolling Gallery - NonScrolling Page / Large Titles";
		
		public override string GalleryPageName => KeyboardScrollingGallery;

		public KeyboardScrollingNonScrollingPageLargeTitlesTests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(KeyboardScrollingGallery);
		}

		[Test]
		[ShardedTestCategory(UITestCategories.Layout, shard: 2)]
		public void EntriesScrollingPageTest()
		{
			KeyboardScrolling.EntriesScrollingTest(App, KeyboardScrollingGallery);
		}

		[Test]
		[ShardedTestCategory(UITestCategories.Layout, shard: 3)]
		public void EditorsScrollingPageTest()
		{
			KeyboardScrolling.EditorsScrollingTest(App, KeyboardScrollingGallery);
		}

		[Test]
		[ShardedTestCategory(UITestCategories.Layout, shard: 2)]
		public void EntryNextEditorTest()
		{
			KeyboardScrolling.EntryNextEditorScrollingTest(App, KeyboardScrollingGallery);
		}
	}
}
#endif