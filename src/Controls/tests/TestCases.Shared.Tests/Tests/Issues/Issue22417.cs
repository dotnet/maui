#if TEST_FAILS_ON_WINDOWS //This test is failing, likely due to product issue, for more information: https://github.com/dotnet/maui/issues/27059
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22417 : _IssuesUITest
	{
		public Issue22417(TestDevice device) : base(device) { }

		public override string Issue => "[Windows] The list does not show newly added items";

		[Test]
		[Category(UITestCategories.CarouselView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest("Flaky test on Windows https://github.com/dotnet/maui/issues/27059")]
		public async Task AddItemsToCarouselViewWorks()
		{
			App.WaitForElement("WaitForStubControl");

			App.Tap("AddItemButton");

			await Task.Delay(500);

			VerifyScreenshot();
		}
	}
}
#endif