﻿using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24996 : _IssuesUITest
	{
		public Issue24996(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Changing Translation of an element causes Maui in iOS to constantly run Measure & ArrangeChildren";

		[Fact]
		[Trait("Category", UITestCategories.Layout)]
		public async Task ChangingTranslationShouldNotCauseLayoutPassOnAncestors()
		{
			var element = App.WaitForElement("Stats");
			// Tries to translate the element in different positions, on-screen and off-screen.
			for (int i = 0; i < 4; i++)
			{
				element.Tap();
				await Task.Delay(150);
				Assert.True(element.GetText()!.StartsWith("Lvl1[0/0]"));
			}
		}
	}
}