using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17610 : _IssuesUITest
	{
		public Issue17610(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "RefreshView indicator hidden behind Navigation bar";

		[Test]
		public void Issue17610Test()
		{
			UITestContext.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Windows },
				"Is an Android issue; see https://github.com/dotnet/maui/issues/17610");

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
