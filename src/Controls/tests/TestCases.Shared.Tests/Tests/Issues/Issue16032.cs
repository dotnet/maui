#if ANDROID
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16032 : _IssuesUITest
	{
		public Issue16032(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Improve the customization of WebView on Android";

		[Fact]
		[Trait("Category", UITestCategories.WebView)]
		public void EnsureSupportForCustomWebViewClients()
		{
			VerifyInternetConnectivity();
			App.WaitForElement("Success");
		}
	}
}
#endif