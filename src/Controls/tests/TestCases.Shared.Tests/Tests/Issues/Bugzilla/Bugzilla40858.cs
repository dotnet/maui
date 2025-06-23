#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40858 : _IssuesUITest
{
	public Bugzilla40858(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Long clicking a text entry in a ListView header/footer cause a crash";

	[Test]
	[Category(UITestCategories.ListView)]
	[Category(UITestCategories.Compatibility)]
	public void ListViewDoesNotCrashOnTextEntryHeaderOrFooterLongClick()
	{
		App.TouchAndHold("Header");
		App.TouchAndHold("Footer");
	}
}
#endif