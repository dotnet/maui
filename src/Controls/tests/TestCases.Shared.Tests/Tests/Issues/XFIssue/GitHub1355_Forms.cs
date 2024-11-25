using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class GitHub1355_Forms : _IssuesUITest
{
	public GitHub1355_Forms(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] ViewCell shows ContextActions on tap instead of long press";

	// [Test]
	// [Category(UITestCategories.Navigation)]
	// public void SwitchMainPageOnAppearing()
	// {
	// 	// Without the fix, this would crash. If we're here at all, the test passed.
	// 	App.WaitForElement(Success);
	// }
}