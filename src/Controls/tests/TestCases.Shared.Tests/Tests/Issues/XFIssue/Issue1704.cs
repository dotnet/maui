using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1704 : _IssuesUITest
{
	#if ANDROID
	const string OnLoad="ON LOAD";
	const string OnStart ="ON START";
	const string Source="SOURCE";
	const string Misc="MISC";
#else
	const string OnLoad="On Load";
	const string OnStart ="On Start";
	const string Source="Source";
	const string Misc="Misc";
#endif
	public Issue1704(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Enhancement] Basic GIF animation features";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	[Category(UITestCategories.ManualReview)]
	public void Issue1704Test()
	{
		App.WaitForElement(OnLoad);
		App.FindElement(OnStart);
		App.FindElement(Source);
		App.FindElement(Misc);
	
		App.Tap(OnLoad);
		App.Tap(OnStart);
		App.WaitForElement("Start Animation");
		App.Tap("Start Animation");
		App.Tap("Stop Animation");
	
		App.Tap(Misc);
		App.WaitForElement("Start Animation");
		App.Tap("Start Animation");
		App.Tap("Stop Animation");
	}
}