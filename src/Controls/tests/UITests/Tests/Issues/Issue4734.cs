using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue4734 : _IssuesUITest
	{
		public Issue4734(TestDevice device)
		: base(device)
		{ }

		public override string Issue => "Gestures in Label Spans not working";

		[Test]
		public void Issue4734Test()
		{
			if (Device == TestDevice.Mac)
			{
				Assert.Ignore("Click (x, y) pointer type mouse is not implemented.");
			}

			if (Device == TestDevice.Windows)
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}
			else
			{
				App.WaitForElement("WaitForStubControl");

				var label = App.WaitForElement("TargetSpanControl");
				var location = label.GetRect();
				var lineHeight = location.Height / 5;
				var lineCenterOffset = lineHeight / 2;
				var y = location.Y;

				App.Click(location.X + 10, y + lineCenterOffset);
				App.Click(location.X + 10, y + (lineHeight * 2) + lineCenterOffset);
				App.Click(location.X + 10, y + (lineHeight * 4) + lineCenterOffset);

				var textAfterTap = App.FindElement("TapResultControl").GetText();
				Assert.False(string.IsNullOrEmpty(textAfterTap));
			}
		}
	}
}