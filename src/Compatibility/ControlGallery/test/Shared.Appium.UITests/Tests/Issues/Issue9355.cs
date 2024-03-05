using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue9355 : IssuesUITest
	{
		const string TestOk = "Test Ok";

		public Issue9355(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ScrollViewRenderer renderer dispose crash";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void Issue9355Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(TestOk);
		}
	}
}
