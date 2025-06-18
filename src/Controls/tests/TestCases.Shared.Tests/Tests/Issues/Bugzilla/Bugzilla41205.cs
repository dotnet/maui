using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla41205 : _IssuesUITest
	{
		const string Success = "Pass";

		public Bugzilla41205(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "UWP CreateDefault passes string instead of object";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void CreateDefaultPassesStringInsteadOfObject()
		{
			App.WaitForElement(Success);
		}
	}
}