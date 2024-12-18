using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6994 : _IssuesUITest
	{
		public Issue6994(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Regression in Xamarin.Forms 4.2.0-pre1 (Java.Lang.NullPointerException when using FastRenderers)";

		[Test]
		[Category(UITestCategories.Button)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void NullPointerExceptionOnFastLabelTextColorChange()
		{
			App.WaitForElement("Click me");
			App.Tap("Click me");
			App.WaitForElement("Success");
		}
	}
}
