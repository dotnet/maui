#if TEST_FAILS_ON_WINDOWS // For Windows: Test excluded on Windows due to unrelated NullReferenceException in test infrastructure - see https://github.com/dotnet/maui/issues/28824. 
// The underlying grouped collection bug (issue #28827) is not Windows-specific; 
// re-enable this test on Windows once issue #28824 is resolved.
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

	[Test, Order(1)]
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

	[Test, Order(2)]
	[Category(UITestCategories.CollectionView)]
	public void VerifyCVGroupHFTemplateWithStringCollection()
	{
		App.WaitForElement("collectionView");
		App.Tap("SwitchToStringCollection");
		App.Tap("IsGroupedTrue");
		App.Tap("GroupHeaderTemplateGrid");
		App.WaitForNoElement("GroupHeaderTemplate");
	}

	[Test, Order(3)]
	[Category(UITestCategories.CollectionView)]
	public void VerifyCVNoSectionCrashOnAddFlatItem()
	{
		App.WaitForElement("collectionView");
		App.Tap("IsGroupedTrue");
		App.Tap("GroupHeaderTemplateGrid");
		App.WaitForNoElement("GroupHeaderTemplate");

		// Add flat items — should not crash or produce new sections
		App.Tap("AddItemButton");
		App.Tap("AddItemButton");
		App.Tap("AddItemButton");
		App.WaitForNoElement("GroupHeaderTemplate");
	}
}
#endif
