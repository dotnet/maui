#if !WINDOWS // It seems the ClearPlaceholderIcon is not supported on Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20250 : _IssuesUITest
	{
		public override string Issue => "[iOS] SearchHandler ClearPlaceholderIcon color";

		public Issue20250(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.ActivityIndicator)]
		public void VerifySearchHandlerClearPlaceholderIconColor()
		{
			App.WaitForElement("label");
			VerifyScreenshot();
		}
	}
}
#endif
