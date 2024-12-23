# if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
//In iOS and Mac platforms, scroll not working.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Bugzilla45125 : _IssuesUITest
{
	const string AppearingLabelId = "appearing";
	const string DisappearingLabelId = "disappearing";
	// const string TestButtonId = "TestButtonId";

	public Bugzilla45125(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView lacks a way to get information about visible elements (such as FirstVisibleItem) to restore visual positions of elements";

	// TODO From Xamarin.UITest migration: test references variable in UI, needs to be refactored
	[Test]
	[Category(UITestCategories.ListView)]
	public void Bugzilla45125Test()
	{
		RunTest();

		App.Tap("Next");
		RunTest();

		App.Tap("Next");
		RunTest();

		App.Tap("Next");
		RunTest();
	}

	void RunTest()
	{
		App.WaitForElement(AppearingLabelId);
		App.WaitForElement(DisappearingLabelId);
		var appearing = int.Parse(App.FindElement(AppearingLabelId)?.GetText() ?? "0");
		var disappearing = int.Parse(App.FindElement(DisappearingLabelId)?.GetText() ?? "0");

		Assert.That(appearing, Is.GreaterThan(0));
		Assert.That(disappearing, Is.GreaterThan(0));
	}
}
#endif