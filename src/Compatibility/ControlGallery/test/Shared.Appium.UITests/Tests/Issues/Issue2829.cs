using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue2829 : IssuesUITest
	{
		const string kScrollMe = "kScrollMe";
		const string kSuccess = "SUCCESS";
		const string kCreateListViewButton = "kCreateListViewButton";

		public Issue2829(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Renderers associated with ListView cells are occasionaly not being disposed of which causes left over events to propagate to disposed views";

		[Test]
		[Category(UITestCategories.ListView)]
		public void ViewCellsAllDisposed()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Tap(kCreateListViewButton);
			RunningApp.WaitForNoElement("0");
			RunningApp.Tap(kScrollMe);
			RunningApp.WaitForNoElement("70");
			RunningApp.Back();
			RunningApp.WaitForElement(kSuccess);
		}
	}
}