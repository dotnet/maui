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
	// 	RunningApp.WaitForElement(q => q.Marked(A));
	// 	RunningApp.Tap(q => q.Marked(A));

	// 	Assert.AreEqual(A, RunningApp.WaitForElement(q => q.Marked(lblItem))[0].ReadText());
	// 	Assert.AreEqual(Group_1, RunningApp.WaitForElement(q => q.Marked(lblGroup))[0].ReadText());

	// 	RunningApp.Tap(q => q.Marked(B));

	// 	Assert.AreEqual(B, RunningApp.WaitForElement(q => q.Marked(lblItem))[0].ReadText());
	// 	Assert.AreEqual(Group_1, RunningApp.WaitForElement(q => q.Marked(lblGroup))[0].ReadText());

	// 	RunningApp.Tap(q => q.Marked(C));

	// 	Assert.AreEqual(C, RunningApp.WaitForElement(q => q.Marked(lblItem))[0].ReadText());
	// 	Assert.AreEqual(Group_2, RunningApp.WaitForElement(q => q.Marked(lblGroup))[0].ReadText());

	// 	RunningApp.Tap(q => q.Marked(D));

	// 	Assert.AreEqual(D, RunningApp.WaitForElement(q => q.Marked(lblItem))[0].ReadText());
	// 	Assert.AreEqual(Group_2, RunningApp.WaitForElement(q => q.Marked(lblGroup))[0].ReadText());
	// }
}