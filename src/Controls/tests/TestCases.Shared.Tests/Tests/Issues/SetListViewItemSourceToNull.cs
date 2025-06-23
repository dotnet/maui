using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class SetListViewItemSourceToNull : _IssuesUITest
	{
		const string Success = "Success";
		const string Go = "Go";

		public SetListViewItemSourceToNull(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Make sure setting ItemSource to null doesn't blow up";

		[Test]
		[Category(UITestCategories.ListView)]
		public void SettingItemsSourceToNullDoesNotCrash()
		{
			App.WaitForElement(Go);
			App.Tap(Go);

			// If we can see the Success label, it means we didn't crash. 
			App.WaitForElement(Success);
		}
	}
}