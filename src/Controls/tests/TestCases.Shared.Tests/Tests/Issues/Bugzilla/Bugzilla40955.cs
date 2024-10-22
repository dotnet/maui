using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40955 : _IssuesUITest
{
	public Bugzilla40955(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Memory leak with FormsAppCompatActivity and NavigationPage";

	// TODO from Xamarin.UITest migration
	// Needs some refactoring to use AutomationIds
	// [Test]
	// [Category(UITestCategories.Performance)]
	// public void MemoryLeakInFormsAppCompatActivity()
	// {
	// 	App.WaitForElement(Page1Title);
	// 	App.Tap(LabelPage1);
	// 	App.WaitForElement(Page1Title);
	// 	App.Tap(Page2Title);
	// 	App.WaitForElement(LabelPage2);
	// 	App.Tap(LabelPage2);
	// 	App.WaitForElement(Page2Title);
	// 	App.Tap(Page3Title);
	// 	App.WaitForElement(LabelPage3);
	// 	App.Tap(LabelPage3);
	// 	App.WaitForElement(Success);
	// }
}