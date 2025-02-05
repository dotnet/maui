using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1704 : _IssuesUITest
{
	const string OnLoad = "On Load";
	const string OnStart = "On Start";
	const string Source = "Source";
	const string Misc = "Misc";

	public Issue1704(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Enhancement] Basic GIF animation features";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	[Category(UITestCategories.ManualReview)]
	public void Issue1704Test()
	{
		App.WaitForTabElement(OnLoad);
		App.WaitForTabElement(OnStart);
		App.WaitForTabElement(Source);
		App.WaitForTabElement(Misc);

		App.TapTab(OnLoad);
		App.TapTab(OnStart);
		App.WaitForElement("Start Animation");
		App.Tap("Start Animation");
		App.Tap("Stop Animation");

		App.TapTab(Misc);
		App.WaitForElement("Start Animation");
		App.Tap("Start Animation");
		App.Tap("Stop Animation");
	}
}