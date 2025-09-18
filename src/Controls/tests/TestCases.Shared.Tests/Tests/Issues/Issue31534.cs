using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31534 : _IssuesUITest
{
	public Issue31534(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "ScrollView height was increased after the application closes";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewHeightWhenAppClose()
	{
		App.WaitForElement("MauiButton");
		App.Tap("MauiButton");
		App.Tap("MauiButton");
		App.Tap("MauiButton");
		App.WaitForElement("MauiButton");
	}
}