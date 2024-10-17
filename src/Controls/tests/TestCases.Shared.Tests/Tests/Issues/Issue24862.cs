#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24862 : _IssuesUITest
	{
		public Issue24862(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Android - picker on hidden page opens after back navigation";

		[Test]
		[Category(UITestCategories.Picker)]
		public void VerifyPickerDoesNotOpenOnNavigation()
		{
			App.WaitForElement("MainButton");
			App.Tap("MainButton");
			App.WaitForElement("FirstPageButton");
			App.Tap("FirstPageButton");
			App.WaitForElement("GoBackButton");
			App.Tap("GoBackButton");
			VerifyScreenshot();
		}
	}
}
#endif