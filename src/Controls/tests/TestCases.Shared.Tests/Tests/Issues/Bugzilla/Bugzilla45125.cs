using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla45125 : _IssuesUITest
{
	const string AppearingLabelId = "appearing";
	const string DisappearingLabelId = "disappearing";
	const string TestButtonId = "Next";

	public Bugzilla45125(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView lacks a way to get information about visible elements (such as FirstVisibleItem) to restore visual positions of elements";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Bugzilla45125Test()
	{
		RunTest();

		App.Tap(TestButtonId);
		RunTest();

		App.Tap(TestButtonId);
		RunTest();

		App.Tap(TestButtonId);
		RunTest();
	}

	void RunTest()
	{
		App.WaitForElementTillPageNavigationSettled(AppearingLabelId);
		App.WaitForElementTillPageNavigationSettled(DisappearingLabelId);
		var appearing = int.Parse(App.FindElement(AppearingLabelId)?.GetText() ?? "0");
		var disappearing = int.Parse(App.FindElement(DisappearingLabelId)?.GetText() ?? "0");

		Assert.That(appearing, Is.GreaterThan(0));
		Assert.That(disappearing, Is.GreaterThan(0));
	}
}