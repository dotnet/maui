using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36652 : _IssuesUITest
{
	public Issue36652(TestDevice device) : base(device) { }

	public override string Issue => "SwipeView directly inside a Border crashes natively on Windows";

	[Test]
	[Category(UITestCategories.Border)]
	public void SwipeViewInsideBorderDoesNotCrash()
	{
		App.WaitForElement("ResultLabel");
	}
}
