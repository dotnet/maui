using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16094 : _IssuesUITest
	{
		public Issue16094(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Shadows don't respect control shape";

		[Test]
		public void Issue16094Test()
		{
			// This issue is not working on net7 for the following platforms 
			// This is not a regression it's just the test being backported from net8
			UITestContext.IgnoreIfPlatforms(new[]
			{
				TestDevice.Android, TestDevice.Windows, TestDevice.iOS, TestDevice.Mac
			}, BackportedTestMessage);

			App.WaitForElement("EditorControl");
			VerifyScreenshot();
		}
	}
}
