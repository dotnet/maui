using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16018 : _IssuesUITest
	{
		const string TestEntry = "WaitForStubControl";

		public Issue16018(TestDevice device) : base(device) { }

		public override string Issue => "WordWrap on labels with padding does not work properly on iOS";

		[Test]
		public void LabelWordWrapWithPaddingTest()
		{
			App.WaitForElement(TestEntry);

			// 1.Enter a long text in the Entry.
			App.EnterText(TestEntry, "1234567890123456789012345678901234567890");

			// 2. Verify that the Label is wrapping.
			VerifyScreenshot();
		}
	}
}
