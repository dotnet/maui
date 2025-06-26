﻿#if IOS //This sample is working in IOS platform only due to the use ofiOS-specific modal presentation style.
using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    internal class Issue23411 : _IssuesUITest
	{
		public override string Issue => "[iOS]Use non-overridden traits in AppInfoImplementation.RequestTheme";

		public Issue23411(TestDevice device) : base(device)
		{
		}

		[Fact]
		[Trait("Category", UITestCategories.Page)]
		public void ThemeUnspecifiedDoesNotAffectModalPageSheet()
		{
			// Is a iOS issue; see https://github.com/dotnet/maui/issues/23411
			App.WaitForElement("ModalPageButton");
			App.Tap("ModalPageButton");
			App.WaitForElement("PageSheetModalPage");
			App.WaitForElement("ResetThemePageButton");
			App.Tap("ResetThemePageButton");
			VerifyScreenshot();
		}
	}
}
#endif
