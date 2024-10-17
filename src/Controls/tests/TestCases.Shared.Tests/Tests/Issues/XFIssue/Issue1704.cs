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
	// 	RunningApp.WaitForElement("On Load");
	// 	RunningApp.WaitForElement("On Start");
	// 	RunningApp.WaitForElement("Source");
	// 	RunningApp.WaitForElement("Misc");

	// 	RunningApp.Tap(q => q.Marked("On Load"));
	// 	RunningApp.Tap(q => q.Marked("On Start"));
	// 	RunningApp.WaitForElement(q => q.Marked("Start Animation"));
	// 	RunningApp.Tap(q => q.Marked("Start Animation"));
	// 	RunningApp.Tap(q => q.Marked("Stop Animation"));

	// 	RunningApp.Tap(q => q.Marked("Misc"));
	// 	RunningApp.WaitForElement(q => q.Marked("Start Animation"));
	// 	RunningApp.Tap(q => q.Marked("Start Animation"));
	// 	RunningApp.Tap(q => q.Marked("Stop Animation"));
	// }
}