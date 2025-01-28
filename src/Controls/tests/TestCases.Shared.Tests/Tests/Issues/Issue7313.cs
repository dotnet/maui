#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue7313 : _IssuesUITest
	{
		public Issue7313(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView RefreshControl Not Hiding";

		[Test]
		[Category(UITestCategories.ListView)]
		public void RefreshControlTurnsOffSuccessfully()
		{
			App.WaitForElement("If you see the refresh circle this test has failed");

			App.WaitForNoElement("RefreshControl");
		}
	}
}
#endif