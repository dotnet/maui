using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue21768 : _IssuesUITest
	{
		public override string Issue => "[iOS] BoxView auto scaling not working when layout changes";

		public Issue21768(TestDevice device) : base(device) { }

		[Test]
		public void RowHeighShouldBeCorrectlyCalculated()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });

			App.WaitForElement("row1");
			App.Click("row1");

			VerifyScreenshot();
		}
	}
}
