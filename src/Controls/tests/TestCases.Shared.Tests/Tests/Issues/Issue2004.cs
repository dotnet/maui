using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2004 : _IssuesUITest
	{
		public Issue2004(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Xamarin caused by: android.runtime.JavaProxyThrowable: System.ObjectDisposedException: Cannot access a disposed object";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void NoCrashFromDisposedBitmapWhenSwitchingPages()
		{
			App.WaitForElement("Success", timeout: TimeSpan.FromSeconds(15));
		}
	}
}
