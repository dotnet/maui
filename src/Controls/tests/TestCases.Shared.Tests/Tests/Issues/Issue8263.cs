using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8263 : _IssuesUITest
	{
		public Issue8263(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Enhancement] Add On/Off VisualStates for Switch";

		[Test]
		[Category(UITestCategories.Switch)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void SwitchOnOffVisualStatesTest()
		{
			App.WaitForElement("Switch");
			App.Screenshot("Switch Default");
			App.Tap("Switch");
			App.Screenshot("Switch Off with Red ThumbColor");
			App.Tap("Switch");
			App.Screenshot("Switch On with Green ThumbColor");
		}
	}
}
