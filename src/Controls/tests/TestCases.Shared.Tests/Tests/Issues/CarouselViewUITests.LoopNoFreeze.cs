#if TEST_FAILS_ON_WINDOWS // related issue: https://github.com/dotnet/maui/issues/24482
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
			App.WaitForElement("0 item");

			App.WaitForElement(_carouselAutomationId);
			App.ScrollRight(_carouselAutomationId);

			App.WaitForElement("1 item");
			App.ScrollRight(_carouselAutomationId);


			App.WaitForElement("2 item");

			App.Click(_btnRemoveAutomationId);

			App.WaitForElement("1 item");

			App.ScrollRight(_carouselAutomationId);

			App.WaitForElement("0 item");
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest("https://github.com/dotnet/maui/issues/24482")]
		public void RemoveItemsQuickly()
		{
			App.WaitForElement("0 item");

			App.Click(_btnRemoveAllAutomationId);

			// If we haven't crashed, then the other button should be here
			App.WaitForElement(_btnRemoveAutomationId);
		}
	}
}
#endif