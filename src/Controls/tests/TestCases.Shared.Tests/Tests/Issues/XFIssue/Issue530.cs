using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue530 : _IssuesUITest
{
	public Issue530(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView does not render if source is async";

	//[Test]
	//[Category(UITestCategories.ListView)]
	//public void Issue530TestsLoadAsync()
	//{
	//	RunningApp.WaitForElement(q => q.Button("Load"));
	//	RunningApp.Screenshot("All elements present");
	//	RunningApp.Tap(q => q.Button("Load"));

	//	RunningApp.WaitForElement(q => q.Marked("John"));
	//	RunningApp.WaitForElement(q => q.Marked("Paul"));
	//	RunningApp.WaitForElement(q => q.Marked("George"));
	//	RunningApp.WaitForElement(q => q.Marked("Ringo"));

	//	RunningApp.Screenshot("List items loaded async");
	//}
}