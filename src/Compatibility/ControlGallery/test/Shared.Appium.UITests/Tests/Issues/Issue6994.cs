using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue6994 : IssuesUITest
	{
		public Issue6994(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Regression in Xamarin.Forms 4.2.0-pre1 (Java.Lang.NullPointerException when using FastRenderers)"; 
		
		[Test]
		[Category(UITestCategories.Button)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void NullPointerExceptionOnFastLabelTextColorChange()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("Click me");
			RunningApp.Tap("Click me");
			RunningApp.WaitForElement("Success");
		}
	}
}
