using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1799 : IssuesUITest
	{
		const string ListView = "ListView1799";
		const string Success = "Success";

		public Issue1799(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] listView without data crash on ipad.";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void ListViewWithoutDataDoesNotCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			var result = RunningApp.WaitForElement(ListView);
			var listViewRect = result.GetRect();

			RunningApp.DragCoordinates(listViewRect.CenterX(), listViewRect.Y, listViewRect.CenterX(), listViewRect.Y + 50);

			RunningApp.WaitForElement(Success);
		}
	}
}