using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue7313 : IssuesUITest
	{
		public Issue7313(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView RefreshControl Not Hiding";

		[Test]
		public void RefreshControlTurnsOffSuccessfully()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForNoElement("If you see the refresh circle this test has failed");

			App.WaitForNoElement("RefreshControl");
		}
	}
}