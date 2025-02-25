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

		[Test]
		[Category(UITestCategories.Navigation)]
		public void TestItDoesntCrashWithWrongIconName()
		{
			App.WaitForElement("TestLabel");
		}
	}
}