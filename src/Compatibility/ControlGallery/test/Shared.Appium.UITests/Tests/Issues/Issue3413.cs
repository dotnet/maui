using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue3413 : IssuesUITest
	{
		public Issue3413(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Searchbar in Horizontal Stacklayout doesn't render";

		[Test]
		[Category(UITestCategories.SearchBar)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Issue3413Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("srb_vertical");
			RunningApp.WaitForElement("srb_horizontal");
			RunningApp.Screenshot("Please verify we have 2 SearchBars. One below the label, other side by side with the label");
		}
	}
}