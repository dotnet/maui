using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla32615 : _IssuesUITest
	{
		public Bugzilla32615(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "OnAppearing is not called on previous page when modal page is popped";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla32615Test()
		{
			App.WaitForElement("open");
			App.Tap("open");
			App.WaitForElement("pop");
			App.Tap("pop");
			App.WaitForElement("lblCount");
		}
	}
}