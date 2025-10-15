using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31177 : _IssuesUITest
{
	public Issue31177(TestDevice testDevice) : base(testDevice) { }
	public override string Issue => "ScrollView.ScrollToAsync doesn't work when called from Page.OnAppearing";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollToAsyncShouldWork()
	{
		App.WaitForElement("SuccessLabel");
	}
}