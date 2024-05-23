using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3390 : _IssuesUITest
	{
		public Issue3390(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Crash/incorrect behavior with corner radius 5";

		[Test]
		[Category(UITestCategories.Button)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void Issue3390Test()
		{
			App.Tap("TestButton");
			App.WaitForNoElement("Success");
		}
	}
}