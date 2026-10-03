using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Shell)]
public class ShellNavigationFeatureTests : _GalleryUITest
{
	public const string ShellFeatureMatrix = "Shell Feature Matrix";
	public override string GalleryPageName => ShellFeatureMatrix;

	public ShellNavigationFeatureTests(TestDevice device) : base(device) { }

	void NavigateToPage2()
	{
		App.TapShellFlyoutIcon();
		App.WaitForElement("Page2");
		App.Tap("Page2");
	}

	void NavigateToPage3()
	{
		App.TapShellFlyoutIcon();
		App.WaitForElement("Page3");
		App.Tap("Page3");
	}

	// GoToMainButton shares the same AutomationId on all sub-pages — safe because
	// Shell renders only the active page, so only one instance is in the view tree.
	void GoBackToMain()
	{
		App.WaitForElement("GoToMainButton");
		App.Tap("GoToMainButton");
		App.WaitForElement("MainPageIdentityLabel");
	}

	void NavigateToDetail1AndWait()
	{
		App.WaitForElement("NavigateToDetail1Button");
		App.Tap("NavigateToDetail1Button");
		App.WaitForElement("Detail1PageIdentityLabel");
	}

	void NavigateToDetail2AndWait()
	{
		App.WaitForElement("NavigateToDetail2Button");
		App.Tap("NavigateToDetail2Button");
		App.WaitForElement("Detail2PageIdentityLabel");
	}

	void NavigateToQuerySenderAndWait()
	{
		App.WaitForElement("MainPageIdentityLabel");
		App.Tap("OpenPassDataDemoButton");
		App.WaitForElement("QuerySenderPageIdentityLabel");
	}

