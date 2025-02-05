using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17801 : _IssuesUITest
	{
		public Issue17801(TestDevice device) : base(device) { }

		public override string Issue => "ScrollView always has a scroll bar on iOS";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void NoScrollbarsTest()
		{
			App.WaitForElement("WaitForStubControl");

			// We verify the reference snapshot where the ScrollView should
			// NOT display scrollbars as are not necessary.
			VerifyScreenshot();
		}
	}
}
