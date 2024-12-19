using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.Fonts)]
	public class FontsGalleryTests : CoreGalleryBasePageTest
	{
		public FontsGalleryTests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery("Fonts Gallery");
		}

		[Test]
		public void FromEmbedded_Image() => LoadAndVerify(Test.Fonts.FromEmbedded_Image);

		[Test]
		public void FromEmbedded_Label() => LoadAndVerify(Test.Fonts.FromEmbedded_Label);

		[Test]
		public void FromBundle_Image() => LoadAndVerify(Test.Fonts.FromBundle_Image);

		[Test]
		public void FromBundle_Label() => LoadAndVerify(Test.Fonts.FromBundle_Label);

		void LoadAndVerify(Test.Fonts test)
		{
			var remote = new EventViewContainerRemote(UITestContext, test);
			remote.GoTo(test.ToString());

			App.WaitForElement($"{test}VisualElement");

			VerifyScreenshot();
		}
	}
}
