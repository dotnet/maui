using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25842 : _IssuesUITest
	{
		public Issue25842(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Pickers Inside CollectionView get SelectedItem Cleared on Scrolling";

		[Test]
		[Description("Verify that OnTapped is fired every time a ViewCell is tapped")]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Picker)]
		public void SelectedPickerItemsShouldNotClear()
		{
			App.WaitForElement("CollectionView");
			App.ScrollDown("CollectionView");
			App.ScrollUp("CollectionView");
			VerifyScreenshot();
		}
	}
}