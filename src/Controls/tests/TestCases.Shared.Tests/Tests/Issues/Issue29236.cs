#if IOS //This issue only occurs on iOS - window dimensions not updating after orientation changes
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue29236 : _IssuesUITest
{
	public Issue29236(TestDevice device) : base(device) { }

	public override string Issue => "Window dimensions not updating after device orientation changes on iOS";

	[Test]
	[Category(UITestCategories.Window)]
	public void WindowDimensionsShouldUpdateAfterOrientationChange()
	{
		App.WaitForElement("Label");
		App.SetOrientationLandscape();
		App.WaitForElement("GetDimensionsButton").Tap();
		Assert.That(App.WaitForElement("PageWidth").GetText(), Is.EqualTo(App.WaitForElement("WindowWidth").GetText()));
		Assert.That(App.WaitForElement("PageHeight").GetText(), Is.EqualTo(App.WaitForElement("WindowHeight").GetText()));
	}
}
#endif
