﻿# if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //Assert.that are not equal because App.PressEnter() 
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Github1650 : _IssuesUITest
{
	public Github1650(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[macOS] Completed event of Entry raised on Tab key";

	[Test]
	[Category(UITestCategories.Entry)]
	public void GitHub1650Test()
	{
		App.WaitForElement("CompletedTargetEntry");
		App.Tap("CompletedTargetEntry");

		Assert.That(App.FindElement("CompletedCountLabel").GetText(), Is.EqualTo($"Completed count: {0}"));
		App.Tap("CompletedTargetEntry");
		App.PressEnter();

		Assert.That(App.FindElement("CompletedCountLabel").GetText(), Is.EqualTo($"Completed count: {1}"));
	}
}
#endif