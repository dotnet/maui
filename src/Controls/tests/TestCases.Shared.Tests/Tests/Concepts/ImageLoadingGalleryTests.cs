using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.Image)]
	public class ImageLoadingGalleryTests : CoreGalleryBasePageTest
	{

		const string ImageLoadingGallery = "Image Loading Gallery";

		public override string GalleryPageName => ImageLoadingGallery;
		public ImageLoadingGalleryTests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(ImageLoadingGallery);
		}

		[Test]
		public void LoadAndVerifyPng() => LoadAndVerify(Test.ImageLoading.FromBundlePng);

		[Test]
		public void LoadAndVerifyJpg() => LoadAndVerify(Test.ImageLoading.FromBundleJpg);

		[Test]
		public void LoadAndVerifyGif() => LoadAndVerify(Test.ImageLoading.FromBundleGif);

		[Test]
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
