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
		public void NullPointerExceptionOnFastLabelTextColorChange()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("Click me");
			App.Click("Click me");
			App.WaitForElement("Success");
		}
	}
}
