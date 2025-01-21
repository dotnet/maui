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
		[FailsOnMacWhenRunningOnXamarinUITest("DragCoordinates methods not implemented")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("DragCoordinates methods not implemented")]
		[FailsOnAndroidWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void Issue12574Test()
		{
			App.WaitForElement("0 item");

			var rect = App.FindElement(_carouselAutomationId).GetRect();
			var centerX = rect.CenterX();
			var rightX = rect.X - 5;
			App.DragCoordinates(centerX + 40, rect.CenterY(), rightX, rect.CenterY());

			App.WaitForElement("1 item");

			App.DragCoordinates(centerX + 40, rect.CenterY(), rightX, rect.CenterY());

			App.WaitForElement("2 item");

			App.Click(_btnRemoveAutomationId);

			App.WaitForElement("1 item");

			rightX = rect.X + rect.Width - 1;
			App.DragCoordinates(rect.X, rect.CenterY(), rightX, rect.CenterY());

			App.WaitForElement("0 item");
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issu")]
		[FailsOnAndroidWhenRunningOnXamarinUITest("This test is failing, likely due to product issu")]
		public void RemoveItemsQuickly()
		{
			App.WaitForElement("0 item");

			App.Click(_btnRemoveAllAutomationId);

			// If we haven't crashed, then the other button should be here
			App.WaitForElement(_btnRemoveAutomationId);
		}
	}
}