using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16499 : _IssuesUITest
	{
		public Issue16499(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Crash when using NavigationPage.TitleView and Restarting App";

		[Test]
		public void AppDoesntCrashWhenReusingSameTitleView()
		{
#if NATIVE_AOT
			Assert.Ignore("Times out when running with NativeAOT, see https://github.com/dotnet/maui/issues/20553");
#endif
			App.WaitForElement("SuccessLabel");
		}
	}
}
