using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2617 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue2617(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Error on binding ListView with duplicated items";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		[FailsOnIOS]
		[FailsOnMac]
		public async Task BindingToValuesTypesAndScrollingNoCrash()
		{
			await Task.Delay(4000);
			App.WaitForNoElement(Success);
		}
	}
}