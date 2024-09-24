#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla36802 : _IssuesUITest
	{
		public Bugzilla36802(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] AccessoryView Partially Hidden When Using RecycleElement and GroupShortName";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		[FailsOnMac]
		public void Bugzilla36802Test()
		{
			App.WaitForElement("TestReady");
			App.Screenshot("AccessoryView partially hidden test");
		}
	}
}
#endif