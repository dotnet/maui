using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36816 : _IssuesUITest
{
	public Issue36816(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Android: Clicks being passed through to controls under other Views";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void ParentTapShouldWorkBeforeAndAfterContentViewTransparencyChanges()
	{
		var bounds = App.WaitForElement("ParentGrid").GetRect();
		App.TapCoordinates(bounds.X + 10, bounds.Y + 10);
		Assert.That(App.FindElement("ParentTapCount").GetText(), Is.EqualTo("1"));

		var labelBounds = App.WaitForElement("CardLabel").GetRect();
		App.Tap("ToggleTransparency");
		Assert.That(App.FindElement("ToggleTransparency").GetText(), Is.EqualTo("InputTransparent is true"));
		App.TapCoordinates(labelBounds.X + labelBounds.Width / 2, labelBounds.Y + labelBounds.Height / 2);
		Assert.That(App.FindElement("ParentTapCount").GetText(), Is.EqualTo("2"));
	}
}
