using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla26171 : IssuesUITest
	{
		public Bugzilla26171(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Microsoft.Maui.Controls.Maps is not updating VisibleRegion property when layout is changed";

		[Test]
		[Category(UITestCategories.Maps)]
		public void Bugzilla26171Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS], "To use the Maps functionality, you need to add an API key.");
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("lblValue");
		}
	}
}