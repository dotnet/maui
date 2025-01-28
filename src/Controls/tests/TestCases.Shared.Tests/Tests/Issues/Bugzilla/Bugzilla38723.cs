using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla38723 : _IssuesUITest
	{
		public Bugzilla38723(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Update Content in Picker's SelectedIndexChanged event causes NullReferenceException";

		[Test]
		[Category(UITestCategories.Picker)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla38723Test()
		{
			App.WaitForElement("SELECT");
			App.Tap("SELECT");
			App.WaitForElement("Selected");
			App.WaitForNoElement("SELECT");
		}
	}
}
