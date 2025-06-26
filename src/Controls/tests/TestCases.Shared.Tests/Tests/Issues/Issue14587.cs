#if TEST_FAILS_ON_WINDOWS // On the Windows platform, the automationId of the Glyph icon is not working.
//For more information : https://github.com/dotnet/maui/issues/27702
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	class Issue14587 : _IssuesUITest
	{
		public Issue14587(TestDevice device) : base(device) { }
		public override string Issue => "ImageButton with FontImageSource becomes invisible";

		[Fact]
		[Trait("Category", UITestCategories.Image)]
		public void ImageDoesNotDisappearWhenSwitchingTab()
		{
			App.WaitForElement("Icon1");
			App.Tap("Icon2");
			App.Tap("Icon3");
			App.Tap("Icon1");
			App.Tap("ToTab2");

			App.WaitForElement("ToTab1");
			App.Tap("ToTab1");

			App.WaitForElement("Icon1");
			VerifyScreenshot();
		}
	}
}
#endif