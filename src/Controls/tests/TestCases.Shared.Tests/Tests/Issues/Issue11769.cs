using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

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
		public void DelayedAddingOfShellContentDoesntCrash()
		{
			App.WaitForElement("Success");
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
		public void DelayedAddingOfShellSectionDoesntCrash()
		{
			App.WaitForElement("Success");
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
		public void DelayedAddingOfShellItemDoesntCrash()
		{
			App.WaitForElement("Success");
		}
	}
}