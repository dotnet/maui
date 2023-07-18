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
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='CurrentPage']/Docs/*" />
		public Page CurrentPage => GetVisiblePage() as Page;

		/// <summary>Bindable property for attached property <c>BackButtonBehavior</c>.</summary>
		public static readonly BindableProperty BackButtonBehaviorProperty =
			BindableProperty.CreateAttached("BackButtonBehavior", typeof(BackButtonBehavior), typeof(Shell), null, BindingMode.OneTime,
				propertyChanged: OnBackButonBehaviorPropertyChanged);

		static void OnBackButonBehaviorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is BackButtonBehavior oldHandlerBehavior)
				SetInheritedBindingContext(oldHandlerBehavior, null);
			if (newValue is BackButtonBehavior newHandlerBehavior)
				SetInheritedBindingContext(newHandlerBehavior, bindable.BindingContext);
		}

		/// <summary>Bindable property for attached property <c>PresentationMode</c>.</summary>
		public static readonly BindableProperty PresentationModeProperty = BindableProperty.CreateAttached("PresentationMode", typeof(PresentationMode), typeof(Shell), PresentationMode.Animated);

		/// <summary>Bindable property for attached property <c>FlyoutBehavior</c>.</summary>
		public static readonly BindableProperty FlyoutBehaviorProperty =
			BindableProperty.CreateAttached("FlyoutBehavior", typeof(FlyoutBehavior), typeof(Shell), FlyoutBehavior.Flyout,
				propertyChanged: OnFlyoutBehaviorChanged);

		/// <summary>Bindable property for attached property <c>NavBarIsVisible</c>.</summary>
		public static readonly BindableProperty NavBarIsVisibleProperty =
			BindableProperty.CreateAttached("NavBarIsVisible", typeof(bool), typeof(Shell), true);

		/// <summary>Bindable property for attached property <c>NavBarHasShadow</c>.</summary>
		public static readonly BindableProperty NavBarHasShadowProperty =
			BindableProperty.CreateAttached("NavBarHasShadow", typeof(bool), typeof(Shell), default(bool),
				defaultValueCreator: (b) => DeviceInfo.Platform == DevicePlatform.Android);

		/// <summary>Bindable property for attached property <c>SearchHandler</c>.</summary>
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

		/// <summary>Bindable property for attached property <c>FlyoutItemIsVisible</c>.</summary>
		public static readonly BindableProperty FlyoutItemIsVisibleProperty =
			BindableProperty.CreateAttached("FlyoutItemIsVisible", typeof(bool), typeof(Shell), true, propertyChanged: OnFlyoutItemIsVisibleChanged);
		public static bool GetFlyoutItemIsVisible(BindableObject obj) => (bool)obj.GetValue(FlyoutItemIsVisibleProperty);
		public static void SetFlyoutItemIsVisible(BindableObject obj, bool isVisible) => obj.SetValue(FlyoutItemIsVisibleProperty, isVisible);

		static void OnFlyoutItemIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is Element element)
				element
					.FindParentOfType<Shell>()
					?.SendFlyoutItemsChanged();
		}

		/// <summary>Bindable property for attached property <c>TabBarIsVisible</c>.</summary>
		public static readonly BindableProperty TabBarIsVisibleProperty =
			BindableProperty.CreateAttached("TabBarIsVisible", typeof(bool), typeof(Shell), true);

		/// <summary>Bindable property for attached property <c>TitleView</c>.</summary>
		public static readonly BindableProperty TitleViewProperty =
			BindableProperty.CreateAttached("TitleView", typeof(View), typeof(Shell), null, propertyChanged: OnTitleViewChanged);

		/// <summary>Bindable property for attached property <c>MenuItemTemplate</c>.</summary>
		public static readonly BindableProperty MenuItemTemplateProperty =
			BindableProperty.CreateAttached(nameof(MenuItemTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetMenuItemTemplate']/Docs/*" />
		public static DataTemplate GetMenuItemTemplate(BindableObject obj) => (DataTemplate)obj.GetValue(MenuItemTemplateProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetMenuItemTemplate']/Docs/*" />
		public static void SetMenuItemTemplate(BindableObject obj, DataTemplate menuItemTemplate) => obj.SetValue(MenuItemTemplateProperty, menuItemTemplate);

		/// <summary>Bindable property for attached property <c>ItemTemplate</c>.</summary>
		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.CreateAttached(nameof(ItemTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetItemTemplate']/Docs/*" />
		public static DataTemplate GetItemTemplate(BindableObject obj) => (DataTemplate)obj.GetValue(ItemTemplateProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetItemTemplate']/Docs/*" />
		public static void SetItemTemplate(BindableObject obj, DataTemplate itemTemplate) => obj.SetValue(ItemTemplateProperty, itemTemplate);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetBackButtonBehavior']/Docs/*" />
		public static BackButtonBehavior GetBackButtonBehavior(BindableObject obj) => (BackButtonBehavior)obj.GetValue(BackButtonBehaviorProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetBackButtonBehavior']/Docs/*" />
		public static void SetBackButtonBehavior(BindableObject obj, BackButtonBehavior behavior) => obj.SetValue(BackButtonBehaviorProperty, behavior);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetPresentationMode']/Docs/*" />
		public static PresentationMode GetPresentationMode(BindableObject obj) => (PresentationMode)obj.GetValue(PresentationModeProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetPresentationMode']/Docs/*" />
		public static void SetPresentationMode(BindableObject obj, PresentationMode presentationMode) => obj.SetValue(PresentationModeProperty, presentationMode);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetFlyoutBehavior']/Docs/*" />
		public static FlyoutBehavior GetFlyoutBehavior(BindableObject obj) => (FlyoutBehavior)obj.GetValue(FlyoutBehaviorProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetFlyoutBehavior']/Docs/*" />
		public static void SetFlyoutBehavior(BindableObject obj, FlyoutBehavior value) => obj.SetValue(FlyoutBehaviorProperty, value);

		public static double GetFlyoutWidth(BindableObject obj) => (double)obj.GetValue(FlyoutWidthProperty);
		public static void SetFlyoutWidth(BindableObject obj, double value) => obj.SetValue(FlyoutWidthProperty, value);

		public static double GetFlyoutHeight(BindableObject obj) => (double)obj.GetValue(FlyoutHeightProperty);
		public static void SetFlyoutHeight(BindableObject obj, double value) => obj.SetValue(FlyoutHeightProperty, value);


		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetNavBarIsVisible']/Docs/*" />
		public static bool GetNavBarIsVisible(BindableObject obj) => (bool)obj.GetValue(NavBarIsVisibleProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetNavBarIsVisible']/Docs/*" />
		public static void SetNavBarIsVisible(BindableObject obj, bool value) => obj.SetValue(NavBarIsVisibleProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetNavBarHasShadow']/Docs/*" />
		public static bool GetNavBarHasShadow(BindableObject obj) => (bool)obj.GetValue(NavBarHasShadowProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetNavBarHasShadow']/Docs/*" />
		public static void SetNavBarHasShadow(BindableObject obj, bool value) => obj.SetValue(NavBarHasShadowProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetSearchHandler']/Docs/*" />
		public static SearchHandler GetSearchHandler(BindableObject obj) => (SearchHandler)obj.GetValue(SearchHandlerProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetSearchHandler']/Docs/*" />
		public static void SetSearchHandler(BindableObject obj, SearchHandler handler) => obj.SetValue(SearchHandlerProperty, handler);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetTabBarIsVisible']/Docs/*" />
		public static bool GetTabBarIsVisible(BindableObject obj) => (bool)obj.GetValue(TabBarIsVisibleProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetTabBarIsVisible']/Docs/*" />
		public static void SetTabBarIsVisible(BindableObject obj, bool value) => obj.SetValue(TabBarIsVisibleProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetTitleView']/Docs/*" />
		public static View GetTitleView(BindableObject obj) => (View)obj.GetValue(TitleViewProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetTitleView']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='BackgroundColorProperty']/Docs/*" />
		public static readonly new BindableProperty BackgroundColorProperty =
			BindableProperty.CreateAttached("BackgroundColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>DisabledColor</c>.</summary>
		public static readonly BindableProperty DisabledColorProperty =
			BindableProperty.CreateAttached("DisabledColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>ForegroundColor</c>.</summary>
		public static readonly BindableProperty ForegroundColorProperty =
			BindableProperty.CreateAttached("ForegroundColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>TabBarBackgroundColor</c>.</summary>
		public static readonly BindableProperty TabBarBackgroundColorProperty =
			BindableProperty.CreateAttached("TabBarBackgroundColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>TabBarDisabledColor</c>.</summary>
		public static readonly BindableProperty TabBarDisabledColorProperty =
			BindableProperty.CreateAttached("TabBarDisabledColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>TabBarForegroundColor</c>.</summary>
		public static readonly BindableProperty TabBarForegroundColorProperty =
			BindableProperty.CreateAttached("TabBarForegroundColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>TabBarTitleColor</c>.</summary>
		public static readonly BindableProperty TabBarTitleColorProperty =
			BindableProperty.CreateAttached("TabBarTitleColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>TabBarUnselectedColor</c>.</summary>
		public static readonly BindableProperty TabBarUnselectedColorProperty =
			BindableProperty.CreateAttached("TabBarUnselectedColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>TitleColor</c>.</summary>
		public static readonly BindableProperty TitleColorProperty =
			BindableProperty.CreateAttached("TitleColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>UnselectedColor</c>.</summary>
		public static readonly BindableProperty UnselectedColorProperty =
			BindableProperty.CreateAttached("UnselectedColor", typeof(Color), typeof(Shell), null,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>FlyoutBackdrop</c>.</summary>
		public static readonly BindableProperty FlyoutBackdropProperty =
			BindableProperty.CreateAttached("FlyoutBackdrop", typeof(Brush), typeof(Shell), Brush.Default,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>FlyoutWidth</c>.</summary>
		public static readonly BindableProperty FlyoutWidthProperty =
			BindableProperty.CreateAttached("FlyoutWidth", typeof(double), typeof(Shell), -1d,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <summary>Bindable property for attached property <c>FlyoutHeight</c>.</summary>
		public static readonly BindableProperty FlyoutHeightProperty =
			BindableProperty.CreateAttached("FlyoutHeight", typeof(double), typeof(Shell), -1d,
				propertyChanged: OnShellAppearanceValueChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetBackgroundColor']/Docs/*" />
		public static Color GetBackgroundColor(BindableObject obj) => (Color)obj.GetValue(BackgroundColorProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetBackgroundColor']/Docs/*" />
		public static void SetBackgroundColor(BindableObject obj, Color value) => obj.SetValue(BackgroundColorProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetDisabledColor']/Docs/*" />
		public static Color GetDisabledColor(BindableObject obj) => (Color)obj.GetValue(DisabledColorProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetDisabledColor']/Docs/*" />
		public static void SetDisabledColor(BindableObject obj, Color value) => obj.SetValue(DisabledColorProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetForegroundColor']/Docs/*" />
		public static Color GetForegroundColor(BindableObject obj) => (Color)obj.GetValue(ForegroundColorProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetForegroundColor']/Docs/*" />
		public static void SetForegroundColor(BindableObject obj, Color value) => obj.SetValue(ForegroundColorProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetTabBarBackgroundColor']/Docs/*" />
		public static Color GetTabBarBackgroundColor(BindableObject obj) => (Color)obj.GetValue(TabBarBackgroundColorProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetTabBarBackgroundColor']/Docs/*" />
		public static void SetTabBarBackgroundColor(BindableObject obj, Color value) => obj.SetValue(TabBarBackgroundColorProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetTabBarDisabledColor']/Docs/*" />
		public static Color GetTabBarDisabledColor(BindableObject obj) => (Color)obj.GetValue(TabBarDisabledColorProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetTabBarDisabledColor']/Docs/*" />
		public static void SetTabBarDisabledColor(BindableObject obj, Color value) => obj.SetValue(TabBarDisabledColorProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetTabBarForegroundColor']/Docs/*" />
		public static Color GetTabBarForegroundColor(BindableObject obj) => (Color)obj.GetValue(TabBarForegroundColorProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetTabBarForegroundColor']/Docs/*" />
		public static void SetTabBarForegroundColor(BindableObject obj, Color value) => obj.SetValue(TabBarForegroundColorProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetTabBarTitleColor']/Docs/*" />
		public static Color GetTabBarTitleColor(BindableObject obj) => (Color)obj.GetValue(TabBarTitleColorProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetTabBarTitleColor']/Docs/*" />
		public static void SetTabBarTitleColor(BindableObject obj, Color value) => obj.SetValue(TabBarTitleColorProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetTabBarUnselectedColor']/Docs/*" />
		public static Color GetTabBarUnselectedColor(BindableObject obj) => (Color)obj.GetValue(TabBarUnselectedColorProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetTabBarUnselectedColor']/Docs/*" />
		public static void SetTabBarUnselectedColor(BindableObject obj, Color value) => obj.SetValue(TabBarUnselectedColorProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetTitleColor']/Docs/*" />
		public static Color GetTitleColor(BindableObject obj) => (Color)obj.GetValue(TitleColorProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetTitleColor']/Docs/*" />
		public static void SetTitleColor(BindableObject obj, Color value) => obj.SetValue(TitleColorProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetUnselectedColor']/Docs/*" />
		public static Color GetUnselectedColor(BindableObject obj) => (Color)obj.GetValue(UnselectedColorProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetUnselectedColor']/Docs/*" />
		public static void SetUnselectedColor(BindableObject obj, Color value) => obj.SetValue(UnselectedColorProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='GetFlyoutBackdrop']/Docs/*" />
		public static Brush GetFlyoutBackdrop(BindableObject obj) => (Brush)obj.GetValue(FlyoutBackdropProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='SetFlyoutBackdrop']/Docs/*" />
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
			BindableProperty bp = null;
			string textBinding;
			string iconBinding;
			var bindableObjectWithTemplate = GetBindableObjectWithFlyoutItemTemplate(bo);

			if (bo is IMenuItemController)
			{
				bp = MenuItemTemplateProperty;
				textBinding = "Text";
				iconBinding = "Icon";
			}
			else
			{
				bp = ItemTemplateProperty;
				textBinding = "Title";
				iconBinding = "FlyoutIcon";
			}

			if (bindableObjectWithTemplate.IsSet(bp))
			{
				return (DataTemplate)bindableObjectWithTemplate.GetValue(bp);
			}

			if (IsSet(bp))
			{
				return (DataTemplate)GetValue(bp);
			}

			return BaseShellItem.CreateDefaultFlyoutItemCell(textBinding, iconBinding);
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
				if (Application.Current == null)
					return null;

				foreach (var window in Application.Current.Windows)
					if (window is Window && window.IsActivated && window.Page is Shell shell)
						return shell;

				return Application.Current?.MainPage as Shell;
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

		/// <summary>Bindable property for <see cref="CurrentItem"/>.</summary>
		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(nameof(CurrentItem), typeof(ShellItem), typeof(Shell), null, BindingMode.TwoWay,
				propertyChanging: OnCurrentItemChanging,
				propertyChanged: OnCurrentItemChanged);

		/// <summary>Bindable property for <see cref="CurrentState"/>.</summary>
		public static readonly BindableProperty CurrentStateProperty = CurrentStatePropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="FlyoutBackgroundImage"/>.</summary>
		public static readonly BindableProperty FlyoutBackgroundImageProperty =
			BindableProperty.Create(nameof(FlyoutBackgroundImage), typeof(ImageSource), typeof(Shell), default(ImageSource), BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="FlyoutBackgroundImageAspect"/>.</summary>
		public static readonly BindableProperty FlyoutBackgroundImageAspectProperty =
			BindableProperty.Create(nameof(FlyoutBackgroundImageAspect), typeof(Aspect), typeof(Shell), default(Aspect), BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="FlyoutBackgroundColor"/>.</summary>
		public static readonly BindableProperty FlyoutBackgroundColorProperty =
			BindableProperty.Create(nameof(FlyoutBackgroundColor), typeof(Color), typeof(Shell), null, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="FlyoutBackground"/>.</summary>
		public static readonly BindableProperty FlyoutBackgroundProperty =
			BindableProperty.Create(nameof(FlyoutBackground), typeof(Brush), typeof(Shell), SolidColorBrush.Default, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="FlyoutHeaderBehavior"/>.</summary>
		public static readonly BindableProperty FlyoutHeaderBehaviorProperty =
			BindableProperty.Create(nameof(FlyoutHeaderBehavior), typeof(FlyoutHeaderBehavior), typeof(Shell), FlyoutHeaderBehavior.Default, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="FlyoutHeader"/>.</summary>
		public static readonly BindableProperty FlyoutHeaderProperty =
			BindableProperty.Create(nameof(FlyoutHeader), typeof(object), typeof(Shell), null, BindingMode.OneTime,
				propertyChanging: OnFlyoutHeaderChanging);

		/// <summary>Bindable property for <see cref="FlyoutFooter"/>.</summary>
		public static readonly BindableProperty FlyoutFooterProperty =
			BindableProperty.Create(nameof(FlyoutFooter), typeof(object), typeof(Shell), null, BindingMode.OneTime,
				propertyChanging: OnFlyoutFooterChanging);

		/// <summary>Bindable property for <see cref="FlyoutHeaderTemplate"/>.</summary>
		public static readonly BindableProperty FlyoutHeaderTemplateProperty =
			BindableProperty.Create(nameof(FlyoutHeaderTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime,
				propertyChanging: OnFlyoutHeaderTemplateChanging);

		/// <summary>Bindable property for <see cref="FlyoutFooterTemplate"/>.</summary>
		public static readonly BindableProperty FlyoutFooterTemplateProperty =
			BindableProperty.Create(nameof(FlyoutFooterTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime,
				propertyChanging: OnFlyoutFooterTemplateChanging);

		/// <summary>Bindable property for <see cref="FlyoutIsPresented"/>.</summary>
		public static readonly BindableProperty FlyoutIsPresentedProperty =
			BindableProperty.Create(nameof(FlyoutIsPresented), typeof(bool), typeof(Shell), false, BindingMode.TwoWay);

		/// <summary>Bindable property for <see cref="Items"/>.</summary>
		public static readonly BindableProperty ItemsProperty = ItemsPropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="FlyoutIcon"/>.</summary>
		public static readonly BindableProperty FlyoutIconProperty =
			BindableProperty.Create(nameof(FlyoutIcon), typeof(ImageSource), typeof(Shell), null);

		/// <summary>Bindable property for <see cref="FlyoutVerticalScrollMode"/>.</summary>
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
				Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;

			if (args.NewHandler != null && args.OldHandler == null)
				Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='FlyoutVerticalScrollMode']/Docs/*" />
		public ScrollMode FlyoutVerticalScrollMode
		{
			get => (ScrollMode)GetValue(FlyoutVerticalScrollModeProperty);
			set => SetValue(FlyoutVerticalScrollModeProperty, value);
		}

		public event EventHandler<ShellNavigatedEventArgs> Navigated;
		public event EventHandler<ShellNavigatingEventArgs> Navigating;


		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='FlyoutIcon']/Docs/*" />
		public ImageSource FlyoutIcon
		{
			get => (ImageSource)GetValue(FlyoutIconProperty);
			set => SetValue(FlyoutIconProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='CurrentItem']/Docs/*" />
		public ShellItem CurrentItem
		{
			get => (ShellItem)GetValue(CurrentItemProperty);
			set => SetValue(CurrentItemProperty, value);
		}

		internal ShellContent CurrentContent => CurrentItem?.CurrentItem?.CurrentItem;
		internal ShellSection CurrentSection => CurrentItem?.CurrentItem;

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='CurrentState']/Docs/*" />
		public ShellNavigationState CurrentState => (ShellNavigationState)GetValue(CurrentStateProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='FlyoutBackgroundImage']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource FlyoutBackgroundImage
		{
			get => (ImageSource)GetValue(FlyoutBackgroundImageProperty);
			set => SetValue(FlyoutBackgroundImageProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='FlyoutBackgroundImageAspect']/Docs/*" />
		public Aspect FlyoutBackgroundImageAspect
		{
			get => (Aspect)GetValue(FlyoutBackgroundImageAspectProperty);
			set => SetValue(FlyoutBackgroundImageAspectProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='FlyoutBackgroundColor']/Docs/*" />
		public Color FlyoutBackgroundColor
		{
			get => (Color)GetValue(FlyoutBackgroundColorProperty);
			set => SetValue(FlyoutBackgroundColorProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='FlyoutBackground']/Docs/*" />
		public Brush FlyoutBackground
		{
			get => (Brush)GetValue(FlyoutBackgroundProperty);
			set => SetValue(FlyoutBackgroundProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='FlyoutBackdrop']/Docs/*" />
		public Brush FlyoutBackdrop
		{
			get => (Brush)GetValue(FlyoutBackdropProperty);
			set => SetValue(FlyoutBackdropProperty, value);
		}

		public double FlyoutWidth
		{
			get => (double)GetValue(FlyoutWidthProperty);
			set => SetValue(FlyoutWidthProperty, value);
		}

		public double FlyoutHeight
		{
			get => (double)GetValue(FlyoutHeightProperty);
			set => SetValue(FlyoutHeightProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='FlyoutBehavior']/Docs/*" />
		public FlyoutBehavior FlyoutBehavior
		{
			get => (FlyoutBehavior)GetValue(FlyoutBehaviorProperty);
			set => SetValue(FlyoutBehaviorProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='FlyoutHeader']/Docs/*" />
		public object FlyoutHeader
		{
			get => GetValue(FlyoutHeaderProperty);
			set => SetValue(FlyoutHeaderProperty, value);
		}

		public object FlyoutFooter
		{
			get => GetValue(FlyoutFooterProperty);
			set => SetValue(FlyoutFooterProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='FlyoutHeaderBehavior']/Docs/*" />
		public FlyoutHeaderBehavior FlyoutHeaderBehavior
		{
			get => (FlyoutHeaderBehavior)GetValue(FlyoutHeaderBehaviorProperty);
			set => SetValue(FlyoutHeaderBehaviorProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='FlyoutHeaderTemplate']/Docs/*" />
		public DataTemplate FlyoutHeaderTemplate
		{
			get => (DataTemplate)GetValue(FlyoutHeaderTemplateProperty);
			set => SetValue(FlyoutHeaderTemplateProperty, value);
		}

		public DataTemplate FlyoutFooterTemplate
		{
			get => (DataTemplate)GetValue(FlyoutFooterTemplateProperty);
			set => SetValue(FlyoutFooterTemplateProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='FlyoutIsPresented']/Docs/*" />
		public bool FlyoutIsPresented
		{
			get => (bool)GetValue(FlyoutIsPresentedProperty);
			set => SetValue(FlyoutIsPresentedProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='Items']/Docs/*" />
		public IList<ShellItem> Items => (IList<ShellItem>)GetValue(ItemsProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='ItemTemplate']/Docs/*" />
		public DataTemplate ItemTemplate
		{
			get => GetItemTemplate(this);
			set => SetItemTemplate(this, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Shell.xml" path="//Member[@MemberName='MenuItemTemplate']/Docs/*" />
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

			_previousPage?.SendNavigatedFrom(new NavigatedFromEventArgs(CurrentPage));
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
				oldShellItem.SendDisappearing();

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


		/// <summary>Bindable property for <see cref="FlyoutContent"/>.</summary>

		public static readonly BindableProperty FlyoutContentProperty =
			BindableProperty.Create(nameof(FlyoutContent), typeof(object), typeof(Shell), null, BindingMode.OneTime, propertyChanging: OnFlyoutContentChanging);

		/// <summary>Bindable property for <see cref="FlyoutContentTemplate"/>.</summary>
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
