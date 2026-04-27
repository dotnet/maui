using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.ScrollView)]
public class Issue31177 : _IssuesUITest
{
	public override string Issue => "ScrollView ScrollToAsync does not work when called from Page OnAppearing";

	public Issue31177(TestDevice device) : base(device)
	{
	}

	[Test]
	[Description("ScrollToAsync called from OnAppearing should scroll to the target position after layout")]
	public void ScrollToAsyncFromOnAppearingWorks()
	{
		App.WaitForElement("SuccessLabel");
	}
}
