using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla42277 : _IssuesUITest
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
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla42277Test()
		{
			App.WaitForElement(Success1);
			App.WaitForElement(Success2);
			App.WaitForElement(Success3);
			App.WaitForElement(Success4);
			App.WaitForElement(Success5);
			App.WaitForElement(Success6);
		}
	}
}