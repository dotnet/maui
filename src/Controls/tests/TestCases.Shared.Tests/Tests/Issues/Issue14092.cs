using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14092 : _IssuesUITest
{
	public Issue14092(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Disappeared was not triggered when popping a page";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void PageDisappearingTriggeredOnPop()
	{
		App.WaitForElement("firstPageButton");
		App.Tap("firstPageButton");
		App.WaitForElement("secondPageButton");
		App.Tap("secondPageButton");
		var label = App.WaitForElement("StatusLabel");
		Assert.That(label.GetText(), Is.EqualTo("Disappearing triggered when pop"));
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void PageDisappearingTriggeredOnPopToRoot()
	{
		App.WaitForElement("firstPageButton");
		App.Tap("firstPageButton");
		App.WaitForElement("secondPagePushButton");
		App.Tap("secondPagePushButton");
		App.WaitForElement("thirdPageButton");
		App.Tap("thirdPageButton");
		var label = App.WaitForElement("StatusLabel");
		Assert.That(label.GetText(), Is.EqualTo("Disappearing triggered when PopToRoot"));
	}
}