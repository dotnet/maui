using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue19109 : _IssuesUITest
	{
		public Issue19109(TestDevice device) : base(device) { }

		public override string Issue => "CursorPosition Property Not Applied Correctly to Entry Control on iOS Platform";

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryShouldApplyCursorPositionCorrectly()
		{
			App.WaitForElement("EntryControl");
			App.WaitForElement("FocusButton");
			App.Tap("FocusButton");
			VerifyScreenshot();
		}
	}
}
