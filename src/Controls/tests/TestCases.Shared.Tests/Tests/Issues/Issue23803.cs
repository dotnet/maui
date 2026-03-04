using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23803 : _IssuesUITest
	{
		public override string Issue => "FlyoutItem in overflow menu not fully interactable";

		public Issue23803(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.Shell)]
		public void VerifyClickAroundOverflowMenuItem()
		{
			App.Tap("More");
#if WINDOWS
			var rect = App.WaitForElement("Tab18").GetRect();
			App.TapCoordinates(rect.X + 80, rect.Y + 15);
			App.WaitForElement("Button18");
#else
			App.WaitForElement("Tab6");
			App.Tap("Tab6");
			App.WaitForElement("Button6");
#endif
		}
	}
}