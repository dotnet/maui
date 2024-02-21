using Maui.Controls.Sample;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
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
			Thread.Sleep(1000); // android has some button animations that need to finish

			VerifyScreenshot();
		}
	}
}
