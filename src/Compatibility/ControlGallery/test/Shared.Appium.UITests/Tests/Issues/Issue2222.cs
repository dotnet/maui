using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue2222 : IssuesUITest
	{
		public Issue2222(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NavigationBar.ToolbarItems.Add() crashes / breaks app in iOS7. works fine in iOS8";

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnIOS]
		public void TestItDoesntCrashWithWrongIconName()
		{
			RunningApp.WaitForElement("TestLabel");
			RunningApp.Screenshot("Was label on page shown");
		}
	}
}