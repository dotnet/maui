#if TEST_FAILS_ON_IOS //Application crashes when ToolbarItem is created with invalid IconImageSource name.Issue Link: https://github.com/dotnet/maui/issues/27095
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
#endif