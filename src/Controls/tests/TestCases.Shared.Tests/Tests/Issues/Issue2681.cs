using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue2681 : _IssuesUITest
	{
		const string NavigateToPage = "Click Me.";

		public Issue2681(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] Label inside Listview gets stuck inside infinite loop";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void ListViewDoesntFreezeApp()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			App.Tap(NavigateToPage);
			App.WaitForNoElement("3");
		}
	}
}