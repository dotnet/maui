using NUnit.Framework;
using UITest.Appium;

namespace UITests.Tests.Issues
{
	public class Issue3273 : IssuesUITest
	{
		public Issue3273(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Drag and drop reordering not firing CollectionChanged";

		[Test]
		public void Issue3273Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			App.WaitForElement("MoveItems");
			App.Click("MoveItems");
			App.WaitForNoElement("Success");
		}
	}
}