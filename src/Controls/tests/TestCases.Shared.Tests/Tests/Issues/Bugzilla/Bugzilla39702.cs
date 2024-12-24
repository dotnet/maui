﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla39702 : _IssuesUITest
	{
		const string TheEntry = "TheEntry";
		const string Success = "Success";

		public Bugzilla39702(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Cannot enter text when Entry is focus()'d from an editor completed event";

		[Test]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Focus)]
		[Category(UITestCategories.Compatibility)]
		public void ControlCanBeFocusedByUnfocusedEvent()
		{
			App.WaitForElement(TheEntry);
			App.EnterText(TheEntry, Success);
			App.WaitForElement(Success);
		}
	}
}