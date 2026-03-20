#if IOS || Android
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23325 : _IssuesUITest
	{
		public Issue23325(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Setting background color on the Searchbar does nothing";

		[Test]
		[Category(UITestCategories.SearchBar)]
		public void Issue23325Test()
		{
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/33972
			}
			App.WaitForElement("label");

            // The test passes if search handler is red
			VerifyScreenshot();
		}
	}
}
#endif