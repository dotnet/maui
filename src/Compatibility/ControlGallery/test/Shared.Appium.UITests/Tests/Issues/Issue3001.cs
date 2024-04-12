#if MACCATALYST
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
		[Category(UITestCategories.Navigation)]
		public void Issue3001Test()
		{
			RunningApp.WaitForElement(ButtonId);
			RunningApp.Tap(ButtonId);
			RunningApp.WaitForElement(ReadyId, timeout: TimeSpan.FromSeconds(5));
		}
	}
}
#endif