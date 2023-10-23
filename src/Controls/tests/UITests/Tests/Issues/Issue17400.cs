using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17400 : _IssuesUITest
	{
		public Issue17400(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "CollectionView wrong Layout";

		[Test]
		public void Issue17400Test()
		{
			UITestContext.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Android, TestDevice.Mac },
				"Is a Windows issue; see https://github.com/dotnet/maui/issues/17400");

			App.WaitForElement("UpdateBtn");
			App.Tap("UpdateBtn");

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}