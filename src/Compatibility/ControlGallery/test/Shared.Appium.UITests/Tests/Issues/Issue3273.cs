using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue3273 : IssuesUITest
	{
		public Issue3273(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Drag and drop reordering not firing CollectionChanged";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void Issue3273Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			RunningApp.WaitForElement("MoveItems");
			RunningApp.Tap("MoveItems");
			RunningApp.WaitForNoElement("Success");
		}
	}
}