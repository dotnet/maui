using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla42277 : IssuesUITest
	{
		const string Success1 = "Success1";
		const string Success2 = "Success2";
		const string Success3 = "GroupedSuccess3";
		const string Success4 = "GroupedSuccess4";
		const string Success5 = "GroupedSuccess5";
		const string Success6 = "GroupedSuccess6";

		public Bugzilla42277(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "DataTemplate System.InvalidCastException crash in 2.3.1-pre1";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Bugzilla42277Test()
		{
			RunningApp.WaitForElement(Success1);
			RunningApp.WaitForElement(Success2);
			RunningApp.WaitForElement(Success3);
			RunningApp.WaitForElement(Success4);
			RunningApp.WaitForElement(Success5);
			RunningApp.WaitForElement(Success6);
		}
	}
}