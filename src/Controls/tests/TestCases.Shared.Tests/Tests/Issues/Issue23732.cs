using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23732: _IssuesUITest
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
			App.WaitForElement("PAGE 4"); // Tab tile displayed as uppercase in Android.
			App.Tap("PAGE 4");
#else
			App.WaitForElement("Page 4");
			App.Tap("Page 4");
#endif
			var label = App.WaitForElement("label");
			Assert.That(label.GetText(), Is.EqualTo("page4"));
		}
	}
}