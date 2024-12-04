using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8145 : _IssuesUITest
{
	public Issue8145(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell System.ObjectDisposedException: 'Cannot access a disposed object. Object name: 'Android.Support.Design.Widget.BottomSheetDialog'.'";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//#if !(ANDROID || IOS)
	//	[Ignore("Shell test is only supported on Android and iOS")]
	//#endif
	//	[FailsOnAndroidWhenRunningOnXamarinUITest]
	//	public void Issue8145ShellToolbarDisposedException()
	//	{
	//		App.WaitForElement("More");
	//		App.Tap("More");
	//		App.WaitForElement("target");
	//		App.Tap("target");
	//		App.WaitForElement("Success");
	//	}
}