#if IOS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Appium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19295 : _IssuesUITest
	{
		public Issue19295(TestDevice device) : base(device) { }

		public override string Issue => "Picker does Not Resize Automatically After Selection";

		[Test]
		[Category(UITestCategories.Picker)]
		public void PickerShouldResizeAfterChangingSelection()
		{
			App.WaitForElement("picker");

			var picker = App.FindElement("picker");
			var sizeWithFirstElementSelected = picker.GetRect().Width;

			picker.Click();
			((AppiumApp)App)?.Driver.FindElement(MobileBy.ClassName("XCUIElementTypePickerWheel")).SendKeys("Japanese Macaque");
			var sizeWithSecondElementSelected = picker.GetRect().Width;

			ClassicAssert.AreNotEqual(sizeWithFirstElementSelected, sizeWithSecondElementSelected, "Picker size did not change after selecting an item");
		}
	}
}
#endif