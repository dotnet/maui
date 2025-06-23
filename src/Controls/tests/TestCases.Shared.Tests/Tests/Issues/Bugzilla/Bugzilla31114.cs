#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID
// DragCoordinates not supported on Catalyst. 
// On IOS the cells are disapper randomly https://github.com/dotnet/maui/issues/25656, 
// On Android scroll positions not reseted on Android Issue: https://github.com/dotnet/maui/issues/26661.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla31114 : _IssuesUITest
{
	public Bugzilla31114(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS ContextAction leaves blank line after swiping in ListView";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Bugzilla31114Test()
	{
		for (int i = 0; i < 5; i++)
		{
			App.DragCoordinates(10, 500, 10, 200);
		}
		App.Tap("btnLoad");
		App.DragCoordinates(10, 500, 10, 200);
		App.WaitForElement("PIPE #1007");
		App.WaitForElement("PIPE #1008");
		App.WaitForElement("PIPE #1009");
		App.DragCoordinates(10, 500, 10, 200);
		App.WaitForElement("PIPE #1010");
		App.WaitForElement("PIPE #1011");
		App.WaitForElement("PIPE #1012");
		App.WaitForElement("PIPE #1013");
	}
}
#endif