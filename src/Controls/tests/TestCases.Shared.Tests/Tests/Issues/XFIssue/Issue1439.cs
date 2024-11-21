using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1439 : _IssuesUITest
{
	public Issue1439(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ItemTapped event for a grouped ListView is not working as expected.";

	// [Test]
	// [Category(UITestCategories.TableView)]
	// [FailsOnIOS]
	// public void Issue1439Test()
	// {
	// 	App.WaitForElement(q => q.Marked(A));
	// 	App.Tap(q => q.Marked(A));

	// 	Assert.AreEqual(A, App.WaitForElement(q => q.Marked(lblItem))[0].ReadText());
	// 	Assert.AreEqual(Group_1, App.WaitForElement(q => q.Marked(lblGroup))[0].ReadText());

	// 	App.Tap(q => q.Marked(B));

	// 	Assert.AreEqual(B, App.WaitForElement(q => q.Marked(lblItem))[0].ReadText());
	// 	Assert.AreEqual(Group_1, App.WaitForElement(q => q.Marked(lblGroup))[0].ReadText());

	// 	App.Tap(q => q.Marked(C));

	// 	Assert.AreEqual(C, App.WaitForElement(q => q.Marked(lblItem))[0].ReadText());
	// 	Assert.AreEqual(Group_2, App.WaitForElement(q => q.Marked(lblGroup))[0].ReadText());

	// 	App.Tap(q => q.Marked(D));

	// 	Assert.AreEqual(D, App.WaitForElement(q => q.Marked(lblItem))[0].ReadText());
	// 	Assert.AreEqual(Group_2, App.WaitForElement(q => q.Marked(lblGroup))[0].ReadText());
	// }
}