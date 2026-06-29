using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28064 : _IssuesUITest
{
	public Issue28064(TestDevice device) : base(device)
	{
	}

	public override string Issue => "TapGestureRecognizer on ScrollView background does not fire on Android";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewBackgroundTapGestureShouldFire()
	{
		App.WaitForElement("StatusLabel");
		App.Tap("TheScrollView");
		var labelText = App.WaitForElement("StatusLabel").GetText();
		Assert.That(labelText, Is.EqualTo("ScrollView Tapped"));
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewChildTapGestureShouldFire()
	{
		App.WaitForElement("ChildStatusLabel");
		App.Tap("Child1Label");
		var labelText = App.WaitForElement("ChildStatusLabel").GetText();
		Assert.That(labelText, Is.EqualTo("Child Tapped"));
	}
}
