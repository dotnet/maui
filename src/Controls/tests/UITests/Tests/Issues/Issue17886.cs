using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17886 : _IssuesUITest
	{
		public Issue17886(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Shadow wrong scaling";

		[Test]
		public void Issue17886Test()
		{
			UITestContext.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows },
				"See https://github.com/dotnet/maui/issues/17886");

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
