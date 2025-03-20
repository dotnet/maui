using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25038 : _IssuesUITest
	{
		public Issue25038(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "MAUI Entry in Windows always shows ClearButton if initially hidden and shown even if ClearButtonVisibility set to Never";

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyEntryClearButtonVisibility()
		{
			App.WaitForElement("button");
			App.Tap("button");
			VerifyScreenshot();
		}
	}
}