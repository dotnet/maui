#if ANDROID //related issues: https://github.com/dotnet/maui/issues/15994
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18751 : _IssuesUITest
	{
		public Issue18751(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Can scroll CollectionView inside RefreshView";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnAndroidWhenRunningOnXamarinUITest("Currently fails on Android; see https://github.com/dotnet/maui/issues/15994")]
		public async Task Issue18751Test()
		{
			App.WaitForElement("WaitForStubControl");

			// Load images.
			await Task.Delay(1000);

			// The test passes if you are able to see the image, name, and location of each monkey.
			VerifyScreenshot();
		}
	}
}
#endif