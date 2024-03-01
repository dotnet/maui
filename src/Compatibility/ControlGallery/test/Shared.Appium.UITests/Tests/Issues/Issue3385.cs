using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue3385 : IssuesUITest
	{
		public Issue3385(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Entry's TextChanged event is fired on Unfocus even when no text changed";

		[Test]
		public void Issue3385Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("entry");
			App.Click("entry");
			App.WaitForElement("click");
			App.Click("click");
			App.WaitForNoElement("FAIL");
		}
	}
}