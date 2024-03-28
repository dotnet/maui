#if IOS
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue7313 : IssuesUITest
	{
		public Issue7313(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView RefreshControl Not Hiding";

		[Test]
		[Category(UITestCategories.ListView)]
		public void RefreshControlTurnsOffSuccessfully()
		{
			RunningApp.WaitForNoElement("If you see the refresh circle this test has failed");

			RunningApp.WaitForNoElement("RefreshControl");
		}
	}
}
#endif