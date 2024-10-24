/*
using NUnit.Framework;
using UITest.Appium;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11769_DelayedShellContent : _IssuesUITest
	{
		public Issue11769_DelayedShellContent(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Shell throws exception when delay adding Shell Content";

		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		public void DelayedAddingOfShellContentDoesntCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			App.WaitForNoElement("Success");
		}
	}

	public class Issue11769_DelayedShellSection : _IssuesUITest
	{
		public Issue11769_DelayedShellSection(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Shell throws exception when delay adding Shell Section";

		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		public void DelayedAddingOfShellSectionDoesntCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			App.WaitForNoElement("Success");
		}
	}

	public class Issue11769_DelayedShellItem : _IssuesUITest
	{
		public Issue11769_DelayedShellItem(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Shell throws exception when delay adding Shell Item";

		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		public void DelayedAddingOfShellItemDoesntCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			App.WaitForNoElement("Success");
		}
	}
}
*/