#if IOS || Android
using Xunit;
using Xunit;
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

		[Fact]
		[Category(UITestCategories.SearchBar)]
		public void Issue23325Test()
		{
			App.WaitForElement("label");

            // The test passes if search handler is red
			VerifyScreenshot();
		}
	}
}
#endif