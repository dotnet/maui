using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31520 : _IssuesUITest
{
	public Issue31520(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "NavigatingFrom is triggered first when using PushAsync";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void NavigatingFromTriggeredFirst()
	{
		App.WaitForElement("PushButton");
		App.Tap("PushButton");
		var label = App.WaitForElement("NavigatingStatusLabel");
		Assert.That(label.GetText(), Is.EqualTo("NavigatingFrom triggered before Disappearing"));
	}
}