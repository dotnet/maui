#if IOS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33458 : _IssuesUITest
{
	public override string Issue => "SafeArea incorrectly applied when using ControlTemplate with ContentPresenter";

	public Issue33458(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void ScrollViewSafeAreaWorksWithControlTemplate()
	{
		var pageRect = App.WaitForElement("TestPage").GetRect();
		var scrollViewRect = App.WaitForElement("TestScrollView").GetRect();

		// ContentPresenter should not apply safe area - ScrollView should match page bounds
		Assert.That(scrollViewRect.Y, Is.EqualTo(pageRect.Y),
			"ScrollView Y should match Page Y - no top inset from ContentPresenter");

		Assert.That(scrollViewRect.Height, Is.EqualTo(pageRect.Height),
			$"ScrollView height should match Page height - no insets from ContentPresenter. Expected: {pageRect.Height}, Actual: {scrollViewRect.Height}");
	}
}
#endif
