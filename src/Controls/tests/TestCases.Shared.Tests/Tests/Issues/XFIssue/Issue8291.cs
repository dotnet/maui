using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8291 : _IssuesUITest
{
	public Issue8291(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Editor - Text selection menu does not appear when selecting text on an editor placed within a ScrollView";

	//[Test]
	//[Category(UITestCategories.Editor)]
	//[FailsOnAndroid]
	//public void ContextMenuShowsUpWhenPressAndHoldTextOnEditorAndEntryField()
	//{
	//	RunningApp.TouchAndHold("PressEditor");
	//	TestForPopup();
	//	RunningApp.Tap("PressEntry");
	//	RunningApp.TouchAndHold("PressEntry");
	//	TestForPopup();
	//}

	//void TestForPopup()
	//{
	//	var result = RunningApp.QueryUntilPresent(() =>
	//	{
	//		return RunningApp.Query("Paste")
	//				.Union(RunningApp.Query("Share"))
	//				.Union(RunningApp.Query("Copy"))
	//				.Union(RunningApp.Query("Cut"))
	//				.Union(RunningApp.Query("Select All"))
	//				.ToArray();
	//	});

	//	Assert.IsNotNull(result);
	//	Assert.IsTrue(result.Length > 0);
	//}
}