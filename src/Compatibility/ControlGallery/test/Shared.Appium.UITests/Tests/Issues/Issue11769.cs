using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue11769_DelayedShellContent : IssuesUITest
	{
		public Issue11769_DelayedShellContent(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Shell throws exception when delay adding Shell Content";

		[Test]
		[Category(UITestCategories.Shell)]
		public void DelayedAddingOfShellContentDoesntCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForNoElement("Success");
		}
	}

	public class Issue11769_DelayedShellSection : IssuesUITest
	{
		public Issue11769_DelayedShellSection(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Shell throws exception when delay adding Shell Section";

		[Test]
		[Category(UITestCategories.Shell)]
		public void DelayedAddingOfShellSectionDoesntCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForNoElement("Success");
		}
	}

	public class Issue11769_DelayedShellItem : IssuesUITest
	{
		public Issue11769_DelayedShellItem(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Shell throws exception when delay adding Shell Item";

		[Test]
		[Category(UITestCategories.Shell)]
		public void DelayedAddingOfShellItemDoesntCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForNoElement("Success");
		}
	}
}