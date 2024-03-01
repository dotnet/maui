using NUnit.Framework;

namespace UITests
{
	public class Issue7313 : IssuesUITest
	{
		const string kSuccess = "If you're reading this the test has passed";

		public Issue7313(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] with ListView on page, Navigation.PopAsync() throws exception";

		[Test]
		public void SettingBindingContextToNullBeforingPoppingPageCrashes()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForNoElement(kSuccess);
		}
	}
}