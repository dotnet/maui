using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla38989 : _IssuesUITest
	{
		const string Success = "If you can see this, the test passed.";

		public Bugzilla38989(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] NullReferenceException when using a custom ViewCellRenderer ";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla38989Test()
		{
			App.WaitForElement(Success);
		}
	}
}