	bool iOS26OrHigher => App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp);

	void TapShellBackArrow(string parentPageTitle)
	{
#if ANDROID || WINDOWS
		App.TapBackArrow();
#elif IOS || MACCATALYST
		if (iOS26OrHigher)
			App.TapBackArrow();
		else
			App.TapBackArrow(parentPageTitle);
#endif
	}

	// On iOS 26+, the back button accessibility ID changed from the custom text to the static "BackButton".
	// All other platforms continue to work with the custom identifier.
	void TapCustomLabelBackArrow(string customLabel)
	{
		if (iOS26OrHigher)
			App.TapBackArrow();
		else
			App.TapBackArrow(customLabel);
	}

	void TapContent1()
	{
#if WINDOWS
      App.TapTab("Content");
      App.WaitForElement("Content1");
      App.Tap("Content1");
#else
		App.TapTab("Content1");
#endif
	}

	void TapContent2()
	{
#if WINDOWS
      App.TapTab("Content");
      App.WaitForElement("Content2");
      App.Tap("Content2");
#else
		App.TapTab("Content2");
#endif
	}

	// ── Shell Properties ─────────────────────────────────────────────────────

	// Shell.CurrentState, CurrentPage, CurrentItem, and Shell.Current reflect the initial Main tab.
	[Test, Order(1)]
	public void VerifyShellProperties_InitialState()
	{
		App.WaitForElement("ShellNavigationButton");
		App.Tap("ShellNavigationButton");
		App.WaitForElement("MainPageIdentityLabel");
		Assert.That(App.FindElement("CurrentStateLabel").GetText(), Does.Contain("main"));
		Assert.That(App.FindElement("CurrentPageLabel").GetText(), Is.EqualTo("ShellNavigation"));
		Assert.That(App.FindElement("CurrentItemLabel").GetText(), Is.EqualTo("Main"));
		Assert.That(App.FindElement("ShellCurrentLabel").GetText(), Is.EqualTo("ShellNavigationControlPage"));
	}

	// Shell properties update when switching flyout items (Main → Page2).
	[Test, Order(2)]
	public void VerifyShellProperties_FlyoutSwitch()
	{
		NavigateToPage2();
		App.WaitForElement("Page2ContentA1PageLabel");
		Assert.That(App.FindElement("Page2CurrentStateLabel").GetText(), Does.Contain("page2"));
		Assert.That(App.FindElement("Page2CurrentPageLabel").GetText(), Is.EqualTo("Page2TabAContentA1"));
		Assert.That(App.FindElement("Page2CurrentItemLabel").GetText(), Is.EqualTo("Page2"));
		Assert.That(App.FindElement("Page2ShellCurrentLabel").GetText(), Is.EqualTo("ShellNavigationControlPage"));
		GoBackToMain();
	}

	// Shell.CurrentPage updates when switching ShellContent tabs (Content1 → Content2) in Page3.
	[Test, Order(3)]
	public void VerifyShellProperties_ContentSwitch()
	{
		NavigateToPage3();
		App.WaitForElement("Page3C1PageLabel");
		TapContent2();
		App.WaitForElement("Page3C2PageLabel");
		Assert.That(App.FindElement("Page3C2CurrentStateLabel").GetText(), Does.Contain("Content2"));
		Assert.That(App.FindElement("Page3C2CurrentPageLabel").GetText(), Is.EqualTo("Page3Content2"));
		Assert.That(App.FindElement("Page3C2CurrentItemLabel").GetText(), Is.EqualTo("Page3"));
		Assert.That(App.FindElement("Page3C2ShellCurrentLabel").GetText(), Is.EqualTo("ShellNavigationControlPage"));
		TapContent1();
		GoBackToMain();
	}

	// Shell.CurrentPage updates when switching ShellSection tabs (TabA → TabB).
	[Test, Order(4)]
	public void VerifyShellProperties_TabSwitch()
	{
		NavigateToPage2();
		App.WaitForElement("TabB");
		App.Tap("TabB");
		App.WaitForElement("Page2TabBPageLabel");
		Assert.That(App.FindElement("Page2TabBCurrentStateLabel").GetText(), Does.Contain("page2"));
		Assert.That(App.FindElement("Page2TabBCurrentPageLabel").GetText(), Is.EqualTo("Page2TabBContentB1"));
		Assert.That(App.FindElement("Page2TabBCurrentItemLabel").GetText(), Is.EqualTo("Page2"));
		Assert.That(App.FindElement("Page2TabBShellCurrentLabel").GetText(), Is.EqualTo("ShellNavigationControlPage"));
		App.WaitForElement("TabA");
		App.Tap("TabA");
		GoBackToMain();
	}

	// ── Back Button ───────────────────────────────────────────────────────────

	// Tapping the back arrow with no BackButtonBehavior command pops the page.
	[Test, Order(5)]
	public void BackButton_TapBackArrow_NoCommand_NavigatesBack()
	{
		NavigateToDetail1AndWait();
		TapShellBackArrow("ShellNavigation");
		App.WaitForElement("MainPageIdentityLabel");
	}

	// ── Route Navigation ──────────────────────────────────────────────────────

	// Relative route "navtest1" pushes NavTest1 onto the stack from Detail1.
	[Test, Order(6)]
	public void RelativeRoute_Detail1ToNavTest1()
	{
		App.WaitForElement("MainPageIdentityLabel");
		NavigateToDetail1AndWait();
		App.WaitForElement("Detail1RelativeButton");
		App.Tap("Detail1RelativeButton");
		App.WaitForElement("NavTest1PageIdentityLabel");
	}

	// Forward push: NavTest1 → NavTest2 via relative route.
	[Test, Order(7)]
	public void ForwardNavigation_NavTest1ToNavTest2()
	{
		App.WaitForElement("NavTest1ToNavTest2Button");
		App.Tap("NavTest1ToNavTest2Button");
		App.WaitForElement("NavTest2PageIdentityLabel");
	}

	// "../navtest3" pops NavTest2 then pushes NavTest3 in a single GoToAsync call.
	[Test, Order(8)]
	public void BackAndForwardNavigation_NavTest2ToNavTest3()
	{
		App.WaitForElement("NavTest2BackForwardButton");
		App.Tap("NavTest2BackForwardButton");
		App.WaitForElement("NavTest3PageIdentityLabel");
	}

	// "../.." pops two levels at once, landing on Detail1.
	[Test, Order(9)]
	public void DoubleBackNavigation_NavTest3ToDetail1()
	{
		App.WaitForElement("NavTest3MultiBackButton");
		App.Tap("NavTest3MultiBackButton");
		App.WaitForElement("Detail1PageIdentityLabel");
	}

	// Absolute route "//page2" jumps directly across flyout items from Detail1.
	[Test, Order(10)]
	public void AbsoluteRoute_Detail1ToPage2()
	{
		App.WaitForElement("Detail1AbsoluteButton");
		App.Tap("Detail1AbsoluteButton");
		App.WaitForElement("Page2ContentA1PageLabel");
	}

	// ── Contextual Routes ─────────────────────────────────────────────────────

	// "subdetail" resolves relative to the page that pushed it (Detail1 context).
	[Test, Order(11)]
	public void ContextualRoute_FromDetail1_SubDetailShowsDetail1Context()
	{
		App.WaitForElement("GoToMainButton");
		App.Tap("GoToMainButton");
		App.WaitForElement("MainPageIdentityLabel");
		NavigateToDetail1AndWait();
		App.WaitForElement("Detail1ContextualNavButton");
		App.Tap("Detail1ContextualNavButton");
		App.WaitForElement("SubDetailPageIdentityLabel");
		Assert.That(App.FindElement("SubDetailCurrentRouteLabel").GetText(), Does.Contain("detail1"));
		Assert.That(App.FindElement("SubDetailSourceContextLabel").GetText(), Is.EqualTo("Contextual from: detail1"));
		App.WaitForElement("SubDetailGoBackButton");
		App.Tap("SubDetailGoBackButton");
		App.WaitForElement("Detail1GoBackButton");
		App.Tap("Detail1GoBackButton");
		App.WaitForElement("MainPageIdentityLabel");
		NavigateToDetail2AndWait();
	}

	// "subdetail" resolves relative to the page that pushed it (Detail2 context).
	[Test, Order(12)]
	public void ContextualRoute_FromDetail2_SubDetailShowsDetail2Context()
	{
		App.WaitForElement("Detail2ContextualNavButton");
		App.Tap("Detail2ContextualNavButton");
		App.WaitForElement("SubDetailPageIdentityLabel");
		Assert.That(App.FindElement("SubDetailCurrentRouteLabel").GetText(), Does.Contain("detail2"));
		Assert.That(App.FindElement("SubDetailSourceContextLabel").GetText(), Is.EqualTo("Contextual from: detail2"));
		App.WaitForElement("SubDetailGoBackButton");
		App.Tap("SubDetailGoBackButton");
		App.WaitForElement("Detail2GoBackButton");
		App.Tap("Detail2GoBackButton");
		App.WaitForElement("MainPageIdentityLabel");
	}

	// ── Route Registration ────────────────────────────────────────────────────

	// Unregistering a route prevents navigation to it.
	[Test, Order(13)]
	public void RouteRegistration_UnregisterRoute_NavigationFails()
	{
		App.WaitForElement("ToggleRouteButton");
		App.Tap("ToggleRouteButton");
		App.WaitForElement("RouteStatusLabel");
		Assert.That(App.FindElement("RouteStatusLabel").GetText(), Is.EqualTo("Unregistered"));
	}

	// Re-registering a route restores navigation to it.
	[Test, Order(14)]
	public void RouteRegistration_ReRegisterRoute_NavigationSucceeds()
	{
		App.WaitForElement("ToggleRouteButton");
		App.Tap("ToggleRouteButton");
		App.WaitForElement("Detail2PageIdentityLabel");
		App.WaitForElement("Detail2GoBackButton");
		App.Tap("Detail2GoBackButton");
		App.WaitForElement("RouteStatusLabel");
		Assert.That(App.FindElement("RouteStatusLabel").GetText(), Is.EqualTo("Registered"));
		Assert.That(App.FindElement("ToggleRouteButton").GetText(), Is.EqualTo("Unregister Route"));
	}

	// ── Cancel Navigation ─────────────────────────────────────────────────────

	// ShellNavigatingEventArgs.Cancel() blocks the navigation; disabling it resumes normal flow.
	[Test, Order(15)]
	public void CancelNavigation_NavigationIsBlocked_StaysOnMain()
	{
		App.WaitForElement("MainPageIdentityLabel");
		App.WaitForElement("CancelNavigationButton");
		App.Tap("CancelNavigationButton");
		Assert.That(App.FindElement("CancelNavigationButton").GetText(), Is.EqualTo("CancelNav: True"));
		App.WaitForElement("NavigateToDetail1Button");
		App.Tap("NavigateToDetail1Button");
		App.WaitForElement("MainPageIdentityLabel");
		Assert.That(App.FindElement("NavigatingCancelledLabel").GetText(), Is.EqualTo("True"));
		App.Tap("CancelNavigationButton");
		Assert.That(App.FindElement("CancelNavigationButton").GetText(), Is.EqualTo("CancelNav: False"));
		App.WaitForElement("NavigateToDetail1Button");
		App.Tap("NavigateToDetail1Button");
		App.WaitForElement("Detail1PageIdentityLabel");
		App.WaitForElement("Detail1GoBackButton");
		App.Tap("Detail1GoBackButton");
		App.WaitForElement("MainPageIdentityLabel");
	}

	// ── Deferral Navigation ───────────────────────────────────────────────────

	// Deferral delays navigation completion; the page still loads once the deferral is resolved.
	[Test, Order(16)]
	public void DeferralNavigation_NavigationCompletes_AfterDelay()
	{
		App.WaitForElement("MainPageIdentityLabel");
		App.WaitForElement("EnableDeferralButton");
		App.Tap("EnableDeferralButton");
		Assert.That(App.FindElement("EnableDeferralButton").GetText(), Is.EqualTo("Deferral: True"));
		App.WaitForElement("NavigateToDetail1Button");
		App.Tap("NavigateToDetail1Button");
		App.WaitForElement("Detail1PageIdentityLabel", timeout: TimeSpan.FromSeconds(10));
		App.WaitForElement("Detail1GoBackButton");
		App.Tap("Detail1GoBackButton");
		App.WaitForElement("MainPageIdentityLabel");
		Assert.That(App.FindElement("DeferralStatusLabel").GetText(), Is.EqualTo("Deferral completed"));
		App.Tap("EnableDeferralButton");
		Assert.That(App.FindElement("EnableDeferralButton").GetText(), Is.EqualTo("Deferral: False"));
	}

	// ── Pass Data ─────────────────────────────────────────────────────────────
	// String query param (?name=...) is received by IQueryAttributable on the detail page.
	[Test, Order(40)]
	public void PassData_StringQueryParam_ReceivedByIQueryAttributable()
	{
		App.WaitForElement("MainPageIdentityLabel");
		App.Tap("Reset");
		NavigateToQuerySenderAndWait();

		App.ClearText("QuerySendNameEntry");
		App.EnterText("QuerySendNameEntry", "Alice");
		App.Tap("QuerySendStringButton");
		App.WaitForElement("QueryDataDetailPageIdentityLabel");

		Assert.That(App.FindElement("QueryPropertyReceivedLabel").GetText(), Is.EqualTo("Alice"));
	}

	// Dictionary-based params are received by IQueryAttributable on the detail page.
	[Test, Order(41)]
	public void PassData_DictionaryParam_ReceivedByIQueryAttributable()
	{
		App.WaitForElement("QueryDataDetailPageIdentityLabel");
		App.Tap("QueryDetailGoBackButton");
		App.WaitForElement("QuerySenderPageIdentityLabel");

		App.ClearText("QuerySendNameEntry");
		App.EnterText("QuerySendNameEntry", "Bob");
		App.Tap("QuerySendDictButton");
		App.WaitForElement("QueryDataDetailPageIdentityLabel");

		Assert.That(App.FindElement("IQueryAttributableReceivedLabel").GetText(), Is.EqualTo("Bob"));
	}

	// ShellNavigationQueryParameters (single-use) are received by IQueryAttributable.
	[Test, Order(42)]
	public void PassData_SingleUseParams_ReceivedByIQueryAttributable()
	{
		App.WaitForElement("QueryDataDetailPageIdentityLabel");
		App.Tap("QueryDetailGoBackButton");
		App.WaitForElement("QuerySenderPageIdentityLabel");

		App.ClearText("QuerySendNameEntry");
		App.EnterText("QuerySendNameEntry", "Carol");
		App.Tap("QuerySendSingleUseButton");
		App.WaitForElement("QueryDataDetailPageIdentityLabel");

		Assert.That(App.FindElement("QueryPropertyReceivedLabel").GetText(), Is.EqualTo("Carol"));
	}

	// Both name and IQA labels receive the same param from string query (both set in ApplyQueryAttributes).
	[Test, Order(43)]
	public void PassData_StringQueryParam_BothLabelsReceiveSameValue()
	{
		App.WaitForElement("QueryDataDetailPageIdentityLabel");
		App.Tap("QueryDetailGoBackButton");
		App.WaitForElement("QuerySenderPageIdentityLabel");

		App.ClearText("QuerySendNameEntry");
		App.EnterText("QuerySendNameEntry", "Dave");
		App.Tap("QuerySendStringButton");
		App.WaitForElement("QueryDataDetailPageIdentityLabel");

		Assert.That(App.FindElement("QueryPropertyReceivedLabel").GetText(), Is.EqualTo("Dave"));
		Assert.That(App.FindElement("IQueryAttributableReceivedLabel").GetText(), Is.EqualTo("Dave"));
	}

	// Backwards navigation with data passes ?backvalue to the previous page via IQueryAttributable.
	[Test, Order(44)]
	public void PassData_BackwardsNavigation_PassesDataToPreviousPage()
	{
		App.WaitForElement("QueryDataDetailPageIdentityLabel");
		App.Tap("QueryDetailGoBackWithDataButton");
		App.WaitForElement("QuerySenderPageIdentityLabel");

		Assert.That(App.FindElement("QueryBackValueLabel").GetText(), Is.EqualTo("ReturnedData"));

		// cleanup
		App.Tap("QuerySenderGoBackButton");
		App.WaitForElement("MainPageIdentityLabel");
	}


	// Multiple string params (?name=&location=) are each received by IQueryAttributable.
	[Test, Order(45)]
	public void PassData_MultipleStringParams_ReceivedByIQueryAttributable()
	{
		// After test 45 cleanup we are already on MainPage
		NavigateToQuerySenderAndWait();

		App.ClearText("QuerySendNameEntry");
		App.EnterText("QuerySendNameEntry", "Alice");
		App.ClearText("QuerySendLocationEntry");
		App.EnterText("QuerySendLocationEntry", "Savannah");
		App.Tap("QuerySendMultiParamButton");
		App.WaitForElement("QueryDataDetailPageIdentityLabel");

		Assert.That(App.FindElement("QueryPropertyReceivedLabel").GetText(), Is.EqualTo("Alice"));
		Assert.That(App.FindElement("QueryPropertyLocationLabel").GetText(), Is.EqualTo("Savannah"));
	}

	// IQueryAttributable receives decoded values for all data passing methods.
	[Test, Order(46)]
	public void PassData_IQueryAttributable_ReceivesDecodedValues()
	{
		App.WaitForElement("QueryDataDetailPageIdentityLabel");
		App.Tap("QueryDetailGoBackButton");
		App.WaitForElement("QuerySenderPageIdentityLabel");

		App.ClearText("QuerySendNameEntry");
		App.EnterText("QuerySendNameEntry", "Hello World");
		App.Tap("QuerySendStringButton");
		App.WaitForElement("QueryDataDetailPageIdentityLabel");

		Assert.That(App.FindElement("QueryPropertyReceivedLabel").GetText(), Is.EqualTo("Hello World"));
		Assert.That(App.FindElement("IQueryAttributableReceivedLabel").GetText(), Is.EqualTo("Hello World"));
	}

	// Dictionary data is retained in memory; navigating forward without data and back re-applies it.
	[Test, Order(47)]
	public void PassData_Dictionary_PersistsWhenNavigatingToIntermediatePageAndBack()
	{
		App.WaitForElement("QueryDataDetailPageIdentityLabel");
		App.Tap("QueryDetailGoBackButton");
		App.WaitForElement("QuerySenderPageIdentityLabel");

		App.ClearText("QuerySendNameEntry");
		App.EnterText("QuerySendNameEntry", "test");
		App.Tap("QuerySendDictButton");
		App.WaitForElement("QueryDataDetailPageIdentityLabel");

		// First arrival: IQA called once (count tracks all ApplyQueryAttributes calls with "name")
		Assert.That(App.FindElement("DictAppliedCountLabel").GetText(), Is.EqualTo("1"));

		// Navigate to intermediate WITHOUT data — Dictionary data persists in memory
		App.Tap("QueryDetailGoToIntermediateButton");
		App.WaitForElement("QueryIntermediatePageIdentityLabel");
		App.Tap("QueryIntermediateGoBackButton");
		App.WaitForElement("QueryDataDetailPageIdentityLabel");

		// IQA fired again on return — Dictionary was re-applied automatically
		Assert.That(App.FindElement("DictAppliedCountLabel").GetText(), Is.EqualTo("2"));
		Assert.That(App.FindElement("IQueryAttributableReceivedLabel").GetText(), Is.EqualTo("test"));

		// cleanup
		App.Tap("QueryDetailGoBackButton");
		App.WaitForElement("QuerySenderPageIdentityLabel");
		App.Tap("QuerySenderGoBackButton");
		App.WaitForElement("MainPageIdentityLabel");
	}

	// ── Multi-Page Navigation: Intermediate Page Parameters ─────────────────

	// Multi-page GoToAsync with prefixed params: intermediate page receives its own params,
	// last page receives unprefixed params. Overlapping "name" key is scoped correctly.
	[Test, Order(48)]
	public void PassData_MultiPageNavigation_IntermediateAndLastPageReceiveCorrectParams()
	{
		App.WaitForElement("MainPageIdentityLabel");
		NavigateToQuerySenderAndWait();

		App.ClearText("QuerySendNameEntry");
		App.EnterText("QuerySendNameEntry", "DetailName");
		App.ClearText("QuerySendLocationEntry");
		App.EnterText("QuerySendLocationEntry", "DetailLoc");
		App.Tap("QuerySendMultiPagePrefixedButton");

		// Lands on the last page (QueryDataDetail)
		App.WaitForElement("QueryDataDetailPageIdentityLabel");

		// Last page should have received unprefixed params
		Assert.That(App.FindElement("QueryPropertyReceivedLabel").GetText(), Is.EqualTo("DetailName"));
		Assert.That(App.FindElement("QueryPropertyLocationLabel").GetText(), Is.EqualTo("DetailLoc"));

		// Navigate back to the intermediate page and verify it received its prefixed param
		App.Tap("QueryDetailGoBackButton");
		App.WaitForElement("QueryIntermediatePageIdentityLabel");

		// Intermediate page should have received "name=ForIntermediate" (from queryintermediate.name=ForIntermediate)
		Assert.That(App.FindElement("QueryIntermediateReceivedNameLabel").GetText(), Is.EqualTo("ForIntermediate"));
		// Intermediate page should NOT have received "location" (unprefixed, meant for last page only)
		Assert.That(App.FindElement("QueryIntermediateReceivedLocationLabel").GetText(), Is.EqualTo("(none)"));
		// IQA fires once on forward push and again on pop-back (stored params re-applied) —
		// same contract verified for the detail page in PassData_Dictionary_PersistsWhenNavigatingToIntermediatePageAndBack.
		Assert.That(App.FindElement("QueryIntermediateCallCountLabel").GetText(), Is.EqualTo("2"));

		// cleanup
		App.Tap("QueryIntermediateGoBackButton");
		App.WaitForElement("QuerySenderPageIdentityLabel");
		App.Tap("QuerySenderGoBackButton");
		App.WaitForElement("MainPageIdentityLabel");
	}

	// Verifies that overlapping param names are correctly scoped: intermediate gets
	// "name=ForIntermediate" while detail gets "name=DetailValue" — they don't cross-contaminate.
	[Test, Order(49)]
	public void PassData_MultiPageNavigation_OverlappingParamNamesAreScopedCorrectly()
	{
		App.WaitForElement("MainPageIdentityLabel");
		NavigateToQuerySenderAndWait();

		App.ClearText("QuerySendNameEntry");
		App.EnterText("QuerySendNameEntry", "DetailValue");
		App.Tap("QuerySendMultiPagePrefixedButton");

		// Lands on detail page
		App.WaitForElement("QueryDataDetailPageIdentityLabel");

		// Detail page should have "DetailValue", NOT "ForIntermediate"
		Assert.That(App.FindElement("QueryPropertyReceivedLabel").GetText(), Is.EqualTo("DetailValue"));
		Assert.That(App.FindElement("IQueryAttributableReceivedLabel").GetText(), Is.EqualTo("DetailValue"));

		// Go back to intermediate — should have "ForIntermediate", NOT "DetailValue"
		App.Tap("QueryDetailGoBackButton");
		App.WaitForElement("QueryIntermediatePageIdentityLabel");
		Assert.That(App.FindElement("QueryIntermediateReceivedNameLabel").GetText(), Is.EqualTo("ForIntermediate"));

		// cleanup
		App.Tap("QueryIntermediateGoBackButton");
		App.WaitForElement("QuerySenderPageIdentityLabel");
		App.Tap("QuerySenderGoBackButton");
		App.WaitForElement("MainPageIdentityLabel");
	}

	// ── BackButtonBehavior Properties ─────────────────────────────────────────
