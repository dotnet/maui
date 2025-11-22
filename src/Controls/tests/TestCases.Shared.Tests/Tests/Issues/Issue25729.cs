using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25729 : _IssuesUITest
	{
		public Issue25729(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Picker Selected item gets cleared in BindableLayout";

		[Test]
		[Category(UITestCategories.Picker)]
		public void SelectedItemsShouldNotGetCleared()
		{
			App.WaitForElement("button2");
			App.Click("button2");
			App.Click("button1");

			var text = App.FindElement("label").GetText();

			Assert.That(text, Is.EqualTo("Selected Choice: Choice 1"));
		}
	}
}