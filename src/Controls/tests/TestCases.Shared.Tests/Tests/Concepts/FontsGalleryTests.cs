using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.Fonts)]
	public class FontsGalleryTests : CoreGalleryBasePageTest
	{
		const string FontsGallery = "Fonts Gallery";

		public override string GalleryPageName => FontsGallery;
		
		public FontsGalleryTests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(FontsGallery);
		}

		[Test]
		public void FromEmbedded_Image() => LoadAndVerify(Test.Fonts.FromEmbedded_Image);

		[Test]
		public void FromEmbedded_Label() => LoadAndVerify(Test.Fonts.FromEmbedded_Label);

		[Test]
		public void FromBundle_Image() => LoadAndVerify(Test.Fonts.FromBundle_Image);

#if WINDOWS
		[Ignore("Windows App SDK 1.6 broke this test. See more details in https://github.com/dotnet/maui/issues/26749")]
#endif
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