#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/1625
	// BackButtonBehavior.Text replaces the back button label with a custom string.
	[Test, Order(50)]
	public void BackButtonBehavior_TextOverride_CustomTextShownOnBackButton()
	{
		App.WaitForElement("MainPageIdentityLabel");
		App.WaitForElement("TextOverrideEntry");
		App.ClearText("TextOverrideEntry");
		App.EnterText("TextOverrideEntry", "Go");
		NavigateToDetail1AndWait();
		TapCustomLabelBackArrow("Go");
		App.WaitForElement("MainPageIdentityLabel");
		App.WaitForElement("Reset");
		App.Tap("Reset");
	}
#endif

	// BackButtonBehavior.CommandParameter passes the correct value to the back command.
	[Test, Order(51)]
	public void BackButtonBehavior_CommandParameter_CommandFiresWithCorrectParameter()
	{
		App.WaitForElement("MainPageIdentityLabel");
		App.WaitForElement("CommandParameterEntry");
		App.ClearText("CommandParameterEntry");
		App.EnterText("CommandParameterEntry", "test");
		NavigateToDetail1AndWait();
		TapShellBackArrow("ShellNavigation");
		App.WaitForElement("MainPageIdentityLabel");
		Assert.That(App.FindElement("CommandExecutedLabel").GetText(), Is.EqualTo("Executed: test"));
		App.WaitForElement("Reset");
		App.Tap("Reset");
	}

	// BackButtonBehavior.IsEnabled=false keeps the back button visible but ignores taps.
	[Test, Order(52)]
	public void BackButtonBehavior_IsEnabled_False_BackButtonDoesNotNavigate()
	{
		App.WaitForElement("MainPageIdentityLabel");
		App.WaitForElement("IsEnabledButton");
		App.Tap("IsEnabledButton");
		Assert.That(App.FindElement("IsEnabledButton").GetText(), Is.EqualTo("IsEnabled: False"));
		NavigateToDetail1AndWait();
		TapShellBackArrow("ShellNavigation");
		App.WaitForElement("Detail1PageIdentityLabel");
		App.WaitForElement("Detail1GoBackButton");
		App.Tap("Detail1GoBackButton");
		App.WaitForElement("MainPageIdentityLabel");
		App.Tap("IsEnabledButton");
		Assert.That(App.FindElement("IsEnabledButton").GetText(), Is.EqualTo("IsEnabled: True"));
		NavigateToDetail1AndWait();
		TapShellBackArrow("ShellNavigation");
		App.WaitForElement("MainPageIdentityLabel");
		App.Tap("Reset");
	}

	// BackButtonBehavior.IsVisible=false hides the back button; programmatic navigation still works.
	[Test, Order(53)]
	public void BackButtonBehavior_IsVisible_False_ProgrammaticNavStillWorks()
	{
		App.WaitForElement("MainPageIdentityLabel");
		App.WaitForElement("IsVisibleButton");
		App.Tap("IsVisibleButton");
		NavigateToDetail1AndWait();
		ShellScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/1625
	// BackButtonBehavior.IconOverride replaces the default back arrow with a custom icon.
	[Test, Order(54)]
	public void BackButtonBehavior_IconOverride_CustomIconShownOnBackButton()
	{
		App.WaitForElement("Detail1GoBackButton");
		App.Tap("Detail1GoBackButton");
		App.WaitForElement("MainPageIdentityLabel");
		App.Tap("Reset");
		App.WaitForElement("MainPageIdentityLabel");
		App.Tap("IconOverrideBank");
		NavigateToDetail1AndWait();
		ShellScreenshot();
	}
#endif

	public void ShellScreenshot() // This method is to show titlebar for Screenshot
	{
#if WINDOWS
		VerifyScreenshot(includeTitleBar: true, tolerance: 0.5);
#else
		VerifyScreenshot(tolerance: 0.5);
#endif
	}
}
