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
			App.Click("PushPageButton");
			App.WaitForElement("PushPageButton");
			App.Click("PushPageButton");
			App.WaitForElement("PushPageButton");

			App.WaitForNoElement("You can check result");
			App.Click("CheckResultButton");

			App.WaitForNoElement(Success);
		}
	}
}