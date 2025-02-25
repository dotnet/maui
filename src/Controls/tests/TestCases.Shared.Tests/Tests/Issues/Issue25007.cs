using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25007 : _IssuesUITest
	{
		public Issue25007(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ContentPage Property InputTransparent = true, causes blank page";

		[Test]
		[Category(UITestCategories.InputTransparent)]
		public void InputTransparentShouldWorkOnContentPage()
		{
			App.WaitForElement("ActionButton");
			App.Click("ActionButton");
			App.Click("EnableInputTransparentButton");
			App.Click("ActionButton");
			
			var countNumberText = App.FindElement("ActionButton").GetText();
			Assert.That(countNumberText,Is.EqualTo("Count: 1"));
		}
	}
}