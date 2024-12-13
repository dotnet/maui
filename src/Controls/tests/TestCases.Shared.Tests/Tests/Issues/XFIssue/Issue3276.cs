﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3276 : _IssuesUITest
{
#if ANDROID
	const string First = "FIRST";
	const string Second = "SECOND";
#else
	const string First = "First";
	const string Second = "Second";
#endif

	public Issue3276(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Crashing Unknown cell parent type on ContextAction Bindings";

	[Test]
	[Category(UITestCategories.ContextActions)]
	public void Issue3276Test()
	{
		App.Tap(Second);
		App.Tap(First);
		App.WaitForElement("second 1");
	}
}