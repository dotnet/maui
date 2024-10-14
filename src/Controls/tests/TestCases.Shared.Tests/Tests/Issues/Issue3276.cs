using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3276 : _IssuesUITest
{
	public Issue3276(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Crashing Unknown cell parent type on ContextAction Bindings";

	// [Test]
	// [Category(UITestCategories.ContextActions)]
	// public void Issue3276Test()
	// {
	// 	RunningApp.Tap(q => q.Marked("Second"));
	// 	RunningApp.Tap(q => q.Marked("First"));
	// 	RunningApp.WaitForElement(q => q.Marked("second 1"));
	// }
}