﻿#if ANDROID
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5191 : _IssuesUITest
	{
		public Issue5191(TestDevice device) : base(device) { }

		public override string Issue => "PanGesture notify Completed event moving outside View limits";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void Issue5191Test()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Drag and drop.
			App.DragCoordinates(100, 500, 1000, 100);

			// 2. Verify if PanGesture reports a completed event status when the touch is lifted.
			var result = App.FindElement("WaitForStubControl").GetText();
			ClassicAssert.True(result?.Contains("Completed", StringComparison.OrdinalIgnoreCase));
		}
	}
}
#endif