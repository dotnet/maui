using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24174 : _IssuesUITest
	{
		public Issue24174(TestDevice device) : base(device)
		{
		}

		public override string Issue => "DatePicker does not leak";

		[Test]
		[Category(UITestCategories.DatePicker)]
		public void DatePickerDoesNotLeak()
		{
			App.AssertMemoryTest();
		}
	}
}