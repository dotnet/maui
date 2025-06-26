﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Trait("Category", UITestCategories.Editor)]
	[Trait("Category", UITestCategories.CustomRenderers)]
	public class Issue25684 : _IssuesUITest
	{
		public Issue25684(TestDevice device) : base(device) { }

		public override string Issue => "[WinUI] Editor width is not updated when setting styles for native view";

		[Fact]
		public void CustomStyleEditorFromPlatformViewWorks()
		{
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}