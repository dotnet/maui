using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
    internal class Issue2842 : IssuesUITest
	{
		public Issue2842(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ViewCell in TableView not adapting to changed size on iOS";

		[Test]
		public void Issue2842Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("btnClick");
			App.Click("btnClick");
			App.Screenshot("Verify that the text is not on top of the image");
		}
	}
}
