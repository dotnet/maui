using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2954 : _IssuesUITest
{
	public Issue2954(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Cell becomes empty after adding a new one with context actions (TableView) ";

	// [Test]
	// [Category(UITestCategories.TableView)]
	// [FailsOnIOS]
	// public void Issue2954Test()
	// {
	// 	App.Screenshot("I am at Issue 2954");
	// 	App.WaitForElement(q => q.Marked("Cell2"));
	// 	App.Screenshot("I see the Cell2");
	// 	App.Tap(c => c.Marked("Add new"));
	// 	App.WaitForElement(q => q.Marked("Cell2"));
	// 	App.Screenshot("I still see the Cell2");
	// }
}