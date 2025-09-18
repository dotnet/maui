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
		App.WaitForElement("HeightLabel");
		var scrollViewHeight = App.FindElement("HeightLabel").GetText();
		App.CloseApp();
	    Assert.That(App.FindElement("HeightLabel").GetText(), Is.EqualTo(scrollViewHeight));
	}
}