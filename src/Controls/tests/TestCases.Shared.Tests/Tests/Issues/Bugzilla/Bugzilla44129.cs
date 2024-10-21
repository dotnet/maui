using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla44129 : _IssuesUITest
{

	public Bugzilla44129(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Crash when adding tabbed page after removing all pages using DataTemplates";

	// [Test]
	// [Category(UITestCategories.TabbedPage)]
	// public void Issue44129Test()
	// {
	// 	App.Screenshot("I am at Issue 1");
	// 	App.WaitForElement("First");
	// 	App.Screenshot("I see the Label");
	// 	App.WaitForElement("Second");
	// 	App.Tap("Second");
	// 	App.Tap("Crash Me");
	// }
}