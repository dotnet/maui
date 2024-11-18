using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2951 : _IssuesUITest
{
	public Issue2951(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "On Android, button background is not updated when color changes ";

	//[Test]
	//[Category(UITestCategories.Button)]
	//[FailsOnMauiIOS]
	//public void Issue2951Test()
	//{
	//	App.WaitForElement("Ready");
	//	var bt = App.WaitForElement(c => c.Marked("btnChangeStatus"));

	//	var buttons = App.QueryUntilPresent(() =>
	//	{
	//		var results = App.Query("btnChangeStatus");
	//		if (results.Length == 3)
	//			return results;

	//		return null;
	//	});

	//	Assert.That(buttons.Length, Is.EqualTo(3));
	//	App.Tap(c => c.Marked("btnChangeStatus").Index(1));

	//	buttons = App.QueryUntilPresent(() =>
	//	{
	//		var results = App.Query("btnChangeStatus");
	//		if ((results[1].Text ?? results[1].Label) == "B")
	//			return results;

	//		return null;
	//	});

	//	var text = buttons[1].Text ?? buttons[1].Label;
	//	Assert.That(text, Is.EqualTo("B"));
	//	App.Tap(c => c.Marked("btnChangeStatus").Index(1));

	//	buttons = App.QueryUntilPresent(() =>
	//	{
	//		var results = App.Query("btnChangeStatus");
	//		if (results.Length == 2)
	//			return results;

	//		return null;
	//	});

	//	Assert.That(buttons.Length, Is.EqualTo(2));
	//	//TODO: we should check the color of the button
	//	//var buttonTextColor = GetProperty<Color> ("btnChangeStatus", Button.BackgroundColorProperty);
	//	//Assert.AreEqual (Color.Pink, buttonTextColor);
	//}
}