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

		[TestCase("DatePicker")]
		[TestCase("WebView")]
		public void HandlerDoesNotLeak(string handler)
		{
			App.WaitForElement("DataTypeEntry");
			App.EnterText("DataTypeEntry", handler);
			App.Tap("RunMemoryTestButton");
			App.AssertMemoryTest();
		}
	}
}