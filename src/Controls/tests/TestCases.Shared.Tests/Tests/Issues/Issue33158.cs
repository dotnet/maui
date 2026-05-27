#if TEST_FAILS_ON_WINDOWS // Existing PR for windows - https://github.com/dotnet/maui/pull/26728
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33158 : _IssuesUITest
{
	public Issue33158(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "IsEnabledProperty should work on Tabs";

	[Test]
	[Category(UITestCategories.Shell)]
	public void Issue33158CheckIsEnabled()
	{
		App.WaitForElement("ThirdTab");
		App.Tap("ThirdTab");
		App.WaitForElement("ThirdPageLabel");
		App.Tap("SecondTab");
		App.WaitForNoElement("SecondPageLabel");
		App.Tap("EnableSecondTab");
		App.Tap("SecondTab");
		App.WaitForElement("SecondPageLabel");
	}
}
#endif