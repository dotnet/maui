using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24405 : _IssuesUITest
	{
		public Issue24405(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Entry with right aligned text keeps text jumping to the left during editing";

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyEntryHorizontalEndTextAlignmentPosition()
		{
			App.WaitForElement("button");
			App.Tap("button");
			VerifyScreenshot();
		}
	}
}