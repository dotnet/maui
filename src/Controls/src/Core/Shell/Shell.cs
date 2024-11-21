#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shell']/Docs/*" />
	[ContentProperty(nameof(Items))]
	public partial class Shell : Page, IShellController, IPropertyPropagationController, IPageContainer<Page>, IFlyoutView
	{
		/// <summary>
		/// The currently presented page.
		/// </summary>
		public Page CurrentPage => GetVisiblePage() as Page;

		/// <summary>
		/// Controls the behavior of the page's back button.
		/// </summary>
		public static readonly BindableProperty BackButtonBehaviorProperty =
			BindableProperty.CreateAttached("BackButtonBehavior", typeof(BackButtonBehavior), typeof(Shell), null, BindingMode.OneTime,
				propertyChanged: OnBackButonBehaviorPropertyChanged);

		static void OnBackButonBehaviorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is BackButtonBehavior oldHandlerProperties)
				SetInheritedBindingContext(oldHandlerProperties, null);
			if (newValue is BackButtonBehavior newHandlerProperties)
				SetInheritedBindingContext(newHandlerProperties, bindable.BindingContext);
		}

		/// <summary>
		/// Defines the navigation animation that occurs when a page is navigated to with the <see cref="GoToAsync(ShellNavigationState, bool)"/> method.
		/// Also controls if the content is presented in a modal way or not.
		/// </summary>
		public static readonly BindableProperty PresentationModeProperty = BindableProperty.CreateAttached("PresentationMode", typeof(PresentationMode), typeof(Shell), PresentationMode.Animated);

		/// <summary>
		/// Manages the behavior used to open the flyout.
		/// </summary>
		/// <remarks>
		/// The flyout can be accessed through the hamburger icon or by swiping from the side of the screen. 
		/// This behavior can be changed by setting the <see cref = "FlyoutBehavior" /> property.
		/// </remarks>
		public static readonly BindableProperty FlyoutBehaviorProperty =
			BindableProperty.CreateAttached(nameof(FlyoutBehavior), typeof(FlyoutBehavior), typeof(Shell), FlyoutBehavior.Flyout,
				propertyChanged: OnFlyoutBehaviorChanged);

		/// <summary>
		/// Manages if the navigation bar is visible when a page is presented. 
		/// </summary>
		public static readonly BindableProperty NavBarIsVisibleProperty =
			BindableProperty.CreateAttached("NavBarIsVisible", typeof(bool), typeof(Shell), true, propertyChanged: OnNavBarIsVisibleChanged);

		private static void OnNavBarIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
		{
			// Nav bar visibility change is only interesting from the Shell down to the current Page.
			// Make sure the ShellToolbar knows about any possible change.
			Shell shell = bindable as Shell
				?? (bindable as BaseShellItem)?.FindParentOfType<Shell>()
				?? (bindable as Page)?.FindParentOfType<Shell>();

			shell?.OnPropertyChanged(NavBarIsVisibleProperty.PropertyName);
		}

		/// <summary>
		/// Controls whether the navigation bar has a shadow.
		/// </summary>
		public static readonly BindableProperty NavBarHasShadowProperty =
			BindableProperty.CreateAttached("NavBarHasShadow", typeof(bool), typeof(Shell), default(bool),
				defaultValueCreator: (b) => DeviceInfo.Platform == DevicePlatform.Android);

		/// <summary>
		/// Controls the <see cref = "Shell" /> search functionality.
		/// </summary>
		public static readonly BindableProperty SearchHandlerProperty =
			BindableProperty.CreateAttached("SearchHandler", typeof(SearchHandler), typeof(Shell), null, BindingMode.OneTime,
				propertyChanged: OnSearchHandlerPropertyChanged);

		static void OnSearchHandlerPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is SearchHandler oldHandler)
				SetInheritedBindingContext(oldHandler, null);
			if (newValue is SearchHandler newHandler)
				SetInheritedBindingContext(newHandler, bindable.BindingContext);
		}

		/// <summary>
		/// The <see cref = "FlyoutItem" /> visibility.
		/// Flyout items are visible in the flyout by default.
		/// </summary>
		public static readonly BindableProperty FlyoutItemIsVisibleProperty =
			BindableProperty.CreateAttached("FlyoutItemIsVisible", typeof(bool), typeof(Shell), true, propertyChanged: OnFlyoutItemIsVisibleChanged);
		public static bool GetFlyoutItemIsVisible(BindableObject obj) => (bool)obj.GetValue(FlyoutItemIsVisibleProperty);

		/// <summary>
		/// Sets a value that determines if an object has a visible <see cref = "FlyoutItem" /> in the flyout menu.
		/// Flyout items are visible in the flyout by default. However, an item can be hidden in the flyout with the <see cref = "FlyoutItemIsVisibleProperty" />.
		/// </summary>
		/// <param name="obj">The object that sets the visibility of flyout items.</param>
		/// <param name="isVisible"><see langword="true"/> to set the flyout item as visible; otherwise, <see langword="false"/>.</param>
		public static void SetFlyoutItemIsVisible(BindableObject obj, bool isVisible) => obj.SetValue(FlyoutItemIsVisibleProperty, isVisible);

		static void OnFlyoutItemIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is Element element)
				element
					.FindParentOfType<Shell>()
					?.SendFlyoutItemsChanged();

			if (bindable is BaseShellItem baseShellItem && baseShellItem.FlyoutItemIsVisible != (bool)newValue)
				baseShellItem.FlyoutItemIsVisible = (bool)newValue;
		}

		/// <summary>
		/// Manages the bottom tab bar visibility.
		/// </summary>
		/// <remarks>
		/// The tab bar and tabs are visible in <see cref = "Shell" /> applications by default. 
		/// </remarks>
		public static readonly BindableProperty TabBarIsVisibleProperty =
			BindableProperty.CreateAttached("TabBarIsVisible", typeof(bool), typeof(Shell), true);

		/// <summary>
		/// Enables any <see cref = "View" /> to be displayed in the navigation bar.
		/// </summary>
		public static readonly BindableProperty TitleViewProperty =
			BindableProperty.CreateAttached("TitleView", typeof(View), typeof(Shell), null, propertyChanged: OnTitleViewChanged);

		/// <summary>
		/// Customizes the appearance of each <see cref = "MenuItem" />.
		/// </summary>
		public static readonly BindableProperty MenuItemTemplateProperty =
			BindableProperty.CreateAttached(nameof(MenuItemTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		/// <summary>
		/// Gets the <see cref = "DataTemplate" /> applied to <see cref = "MenuItem" /> objects in the MenuItems collection.
		/// </summary>
		/// <param name="obj">The object to get the <see cref="DataTemplate"/> from.</param>
		/// <returns>The <see cref = "DataTemplate" /> applied to <paramref name="obj"/>.</returns>
		public static DataTemplate GetMenuItemTemplate(BindableObject obj) => (DataTemplate)obj.GetValue(MenuItemTemplateProperty);

		/// <summary>
		/// Sets the <see cref = "DataTemplate" /> applied to <see cref = "MenuItem" /> objects in the MenuItems collection.
		/// Shell provides the Text and IconImageSource properties to the BindingContext of the <see cref = "MenuItemTemplate" />. 
		/// </summary>
		/// <remarks>
		/// Title can be used instead of Text, and Icon instead of IconImageSource. This allows reuse of the same template for menu items and flyout items.
		/// </remarks>
		/// <param name="obj">The object that sets the <see cref = "DataTemplate" /> applied to <see cref = "MenuItem" /> objects.</param>
		/// <param name="menuItemTemplate">The <see cref = "DataTemplate" /> applied to <see cref = "MenuItem" /> objects.</param>
		public static void SetMenuItemTemplate(BindableObject obj, DataTemplate menuItemTemplate) => obj.SetValue(MenuItemTemplateProperty, menuItemTemplate);

		/// <summary>
		///  The <see cref = "DataTemplate" /> applied to each <see cref = "FlyoutItem" /> object managed by Shell.
		/// </summary>
		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.CreateAttached(nameof(ItemTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		/// <summary>
		/// Gets the <see cref = "DataTemplate" /> applied to each <see cref = "FlyoutItem" /> object managed by Shell.
		/// </summary>
		/// <param name="obj">The object that sets the <see cref = "DataTemplate" /> applied to Item objects.</param>
		/// <returns>The <see cref = "DataTemplate" /> applied to Item objects.</returns>
		public static DataTemplate GetItemTemplate(BindableObject obj) => (DataTemplate)obj.GetValue(ItemTemplateProperty);

		/// <summary>
		/// Sets the <see cref = "DataTemplate" /> applied to each <see cref = "FlyoutItem" /> object managed by Shell.
		/// </summary>
		/// <param name="obj">The object that sets the <see cref = "DataTemplate" /> applied to Item objects.</param>
		/// <param name="itemTemplate">The <see cref = "DataTemplate" /> applied to Item objects.</param>
		public static void SetItemTemplate(BindableObject obj, DataTemplate itemTemplate) => obj.SetValue(ItemTemplateProperty, itemTemplate);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetBackButtonBehavior']/Docs/*" />
		public static BackButtonBehavior GetBackButtonBehavior(BindableObject obj) => (BackButtonBehavior)obj.GetValue(BackButtonBehaviorProperty);

		/// <summary>
		/// Sets the back button behavior when the given <paramref name="obj"/> is presented.
		/// </summary>
		/// <remarks>
		/// If the <paramref name="obj"/> is not a page, this property won't do anything.
		/// </remarks>
		/// <param name="obj">The page that dictates the Shell's back button behavior when active.</param>
		/// <param name="behavior">The back button behavior.</param>
		public static void SetBackButtonBehavior(BindableObject obj, BackButtonBehavior behavior) => obj.SetValue(BackButtonBehaviorProperty, behavior);

		/// <summary>
		/// Gets the navigation animation that occurs when a page is navigated to with the <see cref = "GoToAsync(ShellNavigationState, bool)" /> method.
		/// </summary>
		/// <param name="obj">The object that modifies the tabs visibility.</param>
		/// <returns>The navigation animation that occurs when a page is navigated to.</returns>
		public static PresentationMode GetPresentationMode(BindableObject obj) => (PresentationMode)obj.GetValue(PresentationModeProperty);

		/// <summary>
		/// Sets the navigation animation that plays when a <see cref="Page"/> is navigated to with the <see cref = "GoToAsync(ShellNavigationState, bool)" /> method.
		/// </summary>
		/// <param name="obj">The object that modifies the tabs visibility.</param>
		/// <param name="presentationMode">Defines the navigation animation that occurs when a page is navigated.</param>
		public static void SetPresentationMode(BindableObject obj, PresentationMode presentationMode) => obj.SetValue(PresentationModeProperty, presentationMode);

		/// <summary>
		/// Gets the behavior used to open the flyout when the given <paramref name="obj"/> is presented.
		/// </summary>
		/// <param name="obj">The object that modifies the Shell behavior used to open the flyout.</param>
		/// <returns>The behavior used to open the flyout.</returns>
		public static FlyoutBehavior GetFlyoutBehavior(BindableObject obj) => (FlyoutBehavior)obj.GetValue(FlyoutBehaviorProperty);

		/// <summary>
		/// Sets the behavior used to open the flyout when the given <paramref name="obj"/> is presented.
		/// </summary>
		/// <remarks>
		/// The flyout can be accessed through the hamburger icon or by swiping from the side of the screen.
		/// However, this behavior can be changed by setting the <see cref = "FlyoutBehavior" /> attached property.
		/// </remarks>
		/// <param name="obj">The object that modifies the Shell behavior used to open the flyout.</param>
		/// <param name="value">The behavior used to open the flyout.</param>
		public static void SetFlyoutBehavior(BindableObject obj, FlyoutBehavior value) => obj.SetValue(FlyoutBehaviorProperty, value);

		/// <summary>
		/// Gets the width of the flyout.
		/// </summary>
		/// <param name="obj">The object that modifies the width of the flyout.</param>
		/// <returns>The width of the flyout.</returns>
		public static double GetFlyoutWidth(BindableObject obj) => (double)obj.GetValue(FlyoutWidthProperty);

		/// <summary>
		/// Sets the width of the flyout when the given <paramref name="obj"/> is active.
		/// This enables scenarios such as expanding the flyout across the entire screen.
		/// </summary>
		/// <param name="obj">The object that modifies the width of the flyout.</param>
		/// <param name="value">Defines the width of the flyout.</param>
		public static void SetFlyoutWidth(BindableObject obj, double value) => obj.SetValue(FlyoutWidthProperty, value);

		/// <summary>
		/// Gets the height of the flyout when the given <paramref name="obj"/> is active.
		/// </summary>
		/// <param name="obj">The object that modifies the height of the flyout.</param>
		/// <returns>The height of the flyout.</returns>
		public static double GetFlyoutHeight(BindableObject obj) => (double)obj.GetValue(FlyoutHeightProperty);

		/// <summary>
		/// Sets the height of the flyout.
		/// </summary>
		/// <remarks>
		/// The height of the flyout can be customized by setting the Shell.FlyoutHeight attached properties to double value.
		/// This enables scenarios such as reducing the height of the flyout so that it doesn't obscure the tab bar.
		/// </remarks>
		/// <param name="obj">The object that modifies the height of the flyout.</param>
		/// <param name="value">Defines the height of the flyout.</param>
		public static void SetFlyoutHeight(BindableObject obj, double value) => obj.SetValue(FlyoutHeightProperty, value);

		/// <summary>
		/// Gets a value indicating if the navigation bar is visible when when the given <paramref name="obj"/> is active. 
		/// </summary>
		/// <param name="obj">The object that that gets the navigation bar visibility.</param>
		/// <returns><see langword="true"/> if the navigation bar is visible; otherwise, <see langword="false"/>.</returns>
		public static bool GetNavBarIsVisible(BindableObject obj) => (bool)obj.GetValue(NavBarIsVisibleProperty);

		/// <summary>
		/// Controls if the navigation bar is visible when the given <paramref name="obj"/> is presented. 
		/// By default the value of the property is <see langword="true"/>.
		/// </summary>
		/// <param name="obj">The object that modifies the navigation bar visibility.</param>
		/// <param name="value"><see langword="true"/> to set the navigation bar as visible; otherwise, <see langword="false"/>.</param>
		public static void SetNavBarIsVisible(BindableObject obj, bool value) => obj.SetValue(NavBarIsVisibleProperty, value);

		/// <summary>
		/// Gets a value that represents if the navigation bar has a shadow when the given <paramref name="obj"/> is active.
		/// </summary>
		/// <param name="obj">The object that modifies if the navigation bar has a shadow.</param>
		/// <returns><see langword="true"/> if the navigation bar has a shadow when <paramref name="obj"/> is presented, otherwise, <see langword="false"/>.</returns>
		public static bool GetNavBarHasShadow(BindableObject obj) => (bool)obj.GetValue(NavBarHasShadowProperty);

		/// <summary>
		/// Controls whether the navigation bar has a shadow when the given <paramref name="obj"/> is active. 
		/// By default the value of the property is <see langword="true"/> on Android, and <see langword="false"/> on other platforms.
		/// </summary>
		/// <param name="obj">The object that modifies if the navigation bar has a shadow.</param>
		/// <param name="value">Manages if the navigation bar has a shadow.</param>
		public static void SetNavBarHasShadow(BindableObject obj, bool value) => obj.SetValue(NavBarHasShadowProperty, value);

		/// <summary>
		/// Gets the integrated search functionality.
		/// </summary>
		/// <param name="obj">The object that modifies the Shell search functionality.</param>
		/// <returns>The integrated search functionality.</returns>
		public static SearchHandler GetSearchHandler(BindableObject obj) => (SearchHandler)obj.GetValue(SearchHandlerProperty);

		/// <summary>
		/// Sets the handler responsible for implementing the integrated search functionality for when the given <paramref name="obj"/> is active.
		/// Enabling this property results in a search box being added at the top of the page.
		/// </summary>
		/// <param name="obj">The object that modifies the Shell search functionality.</param>
		/// <param name="handler">Defines the integrated search functionality.</param>
		public static void SetSearchHandler(BindableObject obj, SearchHandler handler) => obj.SetValue(SearchHandlerProperty, handler);

		/// <summary>
		/// Gets the tabs visibility when the given <paramref name="obj"/> is active.
		/// </summary>
		/// <param name="obj">The object that modifies the tabs visibility.</param>
		/// <returns><see langword="true"/> if the tab bar is visible; otherwise, <see langword="false"/>.</returns>
		public static bool GetTabBarIsVisible(BindableObject obj) => (bool)obj.GetValue(TabBarIsVisibleProperty);

		/// <summary>
		/// Sets the tabs visibility when the given <paramref name="obj"/> is active.
		/// </summary>
		/// <remarks>
		/// The tab bar and tabs are visible in Shell applications by default. However, the tab bar can be hidden by setting the Shell.TabBarIsVisible attached property to false.
		/// While this property can be set on a subclassed Shell object, it's typically set on any ShellContent or ContentPage objects that want to make the tab bar invisible.
		/// </remarks>
		/// <param name="obj">The object that modifies the tabs visibility.</param>
		/// <param name="value"><see langword="true"/> to set the tab bar as visible; otherwise, <see langword="false"/>.</param>
		public static void SetTabBarIsVisible(BindableObject obj, bool value) => obj.SetValue(TabBarIsVisibleProperty, value);

		/// <summary>
		/// Gets any <see cref = "View" /> to be displayed in the navigation bar when the given <paramref name="obj"/> is active.
		/// </summary>
		/// <param name="obj">The object to which the TitleView is set.</param>
		/// <returns>The View to be displayed in the navigation bar.</returns>
		public static View GetTitleView(BindableObject obj) => (View)obj.GetValue(TitleViewProperty);

		/// <summary>
		/// Sets any <see cref = "View" /> to be displayed in the navigation bar when the given <paramref name="obj"/> is active.
		/// </summary>
		/// <param name="obj">The object to which the TitleView is set.</param>
		/// <param name="value">The View to be displayed in the navigation bar.</param>
		public static void SetTitleView(BindableObject obj, View value) => obj.SetValue(TitleViewProperty, value);

		static void OnFlyoutBehaviorChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var element = (Element)bindable;

			while (!Application.IsApplicationOrWindowOrNull(element))
			{
				if (element is Shell shell)
					shell.NotifyFlyoutBehaviorObservers();
				element = element.Parent;
			}
		}

		/// <summary>
		/// Defines the background color in the Shell chrome. 
		/// The color won't fill in behind the Shell content.
		/// </summary>
		public static readonly new BindableProperty BackgroundColorProperty =
			BindableProperty.CreateAttached("BackgroundColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>
		/// Defines the color to shade text and icons that are disabled.
		/// </summary>
		public static readonly BindableProperty DisabledColorProperty =
			BindableProperty.CreateAttached("DisabledColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>
		/// Defines the color to shade text and icons.
		/// </summary>
		public static readonly BindableProperty ForegroundColorProperty =
			BindableProperty.CreateAttached("ForegroundColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>
		/// Defines the background color for the tab bar. If the property is unset, the <see cref = "BackgroundColorProperty" /> value is used.
		/// </summary>
		public static readonly BindableProperty TabBarBackgroundColorProperty =
			BindableProperty.CreateAttached("TabBarBackgroundColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>
		/// Defines the disabled color for the tab bar. If the property is unset, the <see cref = "DisabledColorProperty" /> value is used.
		/// </summary>
		public static readonly BindableProperty TabBarDisabledColorProperty =
			BindableProperty.CreateAttached("TabBarDisabledColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>TabBarForegroundColor</c>.</summary>
		public static readonly BindableProperty TabBarForegroundColorProperty =
			BindableProperty.CreateAttached("TabBarForegroundColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>
		/// Defines the title color for the tab bar. If the property is unset, the <see cref = "TitleColorProperty" /> value will be used.
		/// </summary>
		public static readonly BindableProperty TabBarTitleColorProperty =
			BindableProperty.CreateAttached("TabBarTitleColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>
		/// Defines the unselected color for the tab bar. If the property is unset, the <see cref = "UnselectedColorProperty" /> value is used.
		/// </summary>
		public static readonly BindableProperty TabBarUnselectedColorProperty =
			BindableProperty.CreateAttached("TabBarUnselectedColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>
		/// Defines the title color for the tab bar. If the property is unset, the <see cref = "TitleColorProperty" /> value will be used.
		/// </summary>
		public static readonly BindableProperty TitleColorProperty =
			BindableProperty.CreateAttached("TitleColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>
		/// Defines the unselected color for the tab bar. If the property is unset, the <see cref = "UnselectedColorProperty" /> value is used.
		/// </summary>
		public static readonly BindableProperty UnselectedColorProperty =
			BindableProperty.CreateAttached("UnselectedColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>
		/// The backdrop of the flyout, which is the appearance of the flyout overlay.
		/// </summary>
		public static readonly BindableProperty FlyoutBackdropProperty =
			BindableProperty.CreateAttached(nameof(FlyoutBackdrop), typeof(Brush), typeof(Shell), Brush.Default,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>
		/// The width of the flyout.
		/// This enables scenarios such as expanding the flyout across the entire screen.
		/// </summary>
		public static readonly BindableProperty FlyoutWidthProperty =
			BindableProperty.CreateAttached(nameof(FlyoutWidth), typeof(double), typeof(Shell), -1d,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>
		/// The height of the flyout.
		/// This enables scenarios such as reducing the height of the flyout so that it doesn't obscure the tab bar.
		/// </summary>
		public static readonly BindableProperty FlyoutHeightProperty =
			BindableProperty.CreateAttached(nameof(FlyoutHeight), typeof(double), typeof(Shell), -1d,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>
		/// Gets the background color in the Shell chrome. 
		/// </summary>
		/// <param name="obj">The object to which the background color is set.</param>
		/// <returns>The background color from the Shell chrome.</returns>
		public static Color GetBackgroundColor(BindableObject obj) => (Color)obj.GetValue(BackgroundColorProperty);

		/// <summary>
		/// Sets the background color in the Shell chrome. 
		/// The color won't fill in behind the Shell content.
		/// </summary>
		/// <param name="obj">The object to which the background color is set.</param>
		/// <param name="value">The background color for the Shell chrome.</param>
		public static void SetBackgroundColor(BindableObject obj, Color value) => obj.SetValue(BackgroundColorProperty, value);

		/// <summary>
		/// Gets the color to shade text and icons that are disabled.
		/// </summary>
		/// <param name="obj">The object to which the disabled color is set.</param>
		/// <returns>The disabled color for the tab bar.</returns>
		public static Color GetDisabledColor(BindableObject obj) => (Color)obj.GetValue(DisabledColorProperty);

		/// <summary>
		/// Sets the color to shade text and icons that are disabled.
		/// </summary>
		/// <param name="obj">The object to which the disabled color is set.</param>
		/// <param name="value">The disabled color for the tab bar.</param>
		public static void SetDisabledColor(BindableObject obj, Color value) => obj.SetValue(DisabledColorProperty, value);

		/// <summary>
		/// Gets the foreground color for the tab bar. 
		/// </summary>
		/// <param name="obj">The object to which the foreground color is set.</param>
		/// <returns>The foreground color for the tab bar.</returns>
		public static Color GetForegroundColor(BindableObject obj) => (Color)obj.GetValue(ForegroundColorProperty);

		/// <summary>
		/// Defines the foreground color for the tab bar. 
		/// If the property is unset, the <see cref = "ForegroundColorProperty" /> value is used.
		/// </summary>
		/// <param name="obj">The object to which the foreground color is set.</param>
		/// <param name="value">The foreground color for the tab bar.</param>
		public static void SetForegroundColor(BindableObject obj, Color value) => obj.SetValue(ForegroundColorProperty, value);

		/// <summary>
		/// Gets the background color for the tab bar.
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <returns>The background color for the tab bar.</returns>
		public static Color GetTabBarBackgroundColor(BindableObject obj) => (Color)obj.GetValue(TabBarBackgroundColorProperty);

		/// <summary>
		/// Sets the background color for the tab bar. 
		/// If the property is unset, the BackgroundColor property value is used.
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <param name="value">The background color for the tab bar.</param>
		public static void SetTabBarBackgroundColor(BindableObject obj, Color value) => obj.SetValue(TabBarBackgroundColorProperty, value);

		/// <summary>
		/// Gets the background color for the tab bar. 
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <returns>The background color for the tab bar.</returns>
		public static Color GetTabBarDisabledColor(BindableObject obj) => (Color)obj.GetValue(TabBarDisabledColorProperty);

		/// <summary>
		/// Sets the disabled color for the tab bar. 
		/// If the property is unset, the <see cref = "DisabledColorProperty" /> value is used.
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <param name="value">The color to set for the tab bar is disabled.</param>
		public static void SetTabBarDisabledColor(BindableObject obj, Color value) => obj.SetValue(TabBarDisabledColorProperty, value);

		/// <summary>
		/// Gets the color of the tab bar when its disabled. 
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <returns>The disabled color for the tab bar.</returns>
		public static Color GetTabBarForegroundColor(BindableObject obj) => (Color)obj.GetValue(TabBarForegroundColorProperty);

		/// <summary>
		/// Sets the foreground color for the tab bar. 
		/// If the property is unset, the ForegroundColor property value is used.
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <param name="value">The foreground color for the tab bar.</param>
		public static void SetTabBarForegroundColor(BindableObject obj, Color value) => obj.SetValue(TabBarForegroundColorProperty, value);

		/// <summary>
		/// Gets the foreground color for the tab bar. 
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <returns>The foreground color for the tab bar.</returns>
		public static Color GetTabBarTitleColor(BindableObject obj) => (Color)obj.GetValue(TabBarTitleColorProperty);

		/// <summary>
		/// Sets the title color for the tab bar. 
		/// If the property is unset, the <see cref="TitleColorProperty" /> value will be used.
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <param name="value">The title color for the tab bar.</param>
		public static void SetTabBarTitleColor(BindableObject obj, Color value) => obj.SetValue(TabBarTitleColorProperty, value);

		/// <summary>
		/// Gets the title color for the tab bar. 
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <returns>The title color for the tab bar.</returns>
		public static Color GetTabBarUnselectedColor(BindableObject obj) => (Color)obj.GetValue(TabBarUnselectedColorProperty);

		/// <summary>
		/// Sets the unselected color for the tab bar. 
		/// If the property is unset, the UnselectedColor property value is used.
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <param name="value">The unselected color for the tab bar.</param>
		public static void SetTabBarUnselectedColor(BindableObject obj, Color value) => obj.SetValue(TabBarUnselectedColorProperty, value);

		/// <summary>
		/// Gets the color used for the title of the current page.
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <returns>The unselected color for the tab bar.</returns>
		public static Color GetTitleColor(BindableObject obj) => (Color)obj.GetValue(TitleColorProperty);

		/// <summary>
		/// Sets the color used for the title of the current page.
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <param name="value">The color used for the title of the current page.</param>
		public static void SetTitleColor(BindableObject obj, Color value) => obj.SetValue(TitleColorProperty, value);

		/// <summary>
		/// Gets the color for unselected text and icons in the Shell chrome.
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <returns>The color for unselected text and icons in the Shell chrome.</returns>
		public static Color GetUnselectedColor(BindableObject obj) => (Color)obj.GetValue(UnselectedColorProperty);

		/// <summary>
		/// Sets the color for unselected text and icons in the Shell chrome.
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <param name="value">The color for unselected text and icons in the Shell chrome.</param>
		public static void SetUnselectedColor(BindableObject obj, Color value) => obj.SetValue(UnselectedColorProperty, value);

		/// <summary>
		/// Gets the color for unselected text and icons in the Shell chrome.
		/// </summary>
		/// <param name="obj">The object to which the color is set.</param>
		/// <returns>The color for unselected text and icons in the Shell chrome.</returns>
		public static Brush GetFlyoutBackdrop(BindableObject obj) => (Brush)obj.GetValue(FlyoutBackdropProperty);

		/// <summary>
		/// Sets the backdrop of the flyout, which is the appearance of the flyout overlay.
		/// </summary>
		/// <param name="obj">The object that sets the backdrop brush.</param>
		/// <param name="value">The brushed used in the backdrop of the flyout.</param>
		public static void SetFlyoutBackdrop(BindableObject obj, Brush value) => obj.SetValue(FlyoutBackdropProperty, value);

		static void OnShellAppearanceValueChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var item = (Element)bindable;
			var source = item;

			while (!Application.IsApplicationOrWindowOrNull(item))
			{
				if (item is IShellController shell)
				{
					shell.AppearanceChanged(source, true);
					return;
				}
				item = item.Parent;
			}
		}

		static readonly BindablePropertyKey CurrentStatePropertyKey =
			BindableProperty.CreateReadOnly(nameof(CurrentState), typeof(ShellNavigationState), typeof(Shell), null);

		static readonly BindablePropertyKey ItemsPropertyKey = BindableProperty.CreateReadOnly(nameof(Items), typeof(ShellItemCollection), typeof(Shell), null,
				defaultValueCreator: bo => new ShellItemCollection { Inner = new ElementCollection<ShellItem>(((Shell)bo).InternalChildren) });

		List<(IAppearanceObserver Observer, Element Pivot)> _appearanceObservers = new List<(IAppearanceObserver Observer, Element Pivot)>();
		List<IFlyoutBehaviorObserver> _flyoutBehaviorObservers = new List<IFlyoutBehaviorObserver>();


		internal static BindableObject GetBindableObjectWithFlyoutItemTemplate(BindableObject bo)
		{
			if (bo is IMenuItemController)
			{
				if (bo is MenuItem mi && mi.Parent != null && mi.Parent.IsSet(MenuItemTemplateProperty))
					return mi.Parent;
				else if (bo is MenuShellItem msi && msi.MenuItem != null && msi.MenuItem.IsSet(MenuItemTemplateProperty))
					return msi.MenuItem;
			}

			return bo;
		}

		DataTemplate IShellController.GetFlyoutItemDataTemplate(BindableObject bo)
		{
			BindableProperty bp = bo is IMenuItemController ? MenuItemTemplateProperty : ItemTemplateProperty;
			var bindableObjectWithTemplate = GetBindableObjectWithFlyoutItemTemplate(bo);

			if (bindableObjectWithTemplate.IsSet(bp))
			{
				return (DataTemplate)bindableObjectWithTemplate.GetValue(bp);
			}

			if (IsSet(bp))
			{
				return (DataTemplate)GetValue(bp);
			}

			return BaseShellItem.CreateDefaultFlyoutItemCell(bo);
		}

		event EventHandler IShellController.StructureChanged
		{
			add { _structureChanged += value; }
			remove { _structureChanged -= value; }
		}

		event EventHandler _structureChanged;

		event EventHandler IShellController.FlyoutItemsChanged
		{
			add { _flyoutManager.FlyoutItemsChanged += value; }
			remove { _flyoutManager.FlyoutItemsChanged -= value; }
		}

		View IShellController.FlyoutHeader => FlyoutHeaderView;
		View IShellController.FlyoutFooter => FlyoutFooterView;
		View IShellController.FlyoutContent => FlyoutContentView;

		IShellController ShellController => this;

		void IShellController.AddAppearanceObserver(IAppearanceObserver observer, Element pivot)
		{
			_appearanceObservers.Add((observer, pivot));
			var appearance = GetAppearanceForPivot(pivot);
			UpdateToolbarAppearanceFeatures(pivot, appearance);
			observer.OnAppearanceChanged(appearance);
		}

		void IShellController.AddFlyoutBehaviorObserver(IFlyoutBehaviorObserver observer)
		{
			_flyoutBehaviorObservers.Add(observer);

			// We need to wait until the visible page has been created before we try to calculate
			// the flyout behavior
			if (GetVisiblePage() != null)
				observer.OnFlyoutBehaviorChanged(GetEffectiveFlyoutBehavior());
		}

		void UpdateToolbarAppearanceFeatures(Element pivot, ShellAppearance appearance)
		{
			// Android sets these inside its renderer
			// once we convert Android to be all handlers we can adjust
			if (pivot is ShellContent || pivot is ShellSection || pivot is ContentPage)
			{
				appearance = appearance ?? GetAppearanceForPivot(pivot);
				Toolbar.BarTextColor = appearance?.TitleColor ?? DefaultTitleColor;
				Toolbar.BarBackground = appearance?.BackgroundColor ?? DefaultBackgroundColor;
				Toolbar.IconColor = appearance?.ForegroundColor ?? DefaultForegroundColor;
			}
		}

#if ANDROID
		static Color DefaultBackgroundColor => ResolveThemeColor(Color.FromArgb("#2c3e50"), Color.FromArgb("#1B3147"));
		static readonly Color DefaultForegroundColor = Colors.White;
		static readonly Color DefaultTitleColor = Colors.White;

		static bool IsDarkTheme => (Application.Current?.RequestedTheme == AppTheme.Dark);

		static Color ResolveThemeColor(Color light, Color dark)
		{
			if (IsDarkTheme)
			{
				return dark;
			}

			return light;
		}
#else
		static Color DefaultBackgroundColor => null;
		static readonly Color DefaultForegroundColor = null;
		static readonly Color DefaultTitleColor = null;
#endif


		void IShellController.AppearanceChanged(Element source, bool appearanceSet)
		{
			if (!appearanceSet)
			{
				// This bubbles up whenever there is an kind of structure/page change
				// So its also quite useful for checking the FlyoutBehavior conditions
				NotifyFlyoutBehaviorObservers();
			}

			UpdateToolbarAppearanceFeatures(source, null);

			// here we wish to notify every element whose "pivot line" contains the source
			// To do that we first need to find the leaf node in the line, and then walk up
			// to see if we find the source on the way up.

			// If this is not an appearanceSet event but just a structural change, then we only
			// need walk up from the source to look for the pivot as items below the change
			// can't be affected by it

			foreach (var (Observer, Pivot) in _appearanceObservers)
			{
				var observer = Observer;
				var pivot = Pivot;

				Element target;
				Element leaf;
				if (appearanceSet)
				{
					leaf = WalkToPage(pivot);
					target = source;
				}
				else
				{
					leaf = source;
					target = pivot;
				}

				while (!Application.IsApplicationOrWindowOrNull(leaf))
				{
					if (leaf == target)
					{
						var appearance = GetAppearanceForPivot(pivot);
						UpdateToolbarAppearanceFeatures(pivot, appearance);
						observer.OnAppearanceChanged(appearance);
						break;
					}

					leaf = leaf.Parent;
				}
			}
		}

		ShellNavigationState IShellController.GetNavigationState(ShellItem shellItem, ShellSection shellSection, ShellContent shellContent, bool includeStack)
			=> ShellNavigationManager.GetNavigationState(shellItem, shellSection, shellContent, includeStack ? shellSection.Stack.ToList() : null, includeStack ? shellSection.Navigation.ModalStack.ToList() : null);

		void OnFlyoutItemSelected(Element element, bool platformInitiated) =>
			OnFlyoutItemSelectedAsync(element, platformInitiated).FireAndForget();

		void IShellController.OnFlyoutItemSelected(Element element) =>
			OnFlyoutItemSelected(element, true);

		Task IShellController.OnFlyoutItemSelectedAsync(Element element) =>
			OnFlyoutItemSelectedAsync(element, true);

		Task OnFlyoutItemSelectedAsync(Element element, bool platformInitiated)
		{
			ShellItem shellItem = null;
			ShellSection shellSection = null;
			ShellContent shellContent = null;

			switch (element)
			{
				case MenuShellItem menuShellItem:
					((IMenuItemController)menuShellItem.MenuItem).Activate();
					break;
				case ShellItem i:
					shellItem = i;
					break;
				case ShellSection s:
					shellItem = s.Parent as ShellItem;
					shellSection = s;
					break;
				case ShellContent c:
					shellItem = c.Parent.Parent as ShellItem;
					shellSection = c.Parent as ShellSection;
					shellContent = c;
					break;
				case MenuItem m:
					((IMenuItemController)m).Activate();
					break;
			}

			if (shellItem == null || !shellItem.IsEnabled)
				return Task.CompletedTask;

			shellSection = shellSection ?? shellItem.CurrentItem;
			shellContent = shellContent ?? shellSection?.CurrentItem;

			if (platformInitiated && FlyoutIsPresented && GetEffectiveFlyoutBehavior() != FlyoutBehavior.Locked)
				SetValueFromRenderer(FlyoutIsPresentedProperty, false);

			if (shellSection == null)
				shellItem.PropertyChanged += OnShellItemPropertyChanged;
			else if (shellContent == null)
				shellSection.PropertyChanged += OnShellItemPropertyChanged;
			else
			{
				if (this.CurrentItem == null)
				{
					var state = ShellNavigationManager.GetNavigationState(shellItem, shellSection, shellContent, shellSection.Navigation.NavigationStack, null);
					var requestBuilder = new RouteRequestBuilder(new List<string>()
					{
						shellItem.Route,
						shellSection.Route,
						shellContent.Route
					});

					var node = new ShellUriHandler.NodeLocation();
					node.SetNode(shellContent);
					requestBuilder.AddMatch(node);

					var navRequest =
						new ShellNavigationRequest(
							new RequestDefinition(requestBuilder, this),
							 ShellNavigationRequest.WhatToDoWithTheStack.ReplaceIt,
							 String.Empty,
							 String.Empty);

					var navParameters = new ShellNavigationParameters()
					{
						TargetState = state,
						Animated = false,
						EnableRelativeShellRoutes = false,
						DeferredArgs = null,
						Parameters = null
					};

					return _navigationManager
						.GoToAsync(navParameters, navRequest);

				}
				else
				{
					var navParameters = ShellNavigationManager.GetNavigationParameters(shellItem, shellSection, shellContent, shellSection.Navigation.NavigationStack, null);

					return _navigationManager
						.GoToAsync(navParameters);
				}
			}

			return Task.CompletedTask;
		}

		void OnShellItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == CurrentItemProperty.PropertyName)
			{
				(sender as BindableObject).PropertyChanged -= OnShellItemPropertyChanged;
				if (sender is ShellItem item)
					OnFlyoutItemSelected(item, false);
				else if (sender is ShellSection section)
					OnFlyoutItemSelected(section.Parent, false);
			}
		}

		bool IShellController.ProposeNavigation(ShellNavigationSource source, ShellItem shellItem, ShellSection shellSection, ShellContent shellContent, IReadOnlyList<Page> stack, bool canCancel)
		{
			return _navigationManager.ProposeNavigationOutsideGotoAsync(source, shellItem, shellSection, shellContent, stack, canCancel, true);
		}

		bool IShellController.RemoveAppearanceObserver(IAppearanceObserver observer)
		{
			for (int i = 0; i < _appearanceObservers.Count; i++)
			{
				if (_appearanceObservers[i].Observer == observer)
				{
					_appearanceObservers.RemoveAt(i);
					return true;
				}
			}

			return false;
		}

		bool IShellController.RemoveFlyoutBehaviorObserver(IFlyoutBehaviorObserver observer)
			=> _flyoutBehaviorObservers.Remove(observer);

		void IShellController.UpdateCurrentState(ShellNavigationSource source)
		{
			var oldState = CurrentState;
			var shellItem = CurrentItem;
			var shellSection = shellItem?.CurrentItem;
			var shellContent = shellSection?.CurrentItem;
			var stack = shellSection?.Stack;
			var modalStack = shellSection?.Navigation?.ModalStack;
			var result = ShellNavigationManager.GetNavigationState(shellItem, shellSection, shellContent, stack, modalStack);

			if (result?.Location != oldState?.Location)
			{
				SetValueFromRenderer(CurrentStatePropertyKey, result);
				_navigationManager.HandleNavigated(new ShellNavigatedEventArgs(oldState, CurrentState, source));
			}
		}

		ReadOnlyCollection<ShellItem> IShellController.GetItems() => ((ShellItemCollection)Items).VisibleItemsReadOnly;

		event NotifyCollectionChangedEventHandler IShellController.ItemsCollectionChanged
		{
			add { ((ShellItemCollection)Items).VisibleItemsChanged += value; }
			remove { ((ShellItemCollection)Items).VisibleItemsChanged -= value; }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='Current']/Docs/*" />
		public static Shell Current
		{
			get
			{
				if (Application.Current is null || Application.Current.Windows.Count == 0)
					return null;

				if (Application.Current.Windows.Count == 1)
				{
					return Application.Current.Windows[0].Page as Shell;
				}

				// Check if shell is activated
				Shell currentShell = null;
				Shell returnIfThereIsJustOneShell = null;
				bool tooManyShells = false;
				foreach (var window in Application.Current.Windows)
				{
					if (window.Page is Shell shell)
					{
						if (window.IsActivated)
						{
							if (currentShell is not null)
							{
								currentShell = null;
								break;
							}

							currentShell = shell;
						}

						if (returnIfThereIsJustOneShell is not null)
						{
							tooManyShells = true;
						}
					}
				}

				if (currentShell is not null)
				{
					return currentShell;
				}

				if (!tooManyShells && returnIfThereIsJustOneShell is not null)
				{
					return returnIfThereIsJustOneShell;
				}

				throw new InvalidOperationException($"Unable to determine the current Shell instance you want to use. Please access Shell via the Windows property on {Application.Current.GetType()}.");
			}
		}

		internal ShellNavigationManager NavigationManager => _navigationManager;
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GoToAsync'][1]/Docs/*" />
		public Task GoToAsync(ShellNavigationState state)
		{
			return _navigationManager.GoToAsync(state, null, false);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GoToAsync'][2]/Docs/*" />
		public Task GoToAsync(ShellNavigationState state, bool animate)
		{
			return _navigationManager.GoToAsync(state, animate, false);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GoToAsync'][1]/Docs/*" />
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		public Task GoToAsync(ShellNavigationState state, IDictionary<string, object> parameters)
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		{
			return _navigationManager.GoToAsync(state, null, false, parameters: new ShellRouteParameters(parameters));
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GoToAsync'][2]/Docs/*" />
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		public Task GoToAsync(ShellNavigationState state, bool animate, IDictionary<string, object> parameters)
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		{
			return _navigationManager.GoToAsync(state, animate, false, parameters: new ShellRouteParameters(parameters));
		}

		/// <summary>
		/// This method navigates to a <see cref="ShellNavigationState" /> and returns a <see cref="Task" /> that will complete once the navigation animation.
		/// </summary>
		/// <param name="state">Defines the path for Shell to navigate to.</param>
		/// <param name="shellNavigationQueryParameters">Parameters to use for this specific navigation operation.</param>
		/// <returns></returns>
		public Task GoToAsync(ShellNavigationState state, ShellNavigationQueryParameters shellNavigationQueryParameters)
		{
			return _navigationManager.GoToAsync(state, null, false, parameters: new ShellRouteParameters(shellNavigationQueryParameters));
		}

		/// <summary>
		/// This method navigates to a <see cref="ShellNavigationState" /> and returns a <see cref="Task" />.
		/// </summary>
		/// <param name="state">Defines the path for Shell to navigate to.</param>
		/// <param name="animate">Indicates if your transition is animated</param>
		/// <param name="shellNavigationQueryParameters">Parameters to use for this specific navigation operation.</param>
		/// <returns></returns>
		public Task GoToAsync(ShellNavigationState state, bool animate, ShellNavigationQueryParameters shellNavigationQueryParameters)
		{
			return _navigationManager.GoToAsync(state, animate, false, parameters: new ShellRouteParameters(shellNavigationQueryParameters));
		}

		/// <summary>
		/// The currently selected ShellItem.
		/// </summary>
		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(nameof(CurrentItem), typeof(ShellItem), typeof(Shell), null, BindingMode.TwoWay,
				propertyChanging: OnCurrentItemChanging,
				propertyChanged: OnCurrentItemChanged);

		/// <summary>Bindable property for <see cref="CurrentState"/>.</summary>
		public static readonly BindableProperty CurrentStateProperty = CurrentStatePropertyKey.BindableProperty;

		/// <summary>
		/// Sets the flyout background image, of type ImageSource, to a file, embedded resource, URI, or stream.
		/// </summary>
		/// <remarks>
		/// The flyout background image appears beneath the flyout header and behind any flyout items, menu items, and the flyout footer. 
		/// </remarks>
		public static readonly BindableProperty FlyoutBackgroundImageProperty =
			BindableProperty.Create(nameof(FlyoutBackgroundImage), typeof(ImageSource), typeof(Shell), default(ImageSource), BindingMode.OneTime);

		/// <summary>
		/// The aspect ratio of the background image.
		/// </summary>
		public static readonly BindableProperty FlyoutBackgroundImageAspectProperty =
			BindableProperty.Create(nameof(FlyoutBackgroundImageAspect), typeof(Aspect), typeof(Shell), default(Aspect), BindingMode.OneTime);

		/// <summary>
		/// The background color of the Shell Flyout.
		/// </summary>
		public static readonly BindableProperty FlyoutBackgroundColorProperty =
			BindableProperty.Create(nameof(FlyoutBackgroundColor), typeof(Color), typeof(Shell), null, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="FlyoutBackground"/>.</summary>
		public static readonly BindableProperty FlyoutBackgroundProperty =
			BindableProperty.Create(nameof(FlyoutBackground), typeof(Brush), typeof(Shell), SolidColorBrush.Default, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="FlyoutHeaderBehavior"/>.</summary>
		public static readonly BindableProperty FlyoutHeaderBehaviorProperty =
			BindableProperty.Create(nameof(FlyoutHeaderBehavior), typeof(FlyoutHeaderBehavior), typeof(Shell), FlyoutHeaderBehavior.Default, BindingMode.OneTime);

		/// <summary>
		/// The flyout header appearance.
		/// The flyout header is the content that optionally appears at the top of the flyout.
		/// </summary>
		public static readonly BindableProperty FlyoutHeaderProperty =
			BindableProperty.Create(nameof(FlyoutHeader), typeof(object), typeof(Shell), null, BindingMode.OneTime,
				propertyChanging: OnFlyoutHeaderChanging);

		/// <summary>
		/// The flyout footer appearance.
		/// The flyout footer is the content that optionally appears at the bottom of the flyout.
		/// </summary>
		public static readonly BindableProperty FlyoutFooterProperty =
			BindableProperty.Create(nameof(FlyoutFooter), typeof(object), typeof(Shell), null, BindingMode.OneTime,
				propertyChanging: OnFlyoutFooterChanging);

		/// <summary>
		/// The flyout header appearance can be defined by setting a <see cref = "DataTemplate" />.
		/// </summary>
		public static readonly BindableProperty FlyoutHeaderTemplateProperty =
			BindableProperty.Create(nameof(FlyoutHeaderTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime,
				propertyChanging: OnFlyoutHeaderTemplateChanging);

		/// <summary>
		/// The flyout footer appearance can be defined by setting a <see cref = "DataTemplate" />.
		/// </summary>
		public static readonly BindableProperty FlyoutFooterTemplateProperty =
			BindableProperty.Create(nameof(FlyoutFooterTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime,
				propertyChanging: OnFlyoutFooterTemplateChanging);

		/// <summary>
		/// The flyout can be programmatically opened and closed by setting the FlyoutIsPresented property to a boolean value that indicates whether the flyout is currently open.
		/// </summary>
		public static readonly BindableProperty FlyoutIsPresentedProperty =
			BindableProperty.Create(nameof(FlyoutIsPresented), typeof(bool), typeof(Shell), false, BindingMode.TwoWay);

		/// <summary>Bindable property for <see cref="Items"/>.</summary>
		public static readonly BindableProperty ItemsProperty = ItemsPropertyKey.BindableProperty;

		/// <summary>
		/// By default, Shell applications have a hamburger icon which, when pressed, opens the flyout.
		/// This icon can be changed by setting the FlyoutIcon property.
		/// </summary>
		public static readonly BindableProperty FlyoutIconProperty =
			BindableProperty.Create(nameof(FlyoutIcon), typeof(ImageSource), typeof(Shell), null);

		/// <summary>
		/// Modifies the behavior of the flyout scroll.
		/// By default, a flyout can be scrolled vertically when the flyout items don't fit in the flyout. 
		/// </summary>
		public static readonly BindableProperty FlyoutVerticalScrollModeProperty =
			BindableProperty.Create(nameof(FlyoutVerticalScrollMode), typeof(ScrollMode), typeof(Shell), ScrollMode.Auto);

		View _flyoutHeaderView;
		View _flyoutFooterView;
		ShellNavigationManager _navigationManager;
		ShellFlyoutItemsManager _flyoutManager;
		Page _previousPage;

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Shell()
		{
			Toolbar = new ShellToolbar(this);

			_navigationManager = new ShellNavigationManager(this);
			_navigationManager.Navigated += (_, args) => SendNavigated(args);
			_navigationManager.Navigating += (_, args) => SendNavigating(args);

			_flyoutManager = new ShellFlyoutItemsManager(this);
			Navigation = new NavigationImpl(this);
			Route = Routing.GenerateImplicitRoute("shell");
			Initialize();

			if (Application.Current != null)
			{
				this.SetBinding(Shell.FlyoutBackgroundColorProperty,
					new AppThemeBinding { Light = Colors.White, Dark = Colors.Black, Mode = BindingMode.OneWay });
			}

			ShellController.FlyoutItemsChanged += (_, __) => Handler?.UpdateValue(nameof(FlyoutItems));
			ShellController.ItemsCollectionChanged += (_, __) => Handler?.UpdateValue(nameof(Items));
		}

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);

			if (Application.Current == null)
				return;

			if (args.NewHandler == null)
#pragma warning disable CS0618 // Type or member is obsolete
				Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;

			if (args.NewHandler != null && args.OldHandler == null)
				Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			ShellController.AppearanceChanged(CurrentPage, false);
		}

		void Initialize()
		{
			if (CurrentItem != null)
				SetCurrentItem()
					.FireAndForget();

			((ShellElementCollection)Items).VisibleItemsChangedInternal += async (s, e) =>
			{
				await SetCurrentItem();

				SendStructureChanged();
				SendFlyoutItemsChanged();
			};

			async Task SetCurrentItem()
			{
				var shellItems = ShellController.GetItems();

				if (CurrentItem != null && shellItems.Contains(CurrentItem))
					return;

				ShellItem shellItem = null;

				// If shell item has been removed try to renavigate to current location
				// Just in case the item was replaced. This is mainly relevant for hot reload
				if (CurrentItem != null)
				{
					try
					{
						var location = CurrentState.Location;
						var navRequest = ShellUriHandler.GetNavigationRequest(this, ((ShellNavigationState)location).FullLocation, false, false);

						if (navRequest != null)
						{
							var item = navRequest.Request.Item;
							var section = navRequest.Request.Section;
							var Content = navRequest.Request.Content;

							if (IsValidRoute(item) && IsValidRoute(section) && IsValidRoute(Content))
							{
								await GoToAsync(location, false);
								return;
							}

							bool IsValidRoute(BaseShellItem baseShellItem)
							{
								if (baseShellItem == null)
									return true;

								if (!baseShellItem.IsVisible)
									return false;

								return baseShellItem.IsPartOfVisibleTree();
							}
						}
					}
					catch (Exception exc)
					{
						Application.Current?.FindMauiContext()?.CreateLogger<Shell>()?.LogWarning(exc, "If you're using hot reload add a route to everything in your shell file");
					}
				}

				if (shellItem == null)
				{
					foreach (var item in shellItems)
					{
						if (item is ShellItem && ValidDefaultShellItem(item))
						{
							shellItem = item;
							break;
						}
					}
				}

				if (shellItem != null)
					await OnFlyoutItemSelectedAsync(shellItem, false).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Modifies the behavior of the flyout scroll.
		/// </summary>
		public ScrollMode FlyoutVerticalScrollMode
		{
			get => (ScrollMode)GetValue(FlyoutVerticalScrollModeProperty);
			set => SetValue(FlyoutVerticalScrollModeProperty, value);
		}

		public event EventHandler<ShellNavigatedEventArgs> Navigated;
		public event EventHandler<ShellNavigatingEventArgs> Navigating;


		/// <summary>
		/// Gets or sets the icon that, when pressed, opens the flyout.
		/// </summary>
		public ImageSource FlyoutIcon
		{
			get => (ImageSource)GetValue(FlyoutIconProperty);
			set => SetValue(FlyoutIconProperty, value);
		}

		/// <summary>
		/// The currently selected ShellItem.
		/// </summary>
		public ShellItem CurrentItem
		{
			get => (ShellItem)GetValue(CurrentItemProperty);
			set => SetValue(CurrentItemProperty, value);
		}

		internal ShellContent CurrentContent => CurrentItem?.CurrentItem?.CurrentItem;
		internal ShellSection CurrentSection => CurrentItem?.CurrentItem;

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='CurrentState']/Docs/*" />
		public ShellNavigationState CurrentState => (ShellNavigationState)GetValue(CurrentStateProperty);

		/// <summary>
		/// Gets or sets the flyout background image. Of type ImageSource, could be a file, embedded resource, URI, or stream.
		/// </summary>
		[System.ComponentModel.TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource FlyoutBackgroundImage
		{
			get => (ImageSource)GetValue(FlyoutBackgroundImageProperty);
			set => SetValue(FlyoutBackgroundImageProperty, value);
		}

		/// <summary>
		/// Gets or sets the aspect ratio of the background image.
		/// </summary>
		public Aspect FlyoutBackgroundImageAspect
		{
			get => (Aspect)GetValue(FlyoutBackgroundImageAspectProperty);
			set => SetValue(FlyoutBackgroundImageAspectProperty, value);
		}

		/// <summary>
		/// Gets or sets the background color of the flyout.
		/// </summary>
		public Color FlyoutBackgroundColor
		{
			get => (Color)GetValue(FlyoutBackgroundColorProperty);
			set => SetValue(FlyoutBackgroundColorProperty, value);
		}

		/// <summary>
		/// Gets or sets the background color of the Shell Flyout.
		/// </summary>
		public Brush FlyoutBackground
		{
			get => (Brush)GetValue(FlyoutBackgroundProperty);
			set => SetValue(FlyoutBackgroundProperty, value);
		}

		/// <summary>
		/// Gets or sets the backdrop of the flyout, which is the appearance of the flyout overlay.
		/// </summary>
		public Brush FlyoutBackdrop
		{
			get => (Brush)GetValue(FlyoutBackdropProperty);
			set => SetValue(FlyoutBackdropProperty, value);
		}

		/// <summary>
		/// Gets or sets the width of the flyout.
		/// </summary>
		public double FlyoutWidth
		{
			get => (double)GetValue(FlyoutWidthProperty);
			set => SetValue(FlyoutWidthProperty, value);
		}

		/// <summary>
		/// Gets or sets the height of the flyout.
		/// </summary>
		public double FlyoutHeight
		{
			get => (double)GetValue(FlyoutHeightProperty);
			set => SetValue(FlyoutHeightProperty, value);
		}

		/// <summary>
		/// Gets or sets the behavior to open the flyout.
		/// </summary>
		public FlyoutBehavior FlyoutBehavior
		{
			get => (FlyoutBehavior)GetValue(FlyoutBehaviorProperty);
			set => SetValue(FlyoutBehaviorProperty, value);
		}

		/// <summary>
		/// Gets or sets the View that define the appearance of the flyout header.
		/// The flyout header is the content that optionally appears at the top of the flyout.
		/// </summary>
		public object FlyoutHeader
		{
			get => GetValue(FlyoutHeaderProperty);
			set => SetValue(FlyoutHeaderProperty, value);
		}

		/// <summary>
		/// Gets or sets the View that define the appearance of the flyout footer.
		/// The flyout footer is the content that optionally appears at the bottom of the flyout.
		/// </summary>
		public object FlyoutFooter
		{
			get => GetValue(FlyoutFooterProperty);
			set => SetValue(FlyoutFooterProperty, value);
		}

		/// <summary>
		/// Gets or sets the header behavior for the flyout.
		/// </summary>
		public FlyoutHeaderBehavior FlyoutHeaderBehavior
		{
			get => (FlyoutHeaderBehavior)GetValue(FlyoutHeaderBehaviorProperty);
			set => SetValue(FlyoutHeaderBehaviorProperty, value);
		}

		/// <summary>
		/// Gets or sets the flyout header appearance using a <see cref = "DataTemplate" />.
		/// </summary>
		public DataTemplate FlyoutHeaderTemplate
		{
			get => (DataTemplate)GetValue(FlyoutHeaderTemplateProperty);
			set => SetValue(FlyoutHeaderTemplateProperty, value);
		}

		/// <summary>
		/// Gets or sets the flyout footer appearance using a <see cref = "DataTemplate" />.
		/// </summary>
		public DataTemplate FlyoutFooterTemplate
		{
			get => (DataTemplate)GetValue(FlyoutFooterTemplateProperty);
			set => SetValue(FlyoutFooterTemplateProperty, value);
		}


		/// <summary>
		/// Gets or sets the visible status of the flyout.
		/// </summary>
		public bool FlyoutIsPresented
		{
			get => (bool)GetValue(FlyoutIsPresentedProperty);
			set => SetValue(FlyoutIsPresentedProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='Items']/Docs/*" />
		public IList<ShellItem> Items => (IList<ShellItem>)GetValue(ItemsProperty);

		/// <summary>
		/// Gets or sets <see cref = "DataTemplate" /> applied to each of the Items.
		/// </summary>
		public DataTemplate ItemTemplate
		{
			get => GetItemTemplate(this);
			set => SetItemTemplate(this, value);
		}

		/// <summary>
		/// Gets or sets the <see cref = "DataTemplate" /> applied to MenuItem objects in the MenuItems collection.
		/// </summary>
		public DataTemplate MenuItemTemplate
		{
			get => GetMenuItemTemplate(this);
			set => SetMenuItemTemplate(this, value);
		}

		internal string Route
		{
			get => Routing.GetRoute(this);
			set => Routing.SetRoute(this, value);
		}

		internal string RouteHost { get; set; } = "shell";

		internal string RouteScheme { get; set; } = "app";

		View FlyoutHeaderView
		{
			get => _flyoutHeaderView;
		}

		View FlyoutFooterView
		{
			get => _flyoutFooterView;
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			if (FlyoutHeaderView != null)
				SetInheritedBindingContext(FlyoutHeaderView, BindingContext);

			if (FlyoutFooterView != null)
				SetInheritedBindingContext(FlyoutFooterView, BindingContext);

			if (FlyoutContentView != null)
				SetInheritedBindingContext(FlyoutContentView, BindingContext);
		}


		internal void SendFlyoutItemsChanged() => _flyoutManager.CheckIfFlyoutItemsChanged();

		public IEnumerable FlyoutItems => _flyoutManager.FlyoutItems;

		List<List<Element>> IShellController.GenerateFlyoutGrouping() =>
			_flyoutManager.GenerateFlyoutGrouping();

		internal void SendStructureChanged()
		{
			UpdateChecked(this);
			_structureChanged?.Invoke(this, EventArgs.Empty);
		}

		protected override bool OnBackButtonPressed()
		{
#if WINDOWS || !PLATFORM
			var backButtonBehavior = GetBackButtonBehavior(GetVisiblePage());
			if (backButtonBehavior != null)
			{
				var command = backButtonBehavior.GetPropertyIfSet<ICommand>(BackButtonBehavior.CommandProperty, null);
				var commandParameter = backButtonBehavior.GetPropertyIfSet<object>(BackButtonBehavior.CommandParameterProperty, null);

				if (command != null)
				{
					command.Execute(commandParameter);
					return true;
				}
			}
#endif

			if (GetVisiblePage() is Page page && page.SendBackButtonPressed())
				return true;

			var currentContent = CurrentItem?.CurrentItem;
			if (currentContent != null && currentContent.Stack.Count > 1)
			{
				NavigationPop();
				return true;
			}

			var args = new ShellNavigatingEventArgs(this.CurrentState, new ShellNavigationState(""), ShellNavigationSource.Pop, true);
			_navigationManager.HandleNavigating(args);
			return args.Cancelled;

			async void NavigationPop()
			{
				try
				{
					await currentContent.Navigation.PopAsync();
				}
				catch (Exception exc)
				{
					Application.Current?.FindMauiContext()?.CreateLogger<Shell>()?.LogWarning(exc, "Failed to Navigate Back");
				}
			}
		}

		bool ValidDefaultShellItem(Element child) => !(child is MenuShellItem);

		void SendNavigated(ShellNavigatedEventArgs args)
		{
			Navigated?.Invoke(this, args);
			OnNavigated(args);

			if (_previousPage != null)
				_previousPage.PropertyChanged -= OnCurrentPagePropertyChanged;

			NavigationType navigationType = NavigationType.PageSwap;

			switch (args.Source)
			{
				case ShellNavigationSource.Pop:
					navigationType = NavigationType.Pop;
					break;
				case ShellNavigationSource.ShellItemChanged:
					navigationType = NavigationType.PageSwap;
					break;
				case ShellNavigationSource.ShellSectionChanged:
					navigationType = NavigationType.PageSwap;
					break;
				case ShellNavigationSource.ShellContentChanged:
					navigationType = NavigationType.PageSwap;
					break;
				case ShellNavigationSource.Push:
					navigationType = NavigationType.Push;
					break;
				case ShellNavigationSource.PopToRoot:
					navigationType = NavigationType.PopToRoot;
					break;
				case ShellNavigationSource.Insert:
					navigationType = NavigationType.Insert;
					break;
			}

			_previousPage?.SendNavigatedFrom(new NavigatedFromEventArgs(CurrentPage, navigationType));
			CurrentPage?.SendNavigatedTo(new NavigatedToEventArgs(_previousPage));
			_previousPage = null;

			if (CurrentPage != null)
				CurrentPage.PropertyChanged += OnCurrentPagePropertyChanged;

			CurrentItem?.Handler?.UpdateValue(Shell.TabBarIsVisibleProperty.PropertyName);
		}

		internal PropertyChangedEventHandler CurrentPagePropertyChanged;

		void OnCurrentPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			CurrentPagePropertyChanged?.Invoke(this, e);

			if (e.Is(Shell.TabBarIsVisibleProperty))
				CurrentItem?.Handler?.UpdateValue(Shell.TabBarIsVisibleProperty.PropertyName);
		}

		void SendNavigating(ShellNavigatingEventArgs args)
		{
			Navigating?.Invoke(this, args);
			OnNavigating(args);

			if (!args.Cancelled)
			{
				_previousPage = CurrentPage;
				CurrentPage?.SendNavigatingFrom(new NavigatingFromEventArgs());
			}
		}

		protected virtual void OnNavigated(ShellNavigatedEventArgs args)
		{
		}

		protected virtual void OnNavigating(ShellNavigatingEventArgs args)
		{

		}

		static void OnCurrentItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is ShellItem oldShellItem)
			{
				oldShellItem.SendDisappearing();

				foreach (var section in oldShellItem.Items)
				{
					foreach (var content in section.Items)
					{
						content.EvaluateDisconnect();
					}
				}
			}

			if (newValue == null)
				return;

			if (newValue is ShellItem newShellItem)
				newShellItem.SendAppearing();

			var shell = (Shell)bindable;
			UpdateChecked(shell);

			shell.ShellController.AppearanceChanged(shell, false);
			shell.ShellController.UpdateCurrentState(ShellNavigationSource.ShellItemChanged);

			if (shell.CurrentItem?.CurrentItem != null)
				shell.ShellController.AppearanceChanged(shell.CurrentItem.CurrentItem, false);
		}

		static void OnCurrentItemChanging(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			var shellItem = (ShellItem)newValue;

			if (!shell.Items.Contains(shellItem))
				shell.Items.Add(shellItem);

			var shellSection = shellItem.CurrentItem;
			var shellContent = shellSection.CurrentItem;
			var stack = shellSection.Stack;
			shell._navigationManager.ProposeNavigationOutsideGotoAsync(ShellNavigationSource.ShellItemChanged, shellItem, shellSection, shellContent, stack, false, true);
		}

		static void UpdateChecked(Element root, bool isChecked = true)
		{
			if (root is BaseShellItem baseItem)
			{
				if (!isChecked && !baseItem.IsChecked)
					return;
				baseItem.SetValue(BaseShellItem.IsCheckedPropertyKey, isChecked);
			}

			if (root is Shell shell)
			{
				ShellItem currentItem = shell.CurrentItem;
				var items = shell.ShellController.GetItems();
				var count = items.Count;
				for (int i = 0; i < count; i++)
				{
					ShellItem item = items[i];
					UpdateChecked(item, isChecked && item == currentItem);
				}
			}
			else if (root is ShellItem shellItem)
			{
				var currentItem = shellItem.CurrentItem;
				var items = (shellItem as IShellItemController).GetItems();
				var count = items.Count;
				for (int i = 0; i < count; i++)
				{
					ShellSection item = items[i];
					UpdateChecked(item, isChecked && item == currentItem);
				}
			}
			else if (root is ShellSection shellSection)
			{
				var currentItem = shellSection.CurrentItem;
				var items = (shellSection as IShellSectionController).GetItems();
				var count = items.Count;
				for (int i = 0; i < count; i++)
				{
					ShellContent item = items[i];
					UpdateChecked(item, isChecked && item == currentItem);
				}
			}
		}

		static void OnFlyoutHeaderChanging(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			shell.OnFlyoutHeaderChanged(oldValue, newValue);
		}

		static void OnFlyoutFooterChanging(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			shell.OnFlyoutFooterChanged(oldValue, newValue);
		}

		static void OnFlyoutHeaderTemplateChanging(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			shell.OnFlyoutHeaderTemplateChanged((DataTemplate)oldValue, (DataTemplate)newValue);
		}

		static void OnFlyoutFooterTemplateChanging(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			shell.OnFlyoutFooterTemplateChanged((DataTemplate)oldValue, (DataTemplate)newValue);
		}

		static void OnTitleViewChanged(BindableObject bindable, object oldValue, object newValue) =>
			bindable.AddRemoveLogicalChildren(oldValue, newValue);

		internal FlyoutBehavior GetEffectiveFlyoutBehavior()
		{
			ShellItem rootItem = null;
			return GetEffectiveValue(Shell.FlyoutBehaviorProperty,
				() =>
				{
					if (this.IsSet(FlyoutBehaviorProperty))
						return FlyoutBehavior;

					if (rootItem is FlyoutItem)
						return FlyoutBehavior.Flyout;
					else if (rootItem is TabBar)
						return FlyoutBehavior.Disabled;
					// This means the user hasn't specified
					// a ShellItem so we don't want the flyout to show up
					// if there is only one ShellItem.
					//
					// This will happen if the user only specifies a
					// single ContentPage
					else if (rootItem != null && Routing.IsImplicit(rootItem))
					{
						if (Items.Count <= 1)
							return FlyoutBehavior.Disabled;
					}

					return FlyoutBehavior;
				},
				(o) => rootItem = rootItem ?? o as ShellItem);
		}

		internal T GetEffectiveValue<T>(BindableProperty property, T defaultValue, bool ignoreImplicit = false)
		{
			return GetEffectiveValue<T>(property, () => defaultValue, null, ignoreImplicit: ignoreImplicit);
		}

		internal T GetEffectiveValue<T>(
			BindableProperty property,
			Func<T> defaultValue,
			Action<Element> observer,
			Element element = null,
			bool ignoreImplicit = false)
		{
			element = element ?? (Element)GetCurrentShellPage() ?? CurrentContent;
			while (element != this && element != null)
			{
				observer?.Invoke(element);

				if (ignoreImplicit && Routing.IsImplicit(element))
				{
					// If this is an implicitly created route.
					// A route that the user doesn't have inside their Shell file.
					// Then we don't want to consider it as a value to use.
					// So we let the code just go to the next parent.
				}
				else if (element.IsSet(property))
					return (T)element.GetValue(property);

				element = element.Parent;
			}

			if (defaultValue == null)
				return default(T);

			return defaultValue();
		}

		ShellAppearance GetAppearanceForPivot(Element pivot)
		{
			// this algorithm is pretty simple
			// 1) Get the "CurrentPage" by walking down from the pivot
			//		Walking down goes Shell -> ShellItem -> ShellContent -> ShellContent.Stack.Last
			// 2) Walk up from the pivot to the root Shell. Stop walking as soon as you find a ShellAppearance and return
			// 3) If nothing found, return null

			pivot = WalkToPage(pivot);

			bool foundShellContent = false;
			bool anySet = false;
			ShellAppearance result = new ShellAppearance();
			// Now we walk up
			while (!Application.IsApplicationOrWindowOrNull(pivot))
			{
				if (pivot is ShellContent)
					foundShellContent = true;

				// One minor deviation here. Even though a pushed page is technically the child of
				// a ShellSection and not the ShellContent, we want the root ShellContent to 
				// be taken into account. Yes this could behave oddly if the developer switches
				// tabs while a page is pushed, however that is in the developers wheelhouse
				// and this will be the generally expected behavior.
				if (!foundShellContent && pivot is ShellSection shellSection && shellSection.CurrentItem != null)
				{
					if (result.Ingest(shellSection.CurrentItem))
						anySet = true;
				}

				if (result.Ingest(pivot))
					anySet = true;

				pivot = pivot.Parent;
			}

			if (anySet)
			{
				result.MakeComplete();
				return result;
			}
			return null;
		}

		void NotifyFlyoutBehaviorObservers()
		{
			if (CurrentItem == null || GetVisiblePage() == null)
				return;

			var behavior = (this as IFlyoutView).FlyoutBehavior;
			for (int i = 0; i < _flyoutBehaviorObservers.Count; i++)
				_flyoutBehaviorObservers[i].OnFlyoutBehaviorChanged(behavior);

			Handler?.UpdateValue(nameof(IFlyoutView.FlyoutBehavior));
		}

		void OnFlyoutHeaderChanged(object oldVal, object newVal)
		{
			ShellTemplatedViewManager.OnViewDataChanged(
				FlyoutHeaderTemplate,
				ref _flyoutHeaderView,
				newVal,
				(element) => RemoveLogicalChild(element),
				AddLogicalChild);
		}

		void OnFlyoutHeaderTemplateChanged(DataTemplate oldValue, DataTemplate newValue)
		{
			ShellTemplatedViewManager.OnViewTemplateChanged(
				newValue,
				ref _flyoutHeaderView,
				FlyoutHeader,
				(element) => RemoveLogicalChild(element),
				AddLogicalChild,
				this);
		}

		void OnFlyoutFooterChanged(object oldVal, object newVal)
		{
			ShellTemplatedViewManager.OnViewDataChanged(
				FlyoutFooterTemplate,
				ref _flyoutFooterView,
				newVal,
				(element) => RemoveLogicalChild(element),
				AddLogicalChild);
		}

		void OnFlyoutFooterTemplateChanged(DataTemplate oldValue, DataTemplate newValue)
		{
			ShellTemplatedViewManager.OnViewTemplateChanged(
				newValue,
				ref _flyoutFooterView,
				FlyoutFooter,
				(element) => RemoveLogicalChild(element),
				AddLogicalChild,
				this);
		}

		internal Element GetVisiblePage()
		{
			if (CurrentItem?.CurrentItem is IShellSectionController scc)
				return scc.PresentedPage;

			return null;
		}

		internal void SendPageAppearing(Page page)
		{
			if (Toolbar is ShellToolbar shellToolbar)
				shellToolbar.ApplyChanges();

			page.SendAppearing();
		}

		// This returns the current shell page that's visible
		// without including the modal stack
		internal Page GetCurrentShellPage()
		{
			var navStack = CurrentSection?.Navigation?.NavigationStack;
			Page currentPage = null;

			if (navStack != null)
			{
				currentPage = navStack[navStack.Count - 1] ??
					((IShellContentController)CurrentContent)?.Page;
			}

			return currentPage;
		}

		Element WalkToPage(Element element)
		{
			switch (element)
			{
				case Shell shell:
					element = shell.CurrentItem;
					break;
				case ShellItem shellItem:
					element = shellItem.CurrentItem;
					break;
				case ShellSection shellSection:
					var controller = (IShellSectionController)element;
					// this is the same as .Last but easier and will add in the root if not null
					// it generally wont be null but this is just in case
					element = controller.PresentedPage ?? element;
					break;
			}

			return element;
		}

		void IPropertyPropagationController.PropagatePropertyChanged(string propertyName)
		{
			PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, ((IVisualTreeElement)this).GetVisualChildren());
		}

		[Obsolete("Use ArrangeOverride instead")]
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			// Page by default tries to layout all logical children
			// we don't want this behavior with shell
		}

		IView IFlyoutView.Flyout => this.FlyoutContentView;
		IView IFlyoutView.Detail => null;

		bool IFlyoutView.IsPresented { get => FlyoutIsPresented; set => FlyoutIsPresented = value; }

		bool IFlyoutView.IsGestureEnabled => false;

		FlyoutBehavior IFlyoutView.FlyoutBehavior
		{
			get
			{
				return GetEffectiveFlyoutBehavior();
			}
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (propertyName == Shell.FlyoutIsPresentedProperty.PropertyName)
				Handler?.UpdateValue(nameof(IFlyoutView.IsPresented));
		}

		#region Shell Flyout Content


		/// <summary>
		/// Flyout items, which represent the flyout content.
		/// </summary>
		/// <remarks>
		/// Can optionally be replaced with custom content.
		/// </remarks>
		public static readonly BindableProperty FlyoutContentProperty =
			BindableProperty.Create(nameof(FlyoutContent), typeof(object), typeof(Shell), null, BindingMode.OneTime, propertyChanging: OnFlyoutContentChanging);

		/// <summary>
		/// The flyout content can be defined by setting a <see cref = "DataTemplate" />.
		/// A flyout header can optionally be displayed above your flyout content, and a flyout footer can optionally be displayed below your flyout content.
		/// </summary>
		public static readonly BindableProperty FlyoutContentTemplateProperty =
			BindableProperty.Create(nameof(FlyoutContentTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime, propertyChanging: OnFlyoutContentTemplateChanging);

		View _flyoutContentView;

		public object FlyoutContent
		{
			get => GetValue(FlyoutContentProperty);
			set => SetValue(FlyoutContentProperty, value);
		}

		public DataTemplate FlyoutContentTemplate
		{
			get => (DataTemplate)GetValue(FlyoutContentTemplateProperty);
			set => SetValue(FlyoutContentTemplateProperty, value);
		}

		View FlyoutContentView
		{
			get => _flyoutContentView;
		}

		void OnFlyoutContentChanged(object oldVal, object newVal)
		{
			ShellTemplatedViewManager.OnViewDataChanged(
				FlyoutContentTemplate,
				ref _flyoutContentView,
				newVal,
				(element) => RemoveLogicalChild(element),
				AddLogicalChild);
		}

		void OnFlyoutContentTemplateChanged(DataTemplate oldValue, DataTemplate newValue)
		{
			ShellTemplatedViewManager.OnViewTemplateChanged(
				newValue,
				ref _flyoutContentView,
				FlyoutContent,
				(element) => RemoveLogicalChild(element),
				AddLogicalChild,
				this);
		}

		static void OnFlyoutContentChanging(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			shell.OnFlyoutContentChanged(oldValue, newValue);
		}

		static void OnFlyoutContentTemplateChanging(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			shell.OnFlyoutContentTemplateChanged((DataTemplate)oldValue, (DataTemplate)newValue);
		}
		#endregion

		class NavigationImpl : NavigationProxy
		{
			readonly Shell _shell;

			NavigationProxy SectionProxy => _shell.CurrentItem?.CurrentItem?.NavigationProxy;

			public NavigationImpl(Shell shell) => _shell = shell;

			protected override IReadOnlyList<Page> GetNavigationStack() => SectionProxy?.NavigationStack;

			protected override void OnInsertPageBefore(Page page, Page before) => SectionProxy.InsertPageBefore(page, before);

			protected override Task<Page> OnPopAsync(bool animated) => SectionProxy.PopAsync(animated);

			protected override Task OnPopToRootAsync(bool animated) => SectionProxy.PopToRootAsync(animated);

			protected override Task OnPushAsync(Page page, bool animated) => SectionProxy.PushAsync(page, animated);

			protected override void OnRemovePage(Page page) => SectionProxy.RemovePage(page);

			protected override async Task<Page> OnPopModal(bool animated)
			{
				if (!_shell.NavigationManager.AccumulateNavigatedEvents)
				{
					var page = _shell.CurrentPage;
					await _shell.GoToAsync("..", animated);
					return page;
				}

				var modalPopped = await base.OnPopModal(animated);

				if (ModalStack.Count == 0 && !_shell.CurrentItem.CurrentItem.IsPoppingModalStack)
					_shell.CurrentItem.SendAppearing();

				return modalPopped;
			}

			protected override async Task OnPushModal(Page modal, bool animated)
			{
				if (_shell.CurrentSection is null)
				{
					await base.OnPushModal(modal, animated);
					return;
				}

				if (!_shell.NavigationManager.AccumulateNavigatedEvents)
				{
					// This will route the modal push through the shell section which is setup
					// to update the shell state after a modal push
					await _shell.CurrentSection.Navigation.PushModalAsync(modal, animated);
					return;
				}

				if (ModalStack.Count == 0)
					_shell.CurrentItem.SendDisappearing();

				await base.OnPushModal(modal, animated);

				modal.NavigationProxy.Inner = new NavigationImplWrapper(modal.NavigationProxy.Inner, this);
			}

			class NavigationImplWrapper : NavigationProxy
			{
				readonly INavigation _shellProxy;

				public NavigationImplWrapper(INavigation proxy, INavigation shellProxy)
				{
					Inner = proxy;
					_shellProxy = shellProxy;

				}

				protected override Task<Page> OnPopModal(bool animated) => _shellProxy.PopModalAsync(animated);

				protected override Task OnPushModal(Page modal, bool animated) => _shellProxy.PushModalAsync(modal, animated);
			}
		}
	}
}
