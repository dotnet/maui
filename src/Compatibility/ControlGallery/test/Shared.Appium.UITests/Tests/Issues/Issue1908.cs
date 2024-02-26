using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue1908 : IssuesUITest
	{
		public Issue1908(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Image reuse"; 
		
		[Test]
		public void Issue1908Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("OASIS1");
			App.Screenshot("For manual review. Images load");
		}
	}
}
