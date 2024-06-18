using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue5376 : IssuesUITest
	{
		public Issue5376(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Call unfocus entry crashes app";
		
		[Test]
		[Category(UITestCategories.Entry)]
		public void Issue5376Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForNoElement("Success");
		}
	}
}
