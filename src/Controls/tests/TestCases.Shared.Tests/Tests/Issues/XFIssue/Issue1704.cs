using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1704 : _IssuesUITest
{
	public Issue1704(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Enhancement] Basic GIF animation features";

	// [Test]
	// [Category(UITestCategories.TabbedPage)]
	// [Category(UITestCategories.ManualReview)]
	// public void Issue1704Test()
	// {
	// 	App.WaitForElement("On Load");
	// 	App.WaitForElement("On Start");
	// 	App.WaitForElement("Source");
	// 	App.WaitForElement("Misc");

	// 	App.Tap(q => q.Marked("On Load"));
	// 	App.Tap(q => q.Marked("On Start"));
	// 	App.WaitForElement(q => q.Marked("Start Animation"));
	// 	App.Tap(q => q.Marked("Start Animation"));
	// 	App.Tap(q => q.Marked("Stop Animation"));

	// 	App.Tap(q => q.Marked("Misc"));
	// 	App.WaitForElement(q => q.Marked("Start Animation"));
	// 	App.Tap(q => q.Marked("Start Animation"));
	// 	App.Tap(q => q.Marked("Stop Animation"));
	// }
}