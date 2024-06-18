#if ANDROID
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
    public class Issue3087 : IssuesUITest
	{
		public Issue3087(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Non appcompat SwitchRenderer regression between 3.0 and 3.1";

		[Test]
		[Category(UITestCategories.Switch)]
		public void NonAppCompatBasicSwitchTest()
		{
			RunningApp.WaitForNoElement("Success");
		}
	}
}
#endif