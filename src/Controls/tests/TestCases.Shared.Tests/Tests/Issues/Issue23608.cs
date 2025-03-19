using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST
	public class Issue23608 : _IssuesUITest
	{
		public Issue23608(TestDevice device) : base(device)
		{
		}

		public override string Issue => "The checkbox's checked state color does not update when the IsEnabled property is changed dynamically";

		[Test]
		[Category(UITestCategories.CheckBox)]
		public void UpdatedIsEnabledProperty()
		{
			App.WaitForElement("Label");
			App.Click("Switch");
			VerifyScreenshot();
		}
	}
#endif
}