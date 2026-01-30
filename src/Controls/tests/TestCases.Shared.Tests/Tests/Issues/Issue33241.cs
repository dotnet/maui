using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33241 : _IssuesUITest
{
		public Issue33241(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "StackLayout fails to render content while applying Clip, and the layout is placed inside a Border with Background";

		[Test]
		[Category(UITestCategories.Border)]
		public void Issue33241Test()
		{
			App.WaitForElement("CustomView");
			VerifyScreenshot();
		}
}
