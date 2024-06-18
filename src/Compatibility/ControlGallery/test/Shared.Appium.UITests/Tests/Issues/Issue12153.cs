using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue12153 : IssuesUITest
	{
		public Issue12153(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Setting FontFamily to pre-installed fonts on UWP crashes";

		[Test]
		[Category(UITestCategories.Label)]
		[FailsOnIOS]
		public void InvalidFontDoesntCauseAppToCrash()
		{
			RunningApp.WaitForElement("Success");
		}
	}
}