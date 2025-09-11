using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31139 : _IssuesUITest
	{
		public Issue31139(TestDevice device) : base(device) { }

		public override string Issue => "Binding updates from background thread should marshal to UI thread";

		[Test]
		[Category(UITestCategories.Dispatcher)]
		public void BindableUpdatesFromBackgroundThreadDoNotCrashAndPropagate()
		{
			App.WaitForElement("StatusLabel");
			Assert.That(App.WaitForTextToBePresentInElement(automationId: "StatusLabel", text: "Success", timeout: TimeSpan.FromSeconds(3)), Is.True);
		}
	}
}