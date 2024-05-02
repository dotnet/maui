using Microsoft.Maui.AppiumTests;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue5191 : _IssuesUITest
	{
		public Issue5191(TestDevice device) : base(device) { }

		public override string Issue => "PanGesture notify Completed event moving outside View limits";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void Issue5191Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows },
				"Android Test.");

			App.WaitForElement("WaitForStubControl");

			// 1. Drag and drop.
			App.DragCoordinates(100, 500, 1000, 100);

			// 2. Verify if PanGesture reports a completed event status when the touch is lifted.
			var result = App.FindElement("WaitForStubControl").GetText();
			Assert.True(result?.Contains("Completed", StringComparison.OrdinalIgnoreCase));
		}
	}
}
