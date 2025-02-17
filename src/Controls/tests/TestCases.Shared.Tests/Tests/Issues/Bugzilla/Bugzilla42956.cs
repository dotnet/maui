using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla42956 : _IssuesUITest
	{
		const string Success = "Success";

		public Bugzilla42956(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView with DataTemplateSelector can have only 17 Templates, even with CachingStrategy=RetainElement";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Bugzilla42956Test()
		{
			App.WaitForElement(Success);
		}
	}
}