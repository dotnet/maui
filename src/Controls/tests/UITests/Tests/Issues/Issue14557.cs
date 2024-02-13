using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	class Issue14557 : _IssuesUITest
	{
		public Issue14557(TestDevice device) : base(device) { }
		public override string Issue => "CollectionView header and footer not displaying on Windows";

		[Test]
		public void HeaderAndFooterRender()
		{
#if NATIVE_AOT
			Assert.Ignore("Times out when running with NativeAOT, see https://github.com/dotnet/maui/issues/20553");
#endif
			App.WaitForElement("collectionView");

			var headerText = App.FindElement("headerLabel").GetText();
			var footerText = App.FindElement("footerLabel").GetText();

			Assert.IsNotEmpty(headerText);
			Assert.IsNotEmpty(footerText);
		}
	}
}
