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

		public override string Issue => "TableView ViewCell vanishes after content is updated";
		
		[Test]
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