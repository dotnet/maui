#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22750 : _IssuesUITest
	{
		public Issue22750(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Using radiobuttons in a group, pressing one button works fine, but pressing the second does not reset the first hence";

		[Test]
		[Category(UITestCategories.RadioButton)]
		public void RadioButtonUpdateValueInsideBorder()
		{
			App.WaitForElement("WaitForStubControl");

			App.Tap("Yes");

			App.Tap("No");
			VerifyScreenshot("RadioButtonUpdateValueInsideBorderNo");

			App.Tap("Yes");
			VerifyScreenshot("RadioButtonUpdateValueInsideBorderYes");
		}
	}
}
#endif