#if IOS
using Xunit;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class KeyboardScrollingGridTests : CoreGalleryBasePageTest
	{
		const string KeyboardScrollingGallery = "Keyboard Scrolling Gallery - Grid with Star Row";
		
		public KeyboardScrollingGridTests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(KeyboardScrollingGallery);
		}

		[Fact]
		[Trait("Category", UITestCategories.Layout)]
		public void GridStarRowScrollingTest()
		{
			KeyboardScrolling.GridStarRowScrollingTest(App);
		}
	}
}
#endif