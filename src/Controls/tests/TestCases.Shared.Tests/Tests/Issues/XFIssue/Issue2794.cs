#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2794 : _IssuesUITest
{
	public Issue2794(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TableView does not react on underlying collection change";

	[Test]
	[Category(UITestCategories.TableView)]
	public void Issue2794Test()
	{
		App.ContextActions("Cell2");
		App.Tap("Delete me first");
		App.WaitForNoElement("Cell2");

		App.ContextActions("Cell1");
		App.Tap("Delete me after");
		App.WaitForNoElement("Cell1");
	}
}
#endif