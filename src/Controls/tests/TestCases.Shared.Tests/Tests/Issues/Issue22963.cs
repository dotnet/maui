using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    internal class Issue22963 : _IssuesUITest
    {
        public Issue22963(TestDevice device) : base(device) { }

        public override string Issue => "Implementation of Customizable Search Button Color for SearchBar Across Platforms";

        [Test]
        [Category(UITestCategories.SearchBar)]
        public void SearchButtonColorInitializesCorrectly()
        {
            App.WaitForElement("SearchBar");
            VerifyScreenshot();
        }
    }
}
