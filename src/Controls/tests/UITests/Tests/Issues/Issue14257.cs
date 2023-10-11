using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue14257 : _IssuesUITest
	{
		public Issue14257(TestDevice device) : base(device) { }

		public override string Issue => "VerticalStackLayout inside Scrollview: Button at the bottom not clickable on IOS";

		[Test]
		public void ResizeScrollViewAndTapButtonTest()
		{
			// Tapping the Resize button will change the height of the ScrollView content
			App.Click("Resize");

			// Scroll down to the Test button. When the bug is present, the button cannot be tapped.
			App.ScrollTo("Test");
			App.Click("Test");

			// If we can successfully tap the button, the Success label will be displayed
			Assert.IsTrue(App.WaitForTextToBePresentInElement("Result", "Success"));
		}
	}
}
