﻿#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20736 : _IssuesUITest
	{
		public override string Issue => "Editor control does not scroll properly on iOS when enclosed in a Border control";

		public Issue20736(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.Editor)]
		[Category(UITestCategories.Border)]
		public async Task EditorScrollingWhenEnclosedInBorder()
		{
			App.WaitForElement("editor");
			App.ScrollDown("editor", ScrollStrategy.Programmatically);
			await Task.Delay(1000);
			VerifyScreenshot();
		}
	}
}
#endif
