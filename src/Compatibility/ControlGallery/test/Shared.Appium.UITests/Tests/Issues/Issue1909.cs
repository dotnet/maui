using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue1909 : IssuesUITest
	{
		public Issue1909(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Xamarin.forms 2.5.0.280555 and android circle button issue";
		
		[Test]
		public void Issue1909Test()
		{
			App.WaitForElement("TestReady");
			App.Screenshot("I am at Issue 1909");
		}
	}
}