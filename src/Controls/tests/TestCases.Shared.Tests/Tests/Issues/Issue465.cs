using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue465 : _IssuesUITest
	{
		public Issue465(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Change in Navigation.PushModal";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		public void Issue465TestsPushPopModal()
		{
			App.WaitForElement("PopPage");
			App.Screenshot("All elements exist");

			App.Tap("PopPage");
			App.WaitForElement("Popppppped");
			App.Screenshot("Popped modal successful");
		}
	}
}