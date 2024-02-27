using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue3001 : IssuesUITest
	{
		const string ButtonId = "ClearButton";
		const string ReadyId = "ReadyLabel";

		public Issue3001(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[macOS] Navigating back from a complex page is highly inefficient";

		[Test]
		public void Issue3001Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Windows]);

			App.WaitForElement(ButtonId);
			App.Click(ButtonId);
			App.WaitForElement(ReadyId, timeout: TimeSpan.FromSeconds(5));
		}
	}
}