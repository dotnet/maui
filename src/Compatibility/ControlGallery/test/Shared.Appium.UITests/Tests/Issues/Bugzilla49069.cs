using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla49069 : IssuesUITest
	{
		public Bugzilla49069(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Java.Lang.ArrayIndexOutOfBoundsException when rendering long Label on Android";

		[Test]
		[Category(UITestCategories.Label)]
		[FailsOnIOS]
		public void Bugzilla49069Test()
		{
			RunningApp.WaitForElement("lblLong");
		}
	}
}