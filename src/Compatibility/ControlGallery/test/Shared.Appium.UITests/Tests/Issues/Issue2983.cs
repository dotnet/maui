using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue2983 : IssuesUITest
	{
		public Issue2983(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView.Footer can cause NullReferenceException";

		[Test]
		public void TestDoesNotCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("footer");
		}
	}
}