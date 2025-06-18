using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23973(TestDevice device) : _IssuesUITest(device)
	{
		protected override bool ResetAfterEachTest => true;
		public override string Issue => "Default Modal Page Is Not Transparent";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void VerifyOpaqueModalDefault()
		{
			App.WaitForElement("PushModal");
			App.Tap("PushModal");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		public void VerifyTransparentModalShowsPageBeneathModal()
		{
			App.WaitForElement("PushTransparentModal");
			App.Tap("PushTransparentModal");
			VerifyScreenshot();
		}
	}
}
