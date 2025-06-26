﻿#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // The ViewCell in TableView does not resize on iOS and MacCatalyst.More Information:https://github.com/dotnet/maui/issues/23319
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2842 : _IssuesUITest
	{
		public Issue2842(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ViewCell in TableView not adapting to changed size on iOS";

		[Fact]
		[Trait("Category", UITestCategories.TabbedPage)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Issue2842Test()
		{
			App.WaitForElement("btnClick");
			App.Tap("btnClick");
			VerifyScreenshot();
		}
	}
}
#endif