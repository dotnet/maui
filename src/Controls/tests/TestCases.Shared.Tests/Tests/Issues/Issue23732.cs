using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23732 : _IssuesUITest
	{
		public Issue23732(TestDevice testDevice) : base(testDevice)
		{
		}
		public override string Issue => "TabBar content not displayed properly";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void TabbedPageTabContentUpdated()
		{
#if ANDROID
			string pageTitle = "PAGE 4";
#else
			string pageTitle = "Page 4";
#endif
			App.WaitForElement(pageTitle);
			App.Tap(pageTitle);
			var label = App.WaitForElement("label");
			Assert.That(label.GetText(), Is.EqualTo("page4"));
		}
	}
}