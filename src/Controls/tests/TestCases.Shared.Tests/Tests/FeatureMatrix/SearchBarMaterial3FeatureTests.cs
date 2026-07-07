#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

// Material3 SearchBar uses TextInputLayout + TextInputEditText instead of SearchView.
// These tests run only on Android where Material3 SearchBarHandler2 is used.
public class SearchBarMaterial3FeatureTests : _GalleryUITest
{
    public override string GalleryPageName => "Search Bar Material3 Feature Matrix";

    public SearchBarMaterial3FeatureTests(TestDevice testDevice) : base(testDevice)
    {
    }

    [Test, Order(1)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_InitialState_VerifyVisualState()
    {
        App.WaitForElement("SearchBar");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(2)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetCancelButtonAndTextColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("CancelButtonOrangeColor");
        App.Tap("CancelButtonOrangeColor");
        App.WaitForElement("TextColorRedButton");
        App.Tap("TextColorRedButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(3)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetFlowDirection_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FlowDirectionRTL");
        App.Tap("FlowDirectionRTL");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(4)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetFontAttributesAndFontFamily_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesBoldButton");
        App.Tap("FontAttributesBoldButton");
        App.WaitForElement("FontFamilyDokdoButton");
        App.Tap("FontFamilyDokdoButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(5)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetFontAttributesAndFontSize_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesItalicButton");
        App.Tap("FontAttributesItalicButton");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "20");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(6)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetFontAttributesAndPlaceholderText_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesItalicButton");
        App.Tap("FontAttributesItalicButton");
        App.WaitForElement("PlaceholderEntry");
        App.EnterText("PlaceholderEntry", "Placeholder Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(7)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetFontAttributesAndText_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesItalicButton");
        App.Tap("FontAttributesItalicButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(8)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetFontAttributesAndTextTransform_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesBoldButton");
        App.Tap("FontAttributesBoldButton");
        App.WaitForElement("TextTransformUppercaseButton");
        App.Tap("TextTransformUppercaseButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(9)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetFontFamilyAndFontSize_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontFamilyDokdoButton");
        App.Tap("FontFamilyDokdoButton");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "20");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(10)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetFontFamilyAndPlaceholder_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontFamilyDokdoButton");
        App.Tap("FontFamilyDokdoButton");
        App.WaitForElement("PlaceholderEntry");
        App.EnterText("PlaceholderEntry", "Placeholder Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(11)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetFontFamilyAndText_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontFamilyDokdoButton");
        App.Tap("FontFamilyDokdoButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(12)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetFontSizeAndPlaceholder_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "20");
        App.WaitForElement("PlaceholderEntry");
        App.EnterText("PlaceholderEntry", "Placeholder Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(13)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetFontSizeAndText_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "20");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(14)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetHorizontalTextAlignmentAndPlaceholder_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("HorizontalTextAlignmentCenterButton");
        App.Tap("HorizontalTextAlignmentCenterButton");
        App.WaitForElement("PlaceholderEntry");
        App.EnterText("PlaceholderEntry", "Placeholder Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(15)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetHorizontalTextAlignmentAndText_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("HorizontalTextAlignmentEndButton");
        App.Tap("HorizontalTextAlignmentEndButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(16)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetHorizontalAndVerticalTextAlignment_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("HorizontalTextAlignmentCenterButton");
        App.Tap("HorizontalTextAlignmentCenterButton");
        App.WaitForElement("VerticalTextAlignmentEndButton");
        App.Tap("VerticalTextAlignmentEndButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

#if TEST_FAILS_ON_ANDROID // Issue Link - https://github.com/dotnet/maui/issues/14566
    [Test, Order(17)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetIsEnabledFalse_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("IsEnabledFalseButton");
        App.Tap("IsEnabledFalseButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }
#endif

#if TEST_FAILS_ON_ANDROID // Issue Link - https://github.com/dotnet/maui/issues/29547
    [Tst, Order(18)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetIsReadOnlyAndText_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("IsReadOnlyTrueButton");
        App.Tap("IsReadOnlyTrueButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

#endif

#if TEST_FAILS_ON_ANDROID // Issue Link - https://github.com/dotnet/maui/issues/29833
    [Test, Order(19)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetIsSpellCheckEnabledAndText_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("IsSpellCheckEnabledFalseButton");
        App.Tap("IsSpellCheckEnabledFalseButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Ths is a spleling eror");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }
#endif

    [Test, Order(20)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetIsTextPredictionEnabledAndText_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("IsTextPredictionEnabledFalseButton");
        App.Tap("IsTextPredictionEnabledFalseButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "t");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

#if TEST_FAILS_ON_ANDROID // Issue Link - https://github.com/dotnet/maui/issues/26968
    [Test, Order(21)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetKeyboardAndText_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("KeyboardNumericButton");
        App.Tap("KeyboardNumericButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.Tap("SearchBar");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }
#endif

    [Test, Order(22)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetPlaceholderAndCharacterSpacing_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("PlaceholderEntry");
        App.EnterText("PlaceholderEntry", "Placeholder Text");
        App.WaitForElement("CharacterSpacingEntry");
        App.ClearText("CharacterSpacingEntry");
        App.EnterText("CharacterSpacingEntry", "10");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(23)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetPlaceholderAndPlaceholderColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("PlaceholderEntry");
        App.EnterText("PlaceholderEntry", "Placeholder Text");
        App.WaitForElement("PlaceholderColorRedButton");
        App.Tap("PlaceholderColorRedButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(24)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetPlaceholderAndVerticalTextAlignment_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("PlaceholderEntry");
        App.EnterText("PlaceholderEntry", "Placeholder Text");
        App.WaitForElement("VerticalTextAlignmentEndButton");
        App.Tap("VerticalTextAlignmentEndButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(25)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetTextAndVerticalTextAlignment_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("VerticalTextAlignmentStartButton");
        App.Tap("VerticalTextAlignmentStartButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(26)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetPlaceholderColorAndTextColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("TextColorGreenButton");
        App.Tap("TextColorGreenButton");
        App.WaitForElement("PlaceholderEntry");
        App.EnterText("PlaceholderEntry", "Placeholder Text");
        App.WaitForElement("PlaceholderColorRedButton");
        App.Tap("PlaceholderColorRedButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(27)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetShadow_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("ShadowTrueButton");
        App.Tap("ShadowTrueButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Shadow Test");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(28)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetTextAndCharacterSpacing_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("CharacterSpacingEntry");
        App.ClearText("CharacterSpacingEntry");
        App.EnterText("CharacterSpacingEntry", "10");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "SearchText");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(29)]
    [Category(UITestCategories.Material3)]
    public void SearchBar_Material3_SetFontAutoScalingFalse_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAutoScalingFalseRadioButton");
        App.Tap("FontAutoScalingFalseRadioButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SearchBar");
        App.ClearText("SearchBar");
        App.EnterText("SearchBar", "Search Text");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }
}
#endif
