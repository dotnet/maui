using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class LabelTextType : _IssuesUITest
{
	public LabelTextType(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Implementation of Label TextType";

	//[Test]
	//[Category(UITestCategories.Label)]
	//[FailsOnIOS]
	//public void LabelToggleHtmlAndPlainTextTest()
	//{
	//	RunningApp.WaitForElement("TextTypeLabel");
	//	RunningApp.Screenshot("I see plain text");

	//	Assert.IsTrue(RunningApp.Query("TextTypeLabel").FirstOrDefault()?.Text == "<h1>Hello World!</h1>");

	//	RunningApp.Tap("ToggleTextTypeButton");
	//	RunningApp.Screenshot("I see HTML text");

	//	Assert.IsFalse(RunningApp.Query("TextTypeLabel").FirstOrDefault()?.Text.Contains("<h1>") ?? true);
	//}
}