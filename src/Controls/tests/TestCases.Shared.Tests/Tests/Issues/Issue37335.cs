using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37335 : _IssuesUITest
{
	public Issue37335(TestDevice device) : base(device) { }

	public override string Issue => "Orientation property as Horizontal is not working properly in ScrollView";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void HorizontalScrollViewShouldNotScrollVertically()
	{
		App.WaitForElement("HorizontalScrollView");
		App.ScrollDown("ScrollContent");
		VerifyScreenshot();
	}
}
