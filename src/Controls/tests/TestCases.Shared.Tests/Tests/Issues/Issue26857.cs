
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26857 : _IssuesUITest
	{
		public override string Issue => "ListView ScrollTo position always remains at the start even when set to Center or End";

		public Issue26857(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.ListView)]
		public void Issue26857Test_SelectItemDown()
		{
			App.WaitForElement("DownButton");
			App.Tap("DownButton");
			VerifyScreenshot("DownButton");
		}

		[Test]
		[Category(UITestCategories.ListView)]
		public void Issue26857Test_SelectItemUp()
		{
			App.WaitForElement("UpButton");
			App.Tap("UpButton");
			VerifyScreenshot("UpButton");
		}
	}
}
