using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19754 : _IssuesUITest
	{
		public Issue19754(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Investigate adding coordinate clicking to the Tap feature for catalyst";

		[Test]
		public void TapCoordinateTest()
		{
			App.WaitForElement("WaitForStubControl");
			App.TapCoordinates(150, 150);
			var result = App.FindElement("TestLabel").GetText();
			Assert.AreEqual("Tapped", result);
		}
	}
}