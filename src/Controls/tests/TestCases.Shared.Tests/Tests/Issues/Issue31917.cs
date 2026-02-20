using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31917 : _IssuesUITest
{
	public Issue31917(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "SwipeItemView and SwipeItem background doesn't update on AppTheme change (Light/Dark)";

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeItemBackgroundShouldUpdateOnAppThemeChange()
	{
		App.WaitForElement("changeThemeButton");
		App.Tap("changeThemeButton");

		Assert.That(App.FindElement("SwipeItemColorLabel").GetText(), Is.EqualTo("PASS"),
			"SwipeItem background should update when app theme changes");
		Assert.That(App.FindElement("SwipeItemViewColorLabel").GetText(), Is.EqualTo("PASS"),
			"SwipeItemView background should update when app theme changes");
	}
}
