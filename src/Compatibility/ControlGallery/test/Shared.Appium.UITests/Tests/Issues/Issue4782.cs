using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue4782 : IssuesUITest
	{
		const string Success = "Success";

		public Issue4782(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Null drawable crashes Image Button";

		[Test]
		public void ImageButtonNullDrawable()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(Success);
		}
	}
}
