using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue3507 : IssuesUITest
	{
		const string Success = "Success";

		public Issue3507(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] Scrollview with null content crashes on UWP";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void NullContentOnScrollViewDoesntCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			App.WaitForNoElement(Success);
			//if it doesn't crash, we're good.
		}
	}
}
