#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26096 : _IssuesUITest
	{

		public Issue26096(TestDevice device) : base(device)
		{
		}

		public override string Issue => "The TabbedPage selection indicator is not updated properly when reloading the TabbedPage with a new instance";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void UpdatedSelectionIndicatorProperly()
		{
			App.WaitForElement("OpenTabbedPage");
			App.Tap("OpenTabbedPage");
			App.Tap("Page 3");
			App.Tap("Back");
			App.Tap("OpenTabbedPage");
			VerifyScreenshot();
		}
	}
}
#endif
