using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;
public class CollectionView_GroupingFeatureTests : _GalleryUITest
{
	public const string GroupingFeatureMatrix = "CollectionView Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string GroupHeaderTemplateGrid = "GroupHeaderTemplateGrid";
	public const string GroupFooterTemplateGrid = "GroupFooterTemplateGrid";
	public const string IsGroupedTrue = "IsGroupedTrue";
	public const string ItemsSourceGroupedList = "ItemsSourceGroupedList";
	public const string FooterString = "FooterString";
	public const string HeaderString = "HeaderString";
	public const string ItemTemplateBasic = "ItemTemplateBasic";
	public const string ItemsLayoutHorizontalList = "ItemsLayoutHorizontalList";
	public const string ItemsLayoutHorizontalGrid = "ItemsLayoutHorizontalGrid";
	public const string ItemsLayoutVerticalGrid = "ItemsLayoutVerticalGrid";
	public const string FlowDirectionLTR = "FlowDirectionLeftToRight";
	public const string FlowDirectionRTL = "FlowDirectionRightToLeft";
	public override string GalleryPageName => GroupingFeatureMatrix;
	protected override string GallerySubPageButton => "GroupingButton";

	public CollectionView_GroupingFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyGroupFooterTemplate_WithFooterString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionView Footer(String)");
		App.WaitForNoElement("GroupFooterTemplate");
		App.WaitForNoElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyGroupFooterTemplate_WithHeaderString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionView Header(String)");
		App.WaitForNoElement("GroupFooterTemplate");
		App.WaitForNoElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyGroupFooterTemplate_WithBasicItemTemplate()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(ItemTemplateBasic);
		App.Tap(ItemTemplateBasic);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Apple");
		App.WaitForElement("Banana");
		App.WaitForNoElement("GroupFooterTemplate");
		App.WaitForNoElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyGroupHeaderTemplate_WithFooterString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionView Footer(String)");
		App.WaitForNoElement("GroupHeaderTemplate");
		App.WaitForNoElement("GroupHeaderTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyGroupHeaderTemplate_WithHeaderString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionView Header(String)");
		App.WaitForNoElement("GroupHeaderTemplate");
		App.WaitForNoElement("GroupHeaderTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyGroupHeaderTemplate_WithBasicItemTemplate()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(ItemTemplateBasic);
		App.Tap(ItemTemplateBasic);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Apple");
		App.WaitForElement("Banana");
		App.WaitForNoElement("GroupHeaderTemplate");
		App.WaitForNoElement("GroupHeaderTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyGroupHeaderAndFooterTemplate_WithObservableCollection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForNoElement("GroupHeaderTemplate");
		App.WaitForNoElement("GroupHeaderTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyGroupHeaderAndFooterTemplate_WithItemSourceNull()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement("ItemsSourceNone");
		App.Tap("ItemsSourceNone");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForNoElement("GroupHeaderTemplate");
		App.WaitForNoElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyIsGroupedFalse_WithItemSourceObservableCollection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Apple");
		App.WaitForElement("Banana");
		App.WaitForNoElement("Fruits");
		App.WaitForNoElement("Vegetables");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyIsGroupedFalse_WithHeaderAndFooterString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionView Header(String)");
		App.WaitForElement("CollectionView Footer(String)");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyIsGroupedFalse_WithBasicItemTemplate()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemTemplateBasic);
		App.Tap(ItemTemplateBasic);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Apple");
		App.WaitForElement("Banana");
	}

#if TEST_FAILS_ON_WINDOWS // [Windows] NullReferenceException thrown When Toggling IsGrouped to True in ObservableCollection Binding Issue Link: https://github.com/dotnet/maui/issues/28824
	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyIsGrouped_WithFooterString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionView Footer(String)");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyIsGrouped_WithHeaderString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionView Header(String)");
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS
	//Test fails on CV2 , GroupHeader and GroupFooter template is not visible  Issue Link: https://github.com/dotnet/maui/issues/28509
	//Test fails on CV2 , ItemsLayout does not change Issue Link: https://github.com/dotnet/maui/issues/28656
	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyGroupHeaderAndFooterTemplate_WithVerticalListAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("GroupHeaderTemplate");
		App.WaitForElement("Apple");
		App.WaitForElement("Carrot");
		App.WaitForElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyGroupHeaderAndFooterTemplate_WithGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("GroupHeaderTemplate");
		App.WaitForElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyIsGrouped_WithGroupHeaderAndGroupFooterTemplate()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("GroupHeaderTemplate");
		App.WaitForElement("GroupFooterTemplate");
		App.WaitForElement("GroupHeaderTemplate");
		App.WaitForElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyIsGroupedTrue_WithItemSourceGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Vegetables");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyIsGrouped_WithVerticalListAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Apple");
		App.WaitForElement("Carrot");
		App.WaitForElement("Vegetables");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyIsGroupedTrue_WithBasicTemplateWhenGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemTemplateBasic);
		App.Tap(ItemTemplateBasic);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Apple");
		App.WaitForElement("Banana");
		App.WaitForElement("Fruits");
		App.WaitForElement("Vegetables");
	}

