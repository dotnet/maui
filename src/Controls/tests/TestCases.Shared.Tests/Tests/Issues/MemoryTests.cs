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
			VerifyInternetConnectivity();
			App.WaitForElement("DataTypeEntry");
			App.EnterText("DataTypeEntry", "DatePicker");
			App.Tap("RunMemoryTestButton");
			App.AssertMemoryTest();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		public void WebViewDoesNotLeak()
		{
			VerifyInternetConnectivity();
			App.WaitForElement("DataTypeEntry");
			App.EnterText("DataTypeEntry", "WebView");
			App.Tap("RunMemoryTestButton");
			App.AssertMemoryTest();
		}
	}
}