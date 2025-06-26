﻿using Xunit;
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

		[Fact]
		[Trait("Category", UITestCategories.Entry)]
		[Trait("Category", UITestCategories.Focus)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void ControlCanBeFocusedByUnfocusedEvent()
		{
			App.WaitForElementTillPageNavigationSettled(TheEntry);
			Thread.Sleep(3000); // In sample uses Delay to focus the entry.
			App.EnterText(TheEntry, Success);
			App.WaitForElementTillPageNavigationSettled(Success);
		}
	}
}