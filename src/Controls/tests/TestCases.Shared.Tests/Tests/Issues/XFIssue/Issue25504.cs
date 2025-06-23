using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25504 : _IssuesUITest
	{
		public override string Issue => "ListView crashes when changing the root page inside the ItemSelected event";

		public Issue25504(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.ListView)]
		public void ListViewShouldNotCrashWhenChangingRootPage()
		{
			App.WaitForElement("listView");
			App.Tap("Label");
			var testLabel = App.WaitForElement("DetailsPage");
			Assert.That(testLabel.GetText(), Is.EqualTo("Details Page"));
		}
	}
}
