using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue181 : IssuesUITest
	{
		public Issue181(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Color not initialized for Label"; 
		
		[Test]
		[Category(UITestCategories.Label)]
		public void Issue181TestsLabelShouldHaveRedText()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("TestLabel");
			RunningApp.Screenshot("Label should have red text");
		}
	}
}