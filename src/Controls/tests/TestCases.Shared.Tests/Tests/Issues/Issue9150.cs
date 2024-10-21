using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue9150 : _IssuesUITest
	{
		public Issue9150(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Picker Attribute \"SelectedIndex\" Not being respected on page load";

		[Test]
		public void SelectedIndexShouldBeRespectedOnPageLoad()
		{
			App.WaitForElement("picker");

			//Selected item should be equal to '1'
			VerifyScreenshot();
		}
	}
}