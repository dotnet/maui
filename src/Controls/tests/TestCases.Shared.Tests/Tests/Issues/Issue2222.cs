using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2222 : _IssuesUITest
	{
		public Issue2222(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NavigationBar.ToolbarItems.Add() crashes / breaks app in iOS7. works fine in iOS8";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void TestItDoesntCrashWithWrongIconName()
		{
			App.WaitForElement("TestLabel");
			App.Screenshot("Was label on page shown");
		}
	}
}