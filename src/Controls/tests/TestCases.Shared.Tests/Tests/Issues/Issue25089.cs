using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue25089 : _IssuesUITest
    {
		public Issue25089(TestDevice device): base(device)
		{
		}

		public override string Issue => "OnAppearing of Page called again, although this page was on already replaced NavigationStack";

		[Test]
		[Category(UITestCategories.Shell)]
		public void VerifyPageAppearingSequence()
		{
			App.WaitForElement("MainButton");
			App.Tap("MainButton");
			App.WaitForElement("FirstPageButton");
			App.Tap("FirstPageButton");
			App.WaitForElement("SecondPageButton");
			App.Tap("SecondPageButton");
			var label = App.FindElement("StatusLabel");
			Assert.That(label.GetText(), Is.EqualTo("Page Appearing Sequence: Initial | MainPage appeared | FirstPage appeared | SecondPage appeared | MainPage appeared"));
		}
	}
}