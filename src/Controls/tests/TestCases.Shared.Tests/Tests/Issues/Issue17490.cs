#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17490 : _IssuesUITest
	{
		public Issue17490(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Crash using Pinvoke.SetParent to create Window as Child";

		// I've commented this test out because nunit will still navigate to this test and then just not run any of the tests
		// Just navigating to this test will crash WinUI so we want to just completely remove it
		/*
		[Test]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This broke with WinAPPSDK 1.4 and we currently don't have an alternative https://github.com/dotnet/maui/issues/20253")]
		[Category(UITestCategories.Window)]
		public void AppDoesntCrashWhenOpeningWinUIWindowParentedToCurrentWindow()
		{
			App.WaitForElement("SuccessLabel");
		}
		*/
	}
}
#endif