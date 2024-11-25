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
	//[FailsOnAndroidWhenRunningOnXamarinUITest]
	//public void ContextMenuShowsUpWhenPressAndHoldTextOnEditorAndEntryField()
	//{
	//	App.TouchAndHold("PressEditor");
	//	TestForPopup();
	//	App.Tap("PressEntry");
	//	App.TouchAndHold("PressEntry");
	//	TestForPopup();
	//}

	//void TestForPopup()
	//{
	//	var result = App.QueryUntilPresent(() =>
	//	{
	//		return App.Query("Paste")
	//				.Union(App.Query("Share"))
	//				.Union(App.Query("Copy"))
	//				.Union(App.Query("Cut"))
	//				.Union(App.Query("Select All"))
	//				.ToArray();
	//	});

	//	Assert.IsNotNull(result);
	//	Assert.IsTrue(result.Length > 0);
	//}
}