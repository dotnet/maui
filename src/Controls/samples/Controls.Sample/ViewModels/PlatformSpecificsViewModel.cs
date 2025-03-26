using System.Collections.Generic;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class PlatformSpecificsViewModel : BaseGalleryViewModel
	{
		protected override IEnumerable<SectionModel> CreateItems()
		{
#if ANDROID
			return new[]
			{
				new SectionModel(typeof(AndroidButtonPage), "Button Default Padding and Shadow",
				"This Android platform-specific controls whether Xamarin.Forms buttons use the default padding and shadow values of Android buttons."),

				new SectionModel(typeof(AndroidEntryPage), "Entry ImeOptions",
					"This Android platform-specific sets the input method editor (IME) options for the soft keyboard for an Entry."),

				new SectionModel(typeof(AndroidListViewFastScrollPage), "ListView FastScroll",
					"This Android platform-specific is used to enable fast scrolling through data in a ListView."),

				new SectionModel(typeof(AndroidNavigationPage), "NavigationPage BarHeight",
					"This Android platform-specific sets the height of the navigation bar on a NavigationPage."),

				new SectionModel(typeof(AndroidSoftInputModeAdjustPage), "Soft Input Mode Adjust",
					"This Android platform-specific sets the height of the navigation bar on a NavigationPage."),

				new SectionModel(typeof(AndroidSwipeViewTransitionModePage), "SwipeView SwipeTransitionMode",
					"This Android platform-specific controls the transition that's used when opening a SwipeView."),

				new SectionModel(typeof(AndroidTabbedPageSwipePage), "TabbedPage platform-specifics",
					"This Android platform-specific is used to enable swiping with a horizontal finger gesture between pages in a TabbedPage."),

				new SectionModel(typeof(AndroidNavigationPage), "NavigationPage BarHeight with TitleView",
					"This Android platform-specific sets the height of the navigation bar on a NavigationPage."),

				new SectionModel(typeof(AndroidWebViewPage), "WebView Mixed Content with Zoom",
					"This Android platform-specific controls whether a WebView can display mixed content."),

				new SectionModel(typeof(AndroidWebViewZoomPage), "WebView Zoom Controls",
					"This Android platform-specific enables pinch-to-zoom and a zoom control on a WebView."),
			};
#elif IOS || MACCATALYST
			return new[]
			{
				new SectionModel(typeof(iOSBlurEffectPage), "Blur Effect",
					"This iOS platform-specific is used to blur the content layered beneath it, and can be applied to any VisualElement."),

				new SectionModel(typeof(iOSDatePickerPage), "DatePicker UpdateMode",
					"This iOS platform-specific controls when item selection occurs in a DatePicker, allowing the user to specify that item selection occurs when browsing items in the control, or only once the Done button is pressed."),

				new SectionModel(typeof(iOSEntryPage), "AdjustsFontSizeToFitWidth",
					"This iOS platform-specific is used to scale the font size of an Entry to ensure that the inputted text fits in the control."),

				new SectionModel(typeof(iOSFirstResponderPage), "VisualElement first responder",
					"This iOS platform-specific enables a VisualElement object to become the first responder to touch events, rather than the page containing the element."),

				new SectionModel(typeof(iOSFlyoutPage), "FlyoutPage Shadow",
					"This platform-specific controls whether the detail page of a FlyoutPage has shadow applied to it, when revealing the flyout page."),

				new SectionModel(typeof(iOSDragAndDropRequestFullSize), "Drag and Drop Gesture Recognizer Platform-Specific",
					"This iOS platform-specific controls whether to request full-sized drag shadows and the UIDropProposal types."),

				new SectionModel(typeof(iOSHideHomeIndicatorPage), "Hide Home Indicator",
					"This iOS platform-specific sets the visibility of the home indicator on a Page."),

				new SectionModel(typeof(iOSLargeTitlePage), "Large Title",
					"This iOS platform-specific is used to display the page title as a large title on the navigation bar of a NavigationPage, for devices that use iOS 11 or greater."),

				new SectionModel(typeof(iOSListViewPage), "ListView Platform-Specifics",
					"This iOS platform-specific controls whether the separator between cells in a ListView uses the full width of the ListView."),

				new SectionModel(typeof(iOSListViewWithCellPage), "ListView/Cell Platform-Specifics",
					"This iOS platform-specific controls whether ListView header cells float during scrolling."),

				new SectionModel(typeof(iOSModalPagePresentationStyle), "Modal Presentation Style Page",
					"This iOS platform-specific is used to set the presentation style of a modal page, and in addition can be used to display modal pages that have transparent backgrounds."),

				new SectionModel(typeof(iOSNavigationPage), "NavigationPage Platform-Specifics",
					"This iOS platform-specific hides the separator line and shadow that is at the bottom of the navigation bar on a NavigationPage"),

				new SectionModel(typeof(iOSPanGestureRecognizerPage), "Pan Gesture Recognizer Platform-Specific",
					"This iOS platform-specific enables a PanGestureRecognizer in a scrolling view to capture and share the pan gesture with the scrolling view."),

				new SectionModel(typeof(iOSPickerPage), "Picker UpdateMode",
					"This iOS platform-specific controls when item selection occurs in a Picker, allowing the user to specify that item selection occurs when browsing items in the control, or only once the Done button is pressed."),

				new SectionModel(typeof(iOSSafeAreaPage), "Safe Area",
					"This iOS platform-specific is used to ensure that page content is positioned on an area of the screen that is safe for all devices that use iOS 11 and greater."),

				new SectionModel(typeof(iOSScrollViewPage), "ScrollView Content Touches",
					"This platform-specific controls whether a ScrollView handles a touch gesture or passes it to its content."),

				new SectionModel(typeof(iOSSearchBarPage), "SearchBar Style",
					"This iOS platform-specific controls whether a SearchBar has a background."),

				new SectionModel(typeof(iOSSliderUpdateOnTapPage), "Slider Update on Tap",
					"This iOS platform-specific enables the Slider.Value property to be set by tapping on a position on the Slider bar, rather than by having to drag the Slider thumb."),

				new SectionModel(typeof(iOSStatusBarPage), "Hide Status Bar",
					"This iOS platform-specific is used to set the visibility of the status bar on a Page, and it includes the ability to control how the status bar enters or leaves the Page."),

				new SectionModel(typeof(iOSSwipeViewTransitionModePage), "SwipeView SwipeTransitionMode",
					"This iOS platform-specific controls the transition that's used when opening a SwipeView."),

				new SectionModel(typeof(iOSTimePickerPage), "TimePicker UpdateMode",
					"This iOS platform-specific controls when item selection occurs in a TimePicker, allowing the user to specify that item selection occurs when browsing items in the control, or only once the Done button is pressed."),

				new SectionModel(typeof(iOSTranslucentNavigationBarPage), "NavigationPage Translucent TabBar",
					"This iOS platform-specific is used to set the translucency mode of the tab bar on a NavigationPage."),

				new SectionModel(typeof(iOSTranslucentTabbedPage), "TabbedPage Translucent TabBar",
					"This iOS platform-specific is used to set the translucency mode of the tab bar on a TabbedPage."),

#if MACCATALYST
				new SectionModel(typeof(TitleBarPage), "TitleBar",
					"Add a customizable title bar to your window."),
#endif
			};
#elif WINDOWS
			return new[]
			{
				new SectionModel(typeof(WindowsAddRemoveToolbarItemsPage), "Add / Remove Toolbar Items",
					"This WinUI platform-specific is used to add and/or remove toolbar items."),

				new SectionModel(typeof(WindowsCollapseStyleChangerPage), "Collapse Style",
					"This WinUI platform-specific is used to change the collapse style on a FlyoutPage."),

				new SectionModel(typeof(WindowsCollapseWidthAdjusterPage), "FlyoutPage Navigation Bar",
					"This WinUI platform-specific is used to collapse the navigation bar on a FlyoutPage."),

				new SectionModel(typeof(WindowsDragAndDropCustomization), "Drag and Drop Gesture Recognizer Platform-Specific",
					"This WinUI platform-specific displays drag and drop customization such as custom drag gylph and text."),

				new SectionModel(typeof(WindowsListViewPage), "ListView Selection Mode",
					"This WinUI platform-specific controls whether items in a ListView can respond to tap gestures, and hence whether the native ListView fires the ItemClick or Tapped event."),

				new SectionModel(typeof(WindowsReadingOrderPage), "Text Reading Order",
					"This WinUI platform-specific enables the reading order (left-to-right or right-to-left) of bidirectional text in Entry, Editor, and Label instances to be detected dynamically."),

				new SectionModel(typeof(WindowsTitleBarPage), "TitleBar",
					"This WinUI platform-specific enables TitleBar customization."),

				new SectionModel(typeof(WindowsRefreshViewPage), "RefreshView Pull Direction",
					"This WinUI platform-specific enables the pull direction of a RefreshView to be changed to match the orientation of the scrollable control that's displaying data."),

				new SectionModel(typeof(WindowsSearchBarPage), "SearchBar Spell Check",
					"This WinUI platform-specific enables a SearchBar to interact with the spell check engine."),

				new SectionModel(typeof(WindowsToolbarPlacementChangerPage), "Toolbar Placement Changer",
					"This WinUI platform-specific is used to change the placement of a toolbar on a Page."),

				new SectionModel(typeof(WindowsVisualElementAccessKeysPage), "VisualElement Access Keys",
					"This WinUI platform-specific is used to specify an access key for a VisualElement."),

				new SectionModel(typeof(WindowsWebViewPage), "WebView Platform-Specifics",
					"This WinUI platform-specific enables a WebView to display JavaScript alerts in a WinUI message dialog."),
			};
#else
			return new List<SectionModel>();
#endif
		}
	}
}