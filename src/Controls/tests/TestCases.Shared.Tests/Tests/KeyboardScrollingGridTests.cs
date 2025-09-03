#if IOS
using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class KeyboardScrollingGridTests : CoreGalleryBasePageTest
	{
		const string KeyboardScrollingGallery = "Keyboard Scrolling Gallery - Grid with Star Row";

		public override string GalleryPageName => KeyboardScrollingGallery;

		public KeyboardScrollingGridTests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(KeyboardScrollingGallery);
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void GridStarRowScrollingTest()
		{
			KeyboardScrolling.GridStarRowScrollingTest(App);
		}
	}
}
#endif