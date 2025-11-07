using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue32271 : _IssuesUITest
{
	public Issue32271(TestDevice device) : base(device) { }

	public override string Issue => "ScrollView with RTL FlowDirection and Horizontal Orientation scrolls in the wrong direction on iOS";
	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewDirection()
	{
		App.WaitForElement("ToggleOrientationButton");
		App.Tap("ToggleOrientationButton");
		App.Tap("ScrollToEndButton");
		Assert.That(App.FindElement("OffsetLabel").GetText(), Is.EqualTo("0"));
	}
}