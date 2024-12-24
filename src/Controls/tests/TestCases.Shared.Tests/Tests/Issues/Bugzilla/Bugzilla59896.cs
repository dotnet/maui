using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla59896 : _IssuesUITest
	{
		const string BtnAdd = "btnAdd";

		public Bugzilla59896(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "v2.4.0: Adding inserting section to ListView causes crash IF first section is empty";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Bugzilla59896Test()
		{
			App.WaitForElement(BtnAdd);
			App.Tap(BtnAdd);
		}
	}
}