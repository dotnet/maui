using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1146 : _IssuesUITest
	{
		public Issue1146(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Disabled Switch in Button Gallery not rendering on all devices";

		[Test]
		[Category(UITestCategories.Switch)]
		[Category(UITestCategories.Compatibility)]
		public void TestSwitchDisable()
		{
			App.WaitForElement("switch");
		}
	}
}