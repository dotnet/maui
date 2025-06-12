#if TEST_FAILS_ON_WINDOWS //related issues: https://github.com/dotnet/maui/issues/15994
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
		public async Task Issue18751Test()
		{
			VerifyInternetConnectivity();

			App.WaitForElement("WaitForStubControl");

			// Load images.
			await Task.Delay(1000);

			// The test passes if you are able to see the image, name, and location of each monkey.
			VerifyScreenshot();
		}
	}
}
#endif