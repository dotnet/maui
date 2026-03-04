#if TEST_FAILS_ON_WINDOWS // Related issue for windows: https://github.com/dotnet/maui/issues/24482
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CarouselViewLoopNoFreeze : _IssuesUITest
	{
		readonly string _carouselAutomationId = "carouselView";
		readonly string _btnRemoveAutomationId = "btnRemove";
		readonly string _btnRemoveAllAutomationId = "btnRemoveAll";
		readonly string _btnSwipeAutomationId = "btnSwipe";

		protected override bool ResetAfterEachTest => true;
		public CarouselViewLoopNoFreeze(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "CarouselView Loop=True default freezes iOS app";

		// Issue12574 (src\ControlGallery\src\Issues.Shared\Issue12574.cs
		[Test]
		[Category(UITestCategories.CarouselView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest("https://github.com/dotnet/maui/issues/24482")]
		public void Issue12574Test()
		{
			// Use longer timeout for CarouselView items which can be slow to appear on CI
			App.WaitForElement("0 item", timeout: TimeSpan.FromSeconds(30));

			App.WaitForElement(_carouselAutomationId);
			App.WaitForElement(_btnSwipeAutomationId);
			App.Tap(_btnSwipeAutomationId);

			App.WaitForElement("1 item", timeout: TimeSpan.FromSeconds(10));
			App.Tap(_btnSwipeAutomationId);


			App.WaitForElement("2 item", timeout: TimeSpan.FromSeconds(10));
			App.WaitForElement(_btnRemoveAutomationId);
			App.Tap(_btnRemoveAutomationId);

			App.WaitForElement("1 item", timeout: TimeSpan.FromSeconds(10));

			App.Tap(_btnSwipeAutomationId);

			App.WaitForElement("0 item", timeout: TimeSpan.FromSeconds(10));
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest("https://github.com/dotnet/maui/issues/24482")]
		public void RemoveItemsQuickly()
		{
			// Use longer timeout for CarouselView items which can be slow to appear on CI
			App.WaitForElement("0 item", timeout: TimeSpan.FromSeconds(30));

			App.Click(_btnRemoveAllAutomationId);

			// If we haven't crashed, then the other button should be here
			App.WaitForElement(_btnRemoveAutomationId, timeout: TimeSpan.FromSeconds(10));
		}
	}
}
#endif