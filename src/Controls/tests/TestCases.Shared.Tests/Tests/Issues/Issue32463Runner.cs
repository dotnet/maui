using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Issue32463Runner : _IssuesUITest
{
	public Issue32463Runner(TestDevice device) : base(device) { }

	public override string Issue => "Direct test launch";
	protected override bool UseDirectLaunch => true;

	[Test]
	[Category(UITestCategories.LifeCycle)]
	public void FixtureLaunchesDirectly()
	{
		App.WaitForElement("DirectLaunchLabel");
		Assert.That(App.FindElement("StartupWindowCount").GetText(), Is.EqualTo("0"));
		Assert.That(App.FindElements("GoToTestButton"), Is.Empty);
	}

	[Test]
	[Category(UITestCategories.LifeCycle)]
	public void ResetReturnsToCurrentTest()
	{
		App.WaitForElement("IncrementCounter");
		App.Tap("IncrementCounter");
		Assert.That(App.FindElement("LaunchCounter").GetText(), Is.EqualTo("1"));
		Reset();
		App.WaitForElement("LaunchCounter");
		Assert.That(App.FindElement("LaunchCounter").GetText(), Is.EqualTo("0"));
		Assert.That(App.FindElement("StartupWindowCount").GetText(), Is.EqualTo("0"));
		Assert.That(App.FindElements("GoToTestButton"), Is.Empty);
	}
}

public class Issue32463LegacyNavigationControl : _IssuesUITest
{
	public Issue32463LegacyNavigationControl(TestDevice device) : base(device) { }

	public override string Issue => "Direct test launch";
	protected override bool UseDirectLaunch => false;

	[Test]
	[Category(UITestCategories.LifeCycle)]
	public void GalleryNavigationConstructsPageAfterWindowCreation()
	{
		App.WaitForElement("DirectLaunchLabel");
		Assert.That(App.FindElement("StartupWindowCount").GetText(), Is.EqualTo("1"));
	}
}

public class Issue32463ResetEachTest : _IssuesUITest
{
	public Issue32463ResetEachTest(TestDevice device) : base(device) { }

	public override string Issue => "Direct test launch";
	protected override bool UseDirectLaunch => true;
	protected override bool ResetAfterEachTest => true;

	[TestCase(1)]
	[TestCase(2)]
	[TestCase(3)]
	[Category(UITestCategories.LifeCycle)]
	public void EachTestStartsFresh(int iteration)
	{
		App.WaitForElement("LaunchCounter");
		Assert.That(App.FindElement("LaunchCounter").GetText(), Is.EqualTo("0"));
		Assert.That(App.FindElement("StartupWindowCount").GetText(), Is.EqualTo("0"));
		App.Tap("IncrementCounter");
		Assert.That(App.FindElement("LaunchCounter").GetText(), Is.EqualTo("1"));
	}
}

public class Issue32463GalleryRunner : _GalleryUITest
{
	public Issue32463GalleryRunner(TestDevice device) : base(device) { }

	public override string GalleryPageName => "Button Feature Matrix";
	protected override bool UseDirectLaunch => true;

	[Test]
	[Category(UITestCategories.LifeCycle)]
	public void FixtureLaunchesGalleryDirectly()
	{
		App.WaitForElement("ButtonControl");
		Assert.That(App.FindElements("GoToTestButton"), Is.Empty);
		App.Tap("ButtonControl");
		Assert.That(App.FindElement("ButtonControl").GetText(), Is.EqualTo("Command Executed"));
	}
}
