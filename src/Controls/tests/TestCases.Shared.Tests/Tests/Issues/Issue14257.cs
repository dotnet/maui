﻿using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue14257 : _IssuesUITest
	{
		public Issue14257(TestDevice device) : base(device) { }

		public override string Issue => "VerticalStackLayout inside Scrollview: Button at the bottom not clickable on IOS";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ResizeScrollViewAndTapButtonTest()
		{
			App.WaitForElement("Resize");

			// Tapping the Resize button will change the height of the ScrollView content
			App.Tap("Resize");

			// Scroll down to the Test button. When the bug is present, the button cannot be tapped.
			App.ScrollTo("Test");

			App.WaitForElement("Test");
			App.Tap("Test");

			// If we can successfully tap the button, the Success label will be displayed
			ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("Result", "Success"));
		}
	}
}
