using NUnit.Framework;
using OpenQA.Selenium.Appium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19295 : _IssuesUITest
	{
		public Issue19295(TestDevice device) : base(device) { }

		public override string Issue => "Picker does Not Resize Automatically After Selection";

		[Test]
		public void PickerShouldResizeAfterChangingSelection()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });

			App.WaitForElement("picker");

			var picker = App.FindElement("picker");
			var sizeWithFirstElementSelected = picker.GetRect().Width;

			picker.Click();
			((AppiumApp)App)?.Driver.FindElement(MobileBy.ClassName("XCUIElementTypePickerWheel")).SendKeys("Japanese Macaque");
			var sizeWithSecondElementSelected = picker.GetRect().Width;

			Assert.AreNotEqual(sizeWithFirstElementSelected, sizeWithSecondElementSelected, "Picker size did not change after selecting an item");
		}
	}
}
