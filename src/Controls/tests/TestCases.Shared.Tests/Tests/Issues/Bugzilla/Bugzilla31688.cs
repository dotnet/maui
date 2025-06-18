using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla31688 : _IssuesUITest
	{
		public Bugzilla31688(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "'Navigation.InsertPageBefore()' does not work for more than two pages, \"throws java.lang.IndexOutOfBoundsException: index=3 count=2";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla31688Test()
		{
			App.WaitForElement("Page3");
		}
	}
}