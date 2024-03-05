using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue3390 : IssuesUITest
	{
		public Issue3390(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Crash/incorrect behavior with corner radius 5";

		[Test]
		[Category(UITestCategories.Button)]
		public void Issue3390Test()
		{
			App.Click("TestButton");
			App.WaitForNoElement("Success");
		}
	}
}