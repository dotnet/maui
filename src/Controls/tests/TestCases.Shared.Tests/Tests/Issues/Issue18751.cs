#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID //related issues: https://github.com/dotnet/maui/issues/33507, https://github.com/dotnet/maui/issues/15994
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
		public void Issue18751Test()
		{
			VerifyInternetConnectivity();

			App.WaitForElement("WaitForStubControl");

			// CollectionView uses virtualization which loads images synchronously once items are visible.
			// Unlike ListView (Issue18896) which may have variable height row rendering delays,
			// CollectionView's image loading completes quickly so retryTimeout handles any timing variance.
			VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
		}
	}
}
#endif