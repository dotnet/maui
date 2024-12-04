﻿#if MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla58779 : _IssuesUITest
{
	const string ButtonId = "button";
	const string CancelId = "cancel";

	public Bugzilla58779(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[MacOS] DisplayActionSheet on MacOS needs scroll bars if list is long";

	[Test]
	[FailsOnIOSWhenRunningOnXamarinUITest]
	[Category(UITestCategories.DisplayAlert)]
	public void Bugzilla58779Test()
	{
		App.WaitForElement(ButtonId);
		App.Tap(ButtonId);
		App.Screenshot("Check list fits on screen");
		App.WaitForElement(CancelId);
		App.Tap(CancelId);
	}
}
#endif
