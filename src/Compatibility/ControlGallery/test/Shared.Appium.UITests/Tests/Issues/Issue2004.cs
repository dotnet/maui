using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue2004 : IssuesUITest
	{
		public Issue2004(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Xamarin caused by: android.runtime.JavaProxyThrowable: System.ObjectDisposedException: Cannot access a disposed object";
	
		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnIOS]
		public void NoCrashFromDisposedBitmapWhenSwitchingPages()
		{
			RunningApp.WaitForElement("Success", timeout: TimeSpan.FromSeconds(20));
		}
	}
}
