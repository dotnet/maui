#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25487: _IssuesUITest
	{
		public Issue25487(TestDevice device) : base(device) { }

		public override string Issue => "iOS button flickering animation when layout updates";

		[Test]
		[Category(UITestCategories.Button)]
		public void ButtonRunTimeAlignmentWithImage()
		{
			App.WaitForElement("ToggleTextButton");
			App.Tap("ToggleTextButton");
			VerifyScreenshot();
		}

	}
}
#endif