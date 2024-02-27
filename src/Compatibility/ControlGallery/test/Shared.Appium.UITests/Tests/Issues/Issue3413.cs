using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests.Tests.Issues
{
	internal class Issue3413 : IssuesUITest
	{
		public Issue3413(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Searchbar in Horizontal Stacklayout doesn't render";

		[Test]
		public void Issue3413Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("srb_vertical");
			App.WaitForElement("srb_horizontal");
			App.Screenshot("Please verify we have 2 SearchBars. One below the label, other side by side with the label");
		}
	}
}