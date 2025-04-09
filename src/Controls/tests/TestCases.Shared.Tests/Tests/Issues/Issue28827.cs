#if TEST_FAILS_ON_WINDOWS // NullReferenceException occurs when switching isGrouped to true
// refer to https://github.com/dotnet/maui/issues/28824
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28827 : _IssuesUITest
{
	public override string Issue => "[Android] Group Header/Footer set for all Items when IsGrouped is True for ObservableCollection";
	public Issue28827(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CVGroupHFTemplateWithObservableCollection()
	{
		App.WaitForElement("collectionView");
		App.Tap("IsGroupedTrue");
		App.Tap("GroupHeaderTemplateGrid");
		App.Tap("GroupFooterTemplateGrid");

		VerifyScreenshot();
	}
}
#endif