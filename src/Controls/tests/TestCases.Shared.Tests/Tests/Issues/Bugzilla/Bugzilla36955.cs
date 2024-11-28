using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla36955 : _IssuesUITest
{
	public Bugzilla36955(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] ViewCellRenderer.UpdateIsEnabled referencing null object";

	// TODO from Xamarin.UITest Migration, seems to be ignored already
	// Also uses some specific XamUITest APIs that we need to find counterparts for
	// [Ignore("Test failing due to unrelated issue, disable for moment")]
	// [Category(UITestCategories.TableView)]
	// [Test]
	// public void Bugzilla36955Test()
	// {
	// 	AppResult[] buttonFalse = App.Query(q => q.Button().Text("False"));
	// 	Assert.AreEqual(buttonFalse.Length == 1, true);
	// 	App.Tap(q => q.Class("Switch"));
	// 	AppResult[] buttonTrue = App.Query(q => q.Button().Text("True"));
	// 	Assert.AreEqual(buttonTrue.Length == 1, true);
	// }
}