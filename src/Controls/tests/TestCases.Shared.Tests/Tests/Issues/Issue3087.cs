#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3087 : _IssuesUITest
	{
		public Issue3087(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Non appcompat SwitchRenderer regression between 3.0 and 3.1";

		[Test]
		[Category(UITestCategories.Switch)]
		[Category(UITestCategories.Compatibility)]
		public void NonAppCompatBasicSwitchTest()
		{
			App.WaitForNoElement("Success");
		}
	}
}
#endif