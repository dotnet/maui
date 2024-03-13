using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue12484 : IssuesUITest
	{
		public Issue12484(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Unable to set ControlTemplate for TemplatedView in Xamarin.Forms version 5.0";

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		public void Issue12484ControlTemplateRendererTest()
		{ 
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForNoElement("Success");
		}
	}
}