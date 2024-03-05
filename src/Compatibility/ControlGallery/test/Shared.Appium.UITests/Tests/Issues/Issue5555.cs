using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue5555 : IssuesUITest
	{
		const string Success = "Success";

		public Issue5555(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Memory leak when SwitchCell or EntryCell";

		[Test]
		[Category(UITestCategories.ListView)]
		public void BindingToValuesTypesAndScrollingNoCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);
			RunningApp.Tap("PushPageButton");
			RunningApp.WaitForElement("PushPageButton");
			RunningApp.Tap("PushPageButton");
			RunningApp.WaitForElement("PushPageButton");

			RunningApp.WaitForNoElement("You can check result");
			RunningApp.Tap("CheckResultButton");

			RunningApp.WaitForNoElement(Success);
		}
	}
}