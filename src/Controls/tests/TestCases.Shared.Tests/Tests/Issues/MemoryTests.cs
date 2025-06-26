using Xunit;
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

		[Fact]
		[Trait("Category", UITestCategories.DatePicker)]
		public void DatePickerDoesNotLeak()
		{
			App.WaitForElement("DataTypeEntry");
			App.EnterText("DataTypeEntry", "DatePicker");
			App.Tap("RunMemoryTestButton");
			App.AssertMemoryTest();
		}

		[Fact]
		[Trait("Category", UITestCategories.WebView)]
		public void WebViewDoesNotLeak()
		{
			App.WaitForElement("DataTypeEntry");
			App.EnterText("DataTypeEntry", "WebView");
			App.Tap("RunMemoryTestButton");
			App.AssertMemoryTest();
		}

		[Fact]
		[Trait("Category", UITestCategories.Image)]
		public void ImageDoesNotLeak()
		{
			App.WaitForElement("DataTypeEntry");
			App.EnterText("DataTypeEntry", "Image");
			App.Tap("RunMemoryTestButton");
			App.AssertMemoryTest();
		}
	}
}