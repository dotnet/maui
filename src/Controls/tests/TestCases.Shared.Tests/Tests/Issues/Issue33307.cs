using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue33307 : _IssuesUITest
	{
		public Issue33307(TestDevice device) : base(device) { }

		public override string Issue => "The Picker is still binding to the property and reacts to data changes after the page is closed.";
		[Test]
		[Category(UITestCategories.Picker)]
		public void VerifyBackButtonTitleUpdates()
		{
			App.WaitForElement("Page1");
			App.Tap("Page1");
			App.Tap("AddItems");
			App.TapBackArrow();
			App.Tap("Page2");
			App.Tap("Add");
			App.Tap("SelectSecondItem");
			App.TapBackArrow();
			App.Tap("Page1");
			App.Tap("Delete");
			App.TapBackArrow();
			App.Tap("Page2");
			App.WaitForElement("Label");
			Assert.That(App.FindElement("Label").GetText(), Is.EqualTo("None"));
		}
	}
}
