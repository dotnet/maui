using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13474 : _IssuesUITest
	{
		public Issue13474(TestDevice device) : base(device) { }

		public override string Issue => "FontImageSource defaults and style";

		[Test]
		[Category(UITestCategories.ImageButton)]
		public void Issue13474Test()
		{
			App.WaitForElement("imageButton");

			// The test passes if font family and color is applied
			VerifyScreenshot();
		}
	}
}