using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18628 : _IssuesUITest
	{
		public Issue18628(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android/iOS] Rectangle rendering is broken";

		[Test]
		[Category(UITestCategories.Shape)]
		public void VerifyRectangleWidth()
		{
			App.WaitForElement("Label");
			VerifyScreenshot();
		}
	}
}