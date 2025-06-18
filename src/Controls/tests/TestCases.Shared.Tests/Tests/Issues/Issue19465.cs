using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19465 : _IssuesUITest
	{
		public Issue19465(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Double tap gesture NullReferenceException when navigating";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void Issue19465Test()
		{
			// 1. Navigate to a second page.
			App.WaitForElement("FirstButton");
			App.Tap("FirstButton");

			// 2. Double tap (gesture) a Label to navigate again.
			// Without exceptions, the test already passed.
			App.WaitForElement("SecondLabel");
			App.DoubleTap("SecondLabel");

			// 3. Navigate back.
			App.WaitForElement("ThirdButton");
			App.DoubleTap("ThirdButton");
			App.Back();
		}
	}
}