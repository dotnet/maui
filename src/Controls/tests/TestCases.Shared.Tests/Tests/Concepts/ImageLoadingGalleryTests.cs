using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Trait("Category", UITestCategories.Image)]
	public class ImageLoadingGalleryTests : CoreGalleryBasePageTest
	{
		public ImageLoadingGalleryTests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery("Image Loading Gallery");
		}

		[Fact]
		public void LoadAndVerifyPng() => LoadAndVerify(Test.ImageLoading.FromBundlePng);

		[Fact]
		public void LoadAndVerifyJpg() => LoadAndVerify(Test.ImageLoading.FromBundleJpg);

		[Fact]
		public void LoadAndVerifyGif() => LoadAndVerify(Test.ImageLoading.FromBundleGif);

		[Fact]
		public void LoadAndVerifySvg() => LoadAndVerify(Test.ImageLoading.FromBundleSvg);

		void LoadAndVerify(Test.ImageLoading test)
		{
			var remote = new EventViewContainerRemote(UITestContext, test);
			remote.GoTo(test.ToString());

			App.WaitForElement($"{test}VisualElement");

			VerifyScreenshot();
		}
	}
}
