#if ANDROID || WINDOWS //Existing PR for iOS and mac which resolve the FlowDirection issue on TabBar https://github.com/dotnet/maui/pull/32701
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32993 : _IssuesUITest
{
	public override string Issue => "TabBar Should update correctly in RTL mode";

	public Issue32993(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Shell)]
	public void Issue32993TabBarUpdatesCorrectlyInRTLMode()
	{
		App.WaitForElement("Issue32993SetRTL");
		App.Tap("Issue32993SetRTL");
		VerifyScreenshot();
	}
}
#endif