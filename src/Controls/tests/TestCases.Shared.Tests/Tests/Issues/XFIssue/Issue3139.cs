#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3139 : _IssuesUITest
{
	public Issue3139(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "DisplayActionSheet is hiding behind Dialogs";

	// [Test]
	// [Category(UITestCategories.ActionSheet)]
	// public void Issue3139Test ()
	// {
	// 	RunningApp.WaitForElement (q => q.Marked ("Click Yes"));
	// 	RunningApp.Tap (c => c.Marked ("Yes"));
	// 	RunningApp.WaitForElement (q => q.Marked ("Again Yes"));
	// 	RunningApp.Tap (c => c.Marked ("Yes"));
	// 	RunningApp.WaitForElement(q => q.Marked("Test passed"));
	// }
}
#endif