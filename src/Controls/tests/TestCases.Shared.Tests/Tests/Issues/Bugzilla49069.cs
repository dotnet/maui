using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla49069 : _IssuesUITest
	{
		public Bugzilla49069(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Java.Lang.ArrayIndexOutOfBoundsException when rendering long Label on Android";

		[Test]
		[Category(UITestCategories.Label)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		[FailsOnMac]
		public void Bugzilla49069Test()
		{
			App.WaitForElement("lblLong");
		}
	}
}