#if TEST_FAILS_ON_WINDOWS
	//ItemsLayout does not change its default value on windows Issue Link: https://github.com/dotnet/maui/issues/27946
	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyGroupHeaderAndFooterTemplate_WithHorizontalListAndObservableCollection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForNoElement("GroupHeaderTemplate");
		App.WaitForElement("Apple");
		App.ScrollRight("CollectionViewControl");
		App.WaitForElement("Carrot");
		App.WaitForNoElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyGroupHeaderAndFooterTemplate_WithHorizontalGridAndObservableCollection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForNoElement("GroupHeaderTemplate");
		App.WaitForElement("Apple");
		App.ScrollRight("CollectionViewControl");
		App.WaitForElement("Carrot");
		App.WaitForNoElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyGroupHeaderAndFooterTemplate_WithVerticalGridAndObservableCollection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForNoElement("GroupHeaderTemplate");
		App.WaitForElement("Apple");
		App.WaitForElement("Carrot");
		App.WaitForNoElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyGroupHeaderAndFooterTemplate_WithVerticalListAndObservableCollection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForNoElement("GroupHeaderTemplate");
		App.WaitForElement("Apple");
		App.WaitForElement("Carrot");
		App.WaitForNoElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyIsGrouped_WithHorizontalListAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Apple");
		App.ScrollRight("CollectionViewControl");
		App.WaitForElement("Carrot");
		App.WaitForElement("Vegetables");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyFlowDirectionRTLIsGrouped_WithHorizontalListAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Apple");
		App.ScrollLeft("CollectionViewControl");
		App.WaitForElement("Carrot");
		App.WaitForElement("Vegetables");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyIsGrouped_WithHorizontalGridAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Apple");
		App.ScrollRight("CollectionViewControl");
		App.WaitForElement("Carrot");
		App.WaitForElement("Vegetables");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyFlowDirectionRTLIsGrouped_WithHorizontalGridAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		VerifyScreenshot();
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyGroupHeaderAndFooterTemplate_WithHorizontalListAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("GroupHeaderTemplate");
		App.WaitForElement("Banana"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Carrot");
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyFlowDirectionRTLGroupHeaderAndFooterTemplate_WithHorizontalListAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("GroupHeaderTemplate");
		App.WaitForElement("Banana"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		App.ScrollLeft("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollLeft("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Carrot");
		App.ScrollLeft("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollLeft("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyGroupHeaderAndFooterTemplate_WithHorizontalGridAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("GroupHeaderTemplate");
		App.WaitForElement("Banana"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Carrot");
		App.WaitForElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyFlowDirectionRTLGroupHeaderAndFooterTemplate_WithHorizontalGridAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("GroupHeaderTemplate");
		App.WaitForElement("Banana"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		App.ScrollLeft("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollLeft("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Carrot");
		App.WaitForElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyGroupHeaderAndFooterTemplate_WithVerticalGridAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("GroupHeaderTemplate");
		App.WaitForElement("Banana"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		App.WaitForElement("Carrot");
		App.WaitForElement("GroupFooterTemplate");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyFlowDirectionRTLGroupHeaderAndFooterTemplate_WithVerticalGridAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GroupHeaderTemplateGrid);
		App.Tap(GroupHeaderTemplateGrid);
		App.WaitForElement(GroupFooterTemplateGrid);
		App.Tap(GroupFooterTemplateGrid);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("GroupHeaderTemplate");
		VerifyScreenshot();
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyIsGrouped_WithVerticalGridAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Banana"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		App.WaitForElement("Carrot");
		App.WaitForElement("Vegetables");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyFlowDirectionRTLIsGrouped_WithVerticalGridAndGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		VerifyScreenshot();
	}
#endif
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
//CollectionView Displays Blank UI When Changing IsGrouped and ItemsSourceType Issue Link: https://github.com/dotnet/maui/issues/28826
//[Windows] NullReferenceException thrown When Toggling IsGrouped to True in ObservableCollection Binding: https://github.com/dotnet/maui/issues/28824
        [Test]
        [ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
        public void VerifyIsGroupedTrue_WithItemSourceObservableCollection()
        {
            App.WaitForElement(Options);
            App.Tap(Options);
            App.WaitForElement(IsGroupedTrue);
            App.Tap(IsGroupedTrue);
            App.WaitForElement(Apply);
            App.Tap(Apply);
            App.WaitForElement("Apple");
            App.WaitForElement("Banana");
            App.WaitForNoElement("Fruits");
            App.WaitForNoElement("Vegetables");
        }
 
        [Test]
        [ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
        public void VerifyIsGroupedFalse_WithItemSourceGroupedList()
        {
            App.WaitForElement(Options);
            App.Tap(Options);
            App.WaitForElement(ItemsSourceGroupedList);
            App.Tap(ItemsSourceGroupedList);
            App.WaitForElement(Apply);
            App.Tap(Apply);
            App.WaitForNoElement("Fruits");
            App.WaitForNoElement("Vegetables");
        }
 
        [Test]
        [ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
        public void VerifyIsGroupedTrue_WithBasicTemplateWhenObservableCollection()
        {
            App.WaitForElement(Options);
            App.Tap(Options);
            App.WaitForElement(IsGroupedTrue);
            App.Tap(IsGroupedTrue);
            App.WaitForElement(ItemTemplateBasic);
            App.Tap(ItemTemplateBasic);
            App.WaitForElement(Apply);
            App.Tap(Apply);
            App.WaitForElement("Apple");
            App.WaitForElement("Banana");
            App.WaitForNoElement("Fruits");
            App.WaitForNoElement("Vegetables");
        }
       
        [Test]
        [ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
        public void VerifyIsGrouped_WithHorizontalListAndObservableCollection()
        {
            App.WaitForElement(Options);
            App.Tap(Options);
            App.WaitForElement(ItemsLayoutHorizontalList);
            App.Tap(ItemsLayoutHorizontalList);
            App.WaitForElement(IsGroupedTrue);  
            App.Tap(IsGroupedTrue);
            App.WaitForElement(Apply);
            App.Tap(Apply);
            App.WaitForNoElement("Fruits");
            App.WaitForElement("Banana"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
            App.ScrollRight("CollectionViewControl");
            App.WaitForElement("Carrot");
            App.WaitForNoElement("Vegetables");
        }
 
        [Test]
        [ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
        public void VerifyIsGrouped_WithHorizontalGridAndObservableCollection()
        {
            App.WaitForElement(Options);
            App.Tap(Options);
            App.WaitForElement(ItemsLayoutHorizontalGrid);
            App.Tap(ItemsLayoutHorizontalGrid);
            App.WaitForElement(IsGroupedTrue);  
            App.Tap(IsGroupedTrue);
            App.WaitForElement(Apply);
            App.Tap(Apply);
            App.WaitForNoElement("Fruits");
            App.WaitForElement("Banana"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
            App.ScrollRight("CollectionViewControl");
            App.WaitForElement("Carrot");
            App.WaitForNoElement("Vegetables");
        }
 
        [Test]
        [ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
        public void VerifyIsGrouped_WithVerticalGridAndObservableCollection()
        {
            App.WaitForElement(Options);
            App.Tap(Options);
            App.WaitForElement(IsGroupedTrue);  
            App.Tap(IsGroupedTrue);
            App.WaitForElement(ItemsLayoutVerticalGrid);
            App.Tap(ItemsLayoutVerticalGrid);
            App.WaitForElement(Apply);
            App.Tap(Apply);
            App.WaitForNoElement("Fruits");
            App.WaitForElement("Banana"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
            App.WaitForElement("Carrot");
            App.WaitForNoElement("Vegetables");
        }
 
        [Test]
        [ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
        public void VerifyIsGrouped_WithVerticalListAndObservableCollection()
        {
            App.WaitForElement(Options);
            App.Tap(Options);
            App.WaitForElement(IsGroupedTrue);  
            App.Tap(IsGroupedTrue);
            App.WaitForElement(Apply);
            App.Tap(Apply);
            App.WaitForNoElement("Fruits");
            App.WaitForElement("Banana"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
            App.WaitForElement("Carrot");
            App.WaitForNoElement("Vegetables");
        }
 
// [Android] Group Header/Footer Repeated for All Items When IsGrouped is True for ObservableCollection Issue Link: https://github.com/dotnet/maui/issues/28827
        [Test]
        [ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
        public void VerifyIsGrouped_WithGroupHeaderAndFooterTemplateAndObservableCollection()
        {
            App.WaitForElement(Options);
            App.Tap(Options);
            App.WaitForElement(GroupHeaderTemplateGrid);
            App.Tap(GroupHeaderTemplateGrid);
            App.WaitForElement(GroupFooterTemplateGrid);
            App.Tap(GroupFooterTemplateGrid);
            App.WaitForElement(IsGroupedTrue);  
            App.Tap(IsGroupedTrue);
            App.WaitForElement(Apply);
            App.Tap(Apply);
            App.WaitForNoElement("GroupHeaderTemplate");
            App.WaitForNoElement("GroupFooterTemplate");
            App.WaitForElement("Banana"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
            App.WaitForElement("Carrot");
        }
#endif

#if TEST_FAILS_ON_WINDOWS //.NET MAUI CollectionView does not reorder when grouped on windows Issue Link:  https://github.com/dotnet/maui/issues/13027

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyCanMixGroupsFalseWithCanReorderItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement("CanReorderItemsTrue");
		App.Tap("CanReorderItemsTrue");
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		var initialY = App.WaitForElement("Banana").GetRect().Y;
		App.DragAndDrop("Banana", "Potato"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		var newY = App.WaitForElement("Carrot").GetRect().Y;
		Assert.That(newY, Is.GreaterThan(initialY), "The Y position of 'Carrot' should be greater than Banana after the drag-and-drop operation.");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyFlowDirectionRTLCanMixGroupsFalseWithCanReorderItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement("CanReorderItemsTrue");
		App.Tap("CanReorderItemsTrue");
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		var initialY = App.WaitForElement("Banana").GetRect().Y;
		App.DragAndDrop("Banana", "Potato"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		var newY = App.WaitForElement("Carrot").GetRect().Y;
		Assert.That(newY, Is.GreaterThan(initialY), "The Y position of 'Carrot' should be greater than Banana after the drag-and-drop operation.");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyCanMixGroupsTrueWithCanReorderItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("CanMixGroupsTrue");
		App.Tap("CanMixGroupsTrue");
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement("CanReorderItemsTrue");
		App.Tap("CanReorderItemsTrue");
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		var initialY = App.WaitForElement("Banana").GetRect().Y;
		App.DragAndDrop("Banana", "Potato"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		var newY = App.WaitForElement("Banana").GetRect().Y;
		Assert.That(newY, Is.GreaterThan(initialY), "The Y position of 'Banana' should be greater after the drag-and-drop operation.");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyFlowDirectionRTLCanMixGroupsTrueWithCanReorderItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("CanMixGroupsTrue");
		App.Tap("CanMixGroupsTrue");
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement("CanReorderItemsTrue");
		App.Tap("CanReorderItemsTrue");
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		var initialY = App.WaitForElement("Banana").GetRect().Y;
		App.DragAndDrop("Banana", "Potato"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		var newY = App.WaitForElement("Banana").GetRect().Y;
		Assert.That(newY, Is.GreaterThan(initialY), "The Y position of 'Banana' should be greater after the drag-and-drop operation.");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyCanReorderItemsTrueWithCanMixGroups()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("CanReorderItemsTrue");
		App.Tap("CanReorderItemsTrue");
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement("CanMixGroupsTrue");
		App.Tap("CanMixGroupsTrue");
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		var initialY = App.WaitForElement("Banana").GetRect().Y;
		App.DragAndDrop("Banana", "Potato"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		var newY = App.WaitForElement("Banana").GetRect().Y;
		Assert.That(newY, Is.GreaterThan(initialY), "The Y position of 'Banana' should be greater after the drag-and-drop operation.");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyFlowDirectionRTLCanReorderItemsTrueWithCanMixGroups()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("CanReorderItemsTrue");
		App.Tap("CanReorderItemsTrue");
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement("CanMixGroupsTrue");
		App.Tap("CanMixGroupsTrue");
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		var initialY = App.WaitForElement("Banana").GetRect().Y;
		App.DragAndDrop("Banana", "Potato"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		var newY = App.WaitForElement("Banana").GetRect().Y;
		Assert.That(newY, Is.GreaterThan(initialY), "The Y position of 'Banana' should be greater after the drag-and-drop operation.");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyCanReorderItemsFalseWithCanMixGroups()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement("CanMixGroupsTrue");
		App.Tap("CanMixGroupsTrue");
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		var initialY = App.WaitForElement("Banana").GetRect().Y;
		App.DragAndDrop("Banana", "Potato"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		var newY = App.WaitForElement("Banana").GetRect().Y;
		Assert.That(newY, Is.EqualTo(initialY), "The Y position of 'Banana' should be Same Value after the drag-and-drop operation.");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyFlowDirectionRTLCanReorderItemsFalseWithCanMixGroups()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement("CanMixGroupsTrue");
		App.Tap("CanMixGroupsTrue");
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		var initialY = App.WaitForElement("Banana").GetRect().Y;
		App.DragAndDrop("Banana", "Potato"); // Changed from Apple to Banana because Appium taps, scrolls, or drags the Apple icon instead of the CollectionView Apple item.
		var newY = App.WaitForElement("Banana").GetRect().Y;
		Assert.That(newY, Is.EqualTo(initialY), "The Y position of 'Banana' should be Same Value after the drag-and-drop operation.");
	}
#endif
}