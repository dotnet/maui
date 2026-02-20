#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID // For Windows: Test excluded on Windows due to unrelated NullReferenceException in test infrastructure - see https://github.com/dotnet/maui/issues/28824. 
// The underlying grouped collection bug (issue #28827) is not Windows-specific; 
// re-enable this test on Windows once issue #28824 is resolved.
// For Android: We already have a fix in separate PR #28886, we can re-enable this test once that PR is merged and available in main branch.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29141 : _IssuesUITest
{
	public override string Issue => "[iOS] Group Header/Footer Repeated for All Items When IsGrouped is True for ObservableCollection in CollectionView";
	public Issue29141(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyCVGroupHFTemplateWithObservableCollection()
	{
		App.WaitForElement("collectionView");
		App.Tap("IsGroupedTrue");
		App.Tap("GroupHeaderTemplateGrid");
		App.Tap("GroupFooterTemplateGrid");
		App.WaitForNoElement("GroupHeaderTemplate");
		App.WaitForNoElement("GroupFooterTemplate");
	}
}
#endif