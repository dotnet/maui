#if TEST_FAILS_ON_WINDOWS // IndicatorView UI automation not working on Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31063 : _IssuesUITest
{
	public override string Issue => "IndicatorView remains interactive even when IsEnabled is False";

	public Issue31063(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.IndicatorView)]
	public void IndicatorViewShouldRespectIsEnabledProperty()
	{
		App.WaitForElement("TestIndicatorView");
		App.Tap("TestIndicatorView");
		App.WaitForElement("Item 1");
	}
}
#endif