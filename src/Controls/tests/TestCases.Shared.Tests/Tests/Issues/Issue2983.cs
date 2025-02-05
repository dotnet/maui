using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2983 : _IssuesUITest
	{
		public Issue2983(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView.Footer can cause NullReferenceException";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void TestDoesNotCrash()
		{
			App.WaitForElement("footer");
		}
	}
}