using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8295 : _IssuesUITest
	{
		const string ToggleGlyphButton = "ToggleGlyphBtn";
		public Issue8295(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Can't Change ToolbarItem FontIconSource Glyph After Load";

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarItemFontIconSourceChangesAtRunTime()
		{
			App.WaitForElement(ToggleGlyphButton);
			App.Tap(ToggleGlyphButton);
			VerifyScreenshot();
		}
	}
}