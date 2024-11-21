using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25912 : _IssuesUITest
	{
		public Issue25912(TestDevice device) : base(device)
		{
		}

		public override string Issue => "ToolbarItem color when used with IconImageSource is always white";

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void VerifyToolbarItemIconColor()
		{
			App.WaitForElement("Label");
			VerifyScreenshot();
		}

	}
}