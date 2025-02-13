using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.Image)]
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

		[Test, Retry(2)]
		public void LoadAndVerifyPng() => LoadAndVerify(Test.ImageLoading.FromBundlePng);

		[Test, Retry(2)]
		public void LoadAndVerifyJpg() => LoadAndVerify(Test.ImageLoading.FromBundleJpg);

		[Test, Retry(2)]
		public void LoadAndVerifyGif() => LoadAndVerify(Test.ImageLoading.FromBundleGif);

		[Test, Retry(2)]
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
