using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue2775 : IssuesUITest
	{
		public Issue2775(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ViewCell background conflicts with ListView Semi-Transparent and Transparent backgrounds";

		[Test]
		public void Issue2775Test()
		{
			App.WaitForElement("TestReady");
			App.Screenshot("I am at Issue 2775");
			App.Screenshot("I see the Label");
		}
	}
}
