#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21948 : _IssuesUITest
	{
		public Issue21948(TestDevice device) : base(device) { }

		public override string Issue => "Crash upon resuming the app window was already activated";

		[Test]
		[Category(UITestCategories.Window)]
		public void OpenAlertWithModals()
		{
			App.WaitForElement("button");
			App.Click("button");

			// The test passes if no exception is thrown
			App.WaitForElement("button");
		}
	}
}
#endif