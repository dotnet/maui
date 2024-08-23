#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13558 : _IssuesUITest
	{
		public Issue13558(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Picker values are not displaying when loaded within a ListView";

		[Test]
		[Category(UITestCategories.ListView)]
		public void PickerValuesShouldBeDisplayedInListView()
		{
			App.WaitForElement("ListViewId");
			VerifyScreenshot();
		}
	}
}
#endif