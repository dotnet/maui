using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18751 : _IssuesUITest
	{
		public Issue18751(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Can scroll CollectionView inside RefreshView";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Ignore("This test is failing on iOS17, https://github.com/dotnet/maui/issues/20582")]
		public async Task Issue18751Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Windows },
				"Currently fails on Android and Windows; see https://github.com/dotnet/maui/issues/15994");

			App.WaitForElement("WaitForStubControl");

			// Load images.
			await Task.Delay(1000);

			// The test passes if you are able to see the image, name, and location of each monkey.
			VerifyScreenshot();
		}
	}
}
