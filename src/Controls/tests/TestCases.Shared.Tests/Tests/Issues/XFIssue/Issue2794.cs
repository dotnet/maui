#if ANDROID
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

	// [Test]
	// [Category(UITestCategories.TableView)]
	// [FailsOnAndroid]
	// public void Issue2794Test()
	// {
	// 	App.TouchAndHold(x => x.Marked("Cell2"));
	// 	App.Tap(x => x.Text("Delete me first"));
	// 	App.WaitForNoElement(q => q.Marked("Cell2"));

	// 	App.TouchAndHold(x => x.Marked("Cell1"));
	// 	App.Tap(x => x.Text("Delete me after"));
	// 	App.WaitForNoElement(q => q.Marked("Cell1"));
	// }
}
#endif