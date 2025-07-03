using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30366 : _IssuesUITest
{
    public Issue30366(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "SearchBar CharacterSpacing property is not working as expected";

    [Test]
    [Category(UITestCategories.SearchBar)]
    public void CharacterSpacingShouldApplyForSearchBarPlaceHolderText()
    {
        App.WaitForElement("Issue30366_SearchBar");
        VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.SearchBar)]
    public void CharacterSpacingShouldApplyForSearchBarText()
    {
        App.WaitForElement("Issue30366_SearchBar");
        App.EnterText("Issue30366_SearchBar", "Test Search");
#if IOS
			VerifyScreenshot(cropBottom: 1200);
#else
        VerifyScreenshot();
#endif
    }
}