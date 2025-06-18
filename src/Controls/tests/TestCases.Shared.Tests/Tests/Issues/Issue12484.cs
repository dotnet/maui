#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12484 : _IssuesUITest
	{
		public Issue12484(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Unable to set ControlTemplate for TemplatedView in Xamarin.Forms version 5.0";

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		public void Issue12484ControlTemplateRendererTest()
		{
			App.WaitForNoElement("Success");
		}
	}
}
#endif