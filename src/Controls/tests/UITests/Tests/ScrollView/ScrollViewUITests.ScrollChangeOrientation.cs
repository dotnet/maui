using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(UITestCategories.ScrollView)]
	public class ScrollChangeOrientationUITests : ScrollViewUITests
	{
		public ScrollChangeOrientationUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		public void ScrollRotationRelayoutIssue()
		{
			App.Click("ScrollViewOrientation");

			App.SetOrientationLandscape();

			var buttonAutomationIds = new[] 
			{
				"Button1",
				"Button2",
				"Button3",
			};

			foreach (string buttonAutomationId in buttonAutomationIds)
				App.WaitForElement(buttonAutomationId);

			App.Screenshot("StackLayout respects rotation");

			App.SetOrientationPortrait();
		}
	}
}