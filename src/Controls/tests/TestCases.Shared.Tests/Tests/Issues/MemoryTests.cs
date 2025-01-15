using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class MemoryTests : _IssuesUITest
	{
		public MemoryTests(TestDevice device) : base(device)
		{
		}

		protected override bool ResetAfterEachTest => true;
		public override string Issue => "Test Handlers for Memory Leaks";

		[Test]
		[Category(UITestCategories.DatePicker)]
		public void DatePickerDoesNotLeak()
		{
			App.WaitForElement("DataTypeEntry");
			App.EnterText("DataTypeEntry", "DatePicker");
			App.Tap("RunMemoryTestButton");
			App.AssertMemoryTest();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		public void WebViewDoesNotLeak()
		{
			App.WaitForElement("DataTypeEntry");
			App.EnterText("DataTypeEntry", "WebView");
			App.Tap("RunMemoryTestButton");
			App.AssertMemoryTest();
		}
		public override void TestSetup()
		{
			base.TestSetup();

			try
			{
				App.WaitForElement("NoInternetAccessLabel", timeout: TimeSpan.FromSeconds(1));
				Assert.Inconclusive("This device doesn't have internet access");
			}
			catch (TimeoutException)
			{
				// Element not found within timeout, assume internet is available
				// Continue with the test
			}
		}
	}
}