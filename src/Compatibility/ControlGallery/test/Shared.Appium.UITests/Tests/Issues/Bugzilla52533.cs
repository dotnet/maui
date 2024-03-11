using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla52533 : IssuesUITest
	{
		const string LabelId = "label";

		public Bugzilla52533(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "System.ArgumentException: NaN is not a valid value for width";

		[Test]
		[Category(UITestCategories.Label)]
		public void Bugzilla52533Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(LabelId);
		}
	}
}