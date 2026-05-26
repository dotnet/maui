using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;

namespace UITest.Appium
{
	public class AppiumCatalystSwipeActions : AppiumSwipeActions
	{
		public AppiumCatalystSwipeActions(AppiumApp appiumApp) : base(appiumApp) { }

		protected override void SwipeToRight(AppiumDriver driver, AppiumElement? element, double swipePercentage,
			int swipeSpeed, bool withInertia = true)
		{
			var position = element?.Location ?? System.Drawing.Point.Empty;
			var size = element?.Size ?? driver.Manage().Window.Size;

			int startX = (int)(position.X + (size.Width * 0.05));
			int startY = position.Y + size.Height / 2;

			int endX = (int)(position.X + (size.Width * swipePercentage));
			int endY = startY;

			var finger = new PointerInputDevice(PointerKind.Mouse);
			var sequence = new ActionSequence(finger, 0);

			sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY,
				TimeSpan.FromMilliseconds(10)));
			sequence.AddAction(finger.CreatePointerDown(MouseButton.Left));
			sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY,
				TimeSpan.FromMilliseconds(swipeSpeed)));
			sequence.AddAction(finger.CreatePointerUp(MouseButton.Left));

			driver.PerformActions(new List<ActionSequence> { sequence });
		}

		protected override void SwipeToLeft(AppiumDriver driver, AppiumElement? element, double swipePercentage,
			int swipeSpeed, bool withInertia = true)
		{
			var position = element?.Location ?? System.Drawing.Point.Empty;
			var size = element?.Size ?? driver.Manage().Window.Size;

			int startX = (int)(position.X + (size.Width * swipePercentage));
			int startY = position.Y + size.Height / 2;

			int endX = (int)(position.X + (size.Width * 0.05));
			int endY = startY;

			var finger = new PointerInputDevice(PointerKind.Mouse);
			var sequence = new ActionSequence(finger, 0);

			sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY,
				TimeSpan.FromMilliseconds(10)));
			sequence.AddAction(finger.CreatePointerDown(MouseButton.Left));
			sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY,
				TimeSpan.FromMilliseconds(swipeSpeed)));
			sequence.AddAction(finger.CreatePointerUp(MouseButton.Left));

			driver.PerformActions(new List<ActionSequence> { sequence });
		}
	}
}