using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22045 : _IssuesUITest
{
	public Issue22045(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] OnSizeAllocated not reported for Android AppShell Flyout content";

	[Test]
	[Category(UITestCategories.Shell)]
	public void GetValidSizeWhenContentViewInFlyoutContent()
	{
		App.WaitForElement("OpenFlyoutButton");
		App.Click("OpenFlyoutButton");
		var width = App.WaitForElement("WidthLabel").GetText();
		var height = App.WaitForElement("HeightLabel").GetText();
		Assert.That(double.TryParse(width, out double w) && w > 0,
			Is.True, $"Expected positive width, got {width}");
		Assert.That(double.TryParse(height, out double h) && h > 0,
			Is.True, $"Expected positive height, got {height}");
	}
}