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
	//	App.WaitForElement(q => q.Button("Load"));
	//	App.Screenshot("All elements present");
	//	App.Tap(q => q.Button("Load"));

	//	App.WaitForElement(q => q.Marked("John"));
	//	App.WaitForElement(q => q.Marked("Paul"));
	//	App.WaitForElement(q => q.Marked("George"));
	//	App.WaitForElement(q => q.Marked("Ringo"));

	//	App.Screenshot("List items loaded async");
	//}
}