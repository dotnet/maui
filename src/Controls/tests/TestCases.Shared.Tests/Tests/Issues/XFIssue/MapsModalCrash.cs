﻿#if TEST_FAILS_ON_WINDOWS // Maps are not implemented on Windows
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class MapsModalCrash : _IssuesUITest
{
	public MapsModalCrash(TestDevice testDevice) : base(testDevice)
	{
	}

	const string StartTest = "Start Test";
	const string DisplayModal = "Click Me";
	const string Success = "SuccessLabel";

	public override string Issue => "Modal Page over Map crashes application";

	[Fact]
	[Trait("Category", UITestCategories.Maps)]
	public void CanDisplayModalOverMap()
	{
		App.WaitForElement(StartTest);
		App.Tap(StartTest);
		App.WaitForElement(DisplayModal);
		App.Tap(DisplayModal);
		App.WaitForElement(Success);
	}
}
#endif