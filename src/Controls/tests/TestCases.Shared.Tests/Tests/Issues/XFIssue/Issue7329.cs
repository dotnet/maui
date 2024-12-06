using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7329 : _IssuesUITest
{
	public Issue7329(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] ListView scroll not working when inside a ScrollView";

	//[Test]
	//[Category(UITestCategories.ScrollView)]

	//[FailsOnAndroid]
	//[FailsOnIOS]
	//public void ScrollListViewInsideScrollView()
	//{
	//	if (!OperatingSystem.IsAndroidVersionAtLeast(21))
	//	{
	//		return;
	//	}

	//	App.WaitForElement("1");

	//	App.QueryUntilPresent(() =>
	//	{
	//		try
	//		{
	//			App.ScrollDownTo("30", strategy: ScrollStrategy.Gesture, swipeSpeed: 100);
	//		}
	//		catch
	//		{
	//			// just ignore if it fails so it can keep trying to scroll
	//		}

	//		return App.Query("30");
	//	});

	//	App.Query("30");
	//}
}