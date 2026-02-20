using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue30957 : _IssuesUITest
	{
		public Issue30957(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "FlexLayout Wrap Misalignment with Dynamically-Sized Buttons in .NET MAUI";

		[Test]
		[Category(UITestCategories.Layout)]
		public void FlexLayoutWrappingWithToleranceWorksCorrectly()
		{
			App.WaitForElement("Issue30957ToggleButton");
			App.Tap("Issue30957ToggleButton");

			var button1Rect = App.WaitForElement("Issue30957Button1").GetRect();
			var button2Rect = App.WaitForElement("Issue30957Button2").GetRect();
			var button3Rect = App.WaitForElement("Issue30957Button3").GetRect();

			// All three buttons should be on the same row (same Y position)
			Assert.That(button2Rect.Y, Is.EqualTo(button1Rect.Y), "Button1 and Button2 should have the same Y position (same row)");
			Assert.That(button3Rect.Y, Is.EqualTo(button1Rect.Y), "Button1 and Button3 should have the same Y position (same row)");

			// Buttons should be laid out horizontally: each subsequent button starts after the previous one
			Assert.That(button2Rect.X, Is.GreaterThan(button1Rect.X), "Button2 should be to the right of Button1");
			Assert.That(button3Rect.X, Is.GreaterThan(button2Rect.X), "Button3 should be to the right of Button2");
		}
	}
}