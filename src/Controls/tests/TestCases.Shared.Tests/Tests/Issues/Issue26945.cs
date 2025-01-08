using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26945 : _IssuesUITest
	{
		public override string Issue => "ListView ScrollTo position always remains at the start even when set to Center or End without animation";

		public Issue26945(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.ListView)]
		public void Issue26945_SelectItemPositionStart()
		{
			App.WaitForElement("StartButton");
			App.Tap("StartButton");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.ListView)]
		public void Issue26945Test_SelectItemPositionCenter()
		{
			App.WaitForElement("CenterButton");
			App.Tap("CenterButton");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.ListView)]
		public void Issue26945Test_SelectItemPositionEnd()
		{
			App.WaitForElement("EndButton");
			App.Tap("EndButton");
			VerifyScreenshot();
		}
	}
}