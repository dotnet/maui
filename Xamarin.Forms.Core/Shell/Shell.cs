using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty(nameof(Items))]
	public class Shell : Page, IShellController, IPropertyPropagationController
	{
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

		public static readonly BindableProperty FlyoutBehaviorProperty =
			BindableProperty.CreateAttached("FlyoutBehavior", typeof(FlyoutBehavior), typeof(Shell), FlyoutBehavior.Flyout,
				propertyChanged: OnFlyoutBehaviorChanged);

		public static readonly BindableProperty NavBarIsVisibleProperty =
			BindableProperty.CreateAttached("NavBarIsVisible", typeof(bool), typeof(Shell), true);

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

		public static readonly BindableProperty SetPaddingInsetsProperty =
			BindableProperty.CreateAttached("SetPaddingInsets", typeof(bool), typeof(Shell), false);

		public static readonly BindableProperty TabBarIsVisibleProperty =
			BindableProperty.CreateAttached("TabBarIsVisible", typeof(bool), typeof(Shell), true);

		public static readonly BindableProperty TitleViewProperty =
			BindableProperty.CreateAttached("TitleView", typeof(View), typeof(Shell), null, propertyChanged: OnTitleViewChanged);

		public static BackButtonBehavior GetBackButtonBehavior(BindableObject obj) => (BackButtonBehavior)obj.GetValue(BackButtonBehaviorProperty);
		public static void SetBackButtonBehavior(BindableObject obj, BackButtonBehavior behavior) => obj.SetValue(BackButtonBehaviorProperty, behavior);

		public static FlyoutBehavior GetFlyoutBehavior(BindableObject obj) => (FlyoutBehavior)obj.GetValue(FlyoutBehaviorProperty);
		public static void SetFlyoutBehavior(BindableObject obj, FlyoutBehavior value) => obj.SetValue(FlyoutBehaviorProperty, value);

		public static bool GetNavBarIsVisible(BindableObject obj) => (bool)obj.GetValue(NavBarIsVisibleProperty);
		public static void SetNavBarIsVisible(BindableObject obj, bool value) => obj.SetValue(NavBarIsVisibleProperty, value);

		public static SearchHandler GetSearchHandler(BindableObject obj) => (SearchHandler)obj.GetValue(SearchHandlerProperty);
		public static void SetSearchHandler(BindableObject obj, SearchHandler handler) => obj.SetValue(SearchHandlerProperty, handler);

		public static bool GetSetPaddingInsets(BindableObject obj) => (bool)obj.GetValue(SetPaddingInsetsProperty);
		public static void SetSetPaddingInsets(BindableObject obj, bool value) => obj.SetValue(SetPaddingInsetsProperty, value);

		public static bool GetTabBarIsVisible(BindableObject obj) => (bool)obj.GetValue(TabBarIsVisibleProperty);
		public static void SetTabBarIsVisible(BindableObject obj, bool value) => obj.SetValue(TabBarIsVisibleProperty, value);

		public static View GetTitleView(BindableObject obj) => (View)obj.GetValue(TitleViewProperty);
		public static void SetTitleView(BindableObject obj, View value) => obj.SetValue(TitleViewProperty, value);

		static void OnFlyoutBehaviorChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var element = (Element)bindable;

			while (!Application.IsApplicationOrNull(element)) {
				if (element is Shell shell)
					shell.NotifyFlyoutBehaviorObservers();
				element = element.Parent;
			}
		}
		
		public static readonly BindableProperty ShellBackgroundColorProperty =
			BindableProperty.CreateAttached("ShellBackgroundColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellDisabledColorProperty =
			BindableProperty.CreateAttached("ShellDisabledColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellForegroundColorProperty =
			BindableProperty.CreateAttached("ShellForegroundColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellTabBarBackgroundColorProperty =
			BindableProperty.CreateAttached("ShellTabBarBackgroundColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellTabBarDisabledColorProperty =
			BindableProperty.CreateAttached("ShellTabBarDisabledColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellTabBarForegroundColorProperty =
			BindableProperty.CreateAttached("ShellTabBarForegroundColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellTabBarTitleColorProperty =
			BindableProperty.CreateAttached("ShellTabBarTitleColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellTabBarUnselectedColorProperty =
			BindableProperty.CreateAttached("ShellTabBarUnselectedColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellTitleColorProperty =
			BindableProperty.CreateAttached("ShellTitleColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellUnselectedColorProperty =
			BindableProperty.CreateAttached("ShellUnselectedColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static Color GetShellBackgroundColor(BindableObject obj) => (Color)obj.GetValue(ShellBackgroundColorProperty);
		public static void SetShellBackgroundColor(BindableObject obj, Color value) => obj.SetValue(ShellBackgroundColorProperty, value);

		public static Color GetShellDisabledColor(BindableObject obj) => (Color)obj.GetValue(ShellDisabledColorProperty);
		public static void SetShellDisabledColor(BindableObject obj, Color value) => obj.SetValue(ShellDisabledColorProperty, value);

		public static Color GetShellForegroundColor(BindableObject obj) => (Color)obj.GetValue(ShellForegroundColorProperty);
		public static void SetShellForegroundColor(BindableObject obj, Color value) => obj.SetValue(ShellForegroundColorProperty, value);

		public static Color GetShellTabBarBackgroundColor(BindableObject obj) => (Color)obj.GetValue(ShellTabBarBackgroundColorProperty);
		public static void SetShellTabBarBackgroundColor(BindableObject obj, Color value) => obj.SetValue(ShellTabBarBackgroundColorProperty, value);

		public static Color GetShellTabBarDisabledColor(BindableObject obj) => (Color)obj.GetValue(ShellTabBarDisabledColorProperty);
		public static void SetShellTabBarDisabledColor(BindableObject obj, Color value) => obj.SetValue(ShellTabBarDisabledColorProperty, value);

		public static Color GetShellTabBarForegroundColor(BindableObject obj) => (Color)obj.GetValue(ShellTabBarForegroundColorProperty);
		public static void SetShellTabBarForegroundColor(BindableObject obj, Color value) => obj.SetValue(ShellTabBarForegroundColorProperty, value);

		public static Color GetShellTabBarTitleColor(BindableObject obj) => (Color)obj.GetValue(ShellTabBarTitleColorProperty);
		public static void SetShellTabBarTitleColor(BindableObject obj, Color value) => obj.SetValue(ShellTabBarTitleColorProperty, value);

		public static Color GetShellTabBarUnselectedColor(BindableObject obj) => (Color)obj.GetValue(ShellTabBarUnselectedColorProperty);
		public static void SetShellTabBarUnselectedColor(BindableObject obj, Color value) => obj.SetValue(ShellTabBarUnselectedColorProperty, value);

		public static Color GetShellTitleColor(BindableObject obj) => (Color)obj.GetValue(ShellTitleColorProperty);
		public static void SetShellTitleColor(BindableObject obj, Color value) => obj.SetValue(ShellTitleColorProperty, value);

		public static Color GetShellUnselectedColor(BindableObject obj) => (Color)obj.GetValue(ShellUnselectedColorProperty);
		public static void SetShellUnselectedColor(BindableObject obj, Color value) => obj.SetValue(ShellUnselectedColorProperty, value);

		static void OnShellColorValueChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var item = (Element)bindable;
			var source = item;

			while (!Application.IsApplicationOrNull(item))
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

		static readonly BindablePropertyKey MenuItemsPropertyKey =
			BindableProperty.CreateReadOnly(nameof(MenuItems), typeof(MenuItemCollection), typeof(Shell), null, defaultValueCreator: bo => new MenuItemCollection());

		List<(IAppearanceObserver Observer, Element Pivot)> _appearanceObservers = new List<(IAppearanceObserver Observer, Element Pivot)>();
		List<IFlyoutBehaviorObserver> _flyoutBehaviorObservers = new List<IFlyoutBehaviorObserver>();

		event EventHandler IShellController.HeaderChanged
		{
			add { _headerChanged += value; }
			remove { _headerChanged -= value; }
		}

		event EventHandler IShellController.StructureChanged
		{
			add { _structureChanged += value; }
			remove { _structureChanged -= value; }
		}

		event EventHandler _headerChanged;
		event EventHandler _structureChanged;

		View IShellController.FlyoutHeader => FlyoutHeaderView;

		void IShellController.AddAppearanceObserver(IAppearanceObserver observer, Element pivot)
		{
			_appearanceObservers.Add((observer, pivot));
			observer.OnAppearanceChanged(GetShellAppearanceForPivot(pivot));
		}

		void IShellController.AddFlyoutBehaviorObserver(IFlyoutBehaviorObserver observer)
		{
			_flyoutBehaviorObservers.Add(observer);
			observer.OnFlyoutBehaviorChanged(GetEffectiveFlyoutBehavior());
		}

		void IShellController.AppearanceChanged(Element source, bool appearanceSet)
		{
			if (!appearanceSet)
			{
				// This bubbles up whenever there is an kind of structure/page change
				// So its also quite useful for checking the FlyoutBehavior conditions
				NotifyFlyoutBehaviorObservers();
			}

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

				while (!Application.IsApplicationOrNull(leaf))
				{
					if (leaf == target)
					{
						observer.OnAppearanceChanged(GetShellAppearanceForPivot(pivot));
						break;
					}
					leaf = leaf.Parent;
				}
			}
		}

		ShellNavigationState IShellController.GetNavigationState(ShellItem shellItem, ShellSection shellSection, ShellContent shellContent, bool includeStack)
			=> GetNavigationState(shellItem, shellSection, shellContent, includeStack ? shellSection.Stack.ToList() : null);

		async void IShellController.OnFlyoutItemSelected(Element element)
		{
			ShellItem shellItem = null;
			ShellSection shellSection = null;
			ShellContent shellContent = null;

			switch (element) {
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
				return;
				
			shellSection = shellSection ?? shellItem.CurrentItem;
			shellContent = shellContent ?? shellSection?.CurrentItem;

			var state = GetNavigationState(shellItem, shellSection, shellContent, null);

			if (FlyoutIsPresented && FlyoutBehavior == FlyoutBehavior.Flyout)
				SetValueFromRenderer(FlyoutIsPresentedProperty, false);

			await GoToAsync(state).ConfigureAwait(false);
		}

		bool IShellController.ProposeNavigation(ShellNavigationSource source, ShellItem shellItem, ShellSection shellSection, ShellContent shellContent, IReadOnlyList<Page> stack, bool canCancel)
		{
			var proposedState = GetNavigationState(shellItem, shellSection, shellContent, stack);
			return ProposeNavigation(source, proposedState, canCancel);
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
			var result = GetNavigationState(shellItem, shellSection, shellContent, stack);

			SetValueFromRenderer(CurrentStatePropertyKey, result);

			OnNavigated(new ShellNavigatedEventArgs(oldState, CurrentState, source));
		}

		public static Shell Current => Application.Current?.MainPage as Shell;

		Uri GetAbsoluteUri(Uri relativeUri)
		{
			if (CurrentItem == null)
				throw new InvalidOperationException("Relative path is used after selecting Current item.");

			var parseUri = Regex.Match(relativeUri.OriginalString, @"(?<u>.+?)(\?(?<q>.+?))?(#(?<f>.+))?$").Groups;
			var url = parseUri["u"].Value;
			var query = parseUri["q"].Value;
			var fragment = parseUri["f"].Value;

			Element item = CurrentItem;
			var list = new List<string>();
			while (item != null && !(item is IApplicationController))
			{
				var route = Routing.GetRoute(item)?.Trim('/');
				if (string.IsNullOrEmpty(route))
					break;
				list.Insert(0, route);
				item = item.Parent;
			}

			var isGlobalRegisteredRoute = Routing.CompareWithRegisteredRoutes(url);
			if (isGlobalRegisteredRoute)
				list.RemoveRange(1, list.Count - 1);

			list.Add(url.Trim('/'));

			var parentUriBuilder = new UriBuilder(RouteScheme)
			{
				Path = string.Join("/", list),
				Query = query,
				Fragment = fragment
			};
			return parentUriBuilder.Uri;
		}

		public async Task GoToAsync(ShellNavigationState state, bool animate = true)
		{
			// FIXME: This should not be none, we need to compute the delta and set flags correctly
			var accept = ProposeNavigation(ShellNavigationSource.Unknown, state, true);
			if (!accept)
				return;

			_accumulateNavigatedEvents = true;

			var uri = state.Location.IsAbsoluteUri ? state.Location : GetAbsoluteUri(state.Location);

			var queryString = uri.Query;
			var queryData = ParseQueryString(queryString);
			var path = uri.AbsolutePath;

			path = path.TrimEnd('/');

			var parts = path.Substring(1).Split('/').ToList();

			if (parts.Count < 2)
				throw new InvalidOperationException("Path must be at least 2 items long in Shell navigation");

			var shellRoute = parts[0];

			var expectedShellRoute = Routing.GetRoute(this) ?? string.Empty;
			if (expectedShellRoute != shellRoute)
				throw new NotImplementedException();
			else
				parts.RemoveAt(0);

			var shellItemRoute = parts[0];
			ApplyQueryAttributes(this, queryData, false);

			var items = Items;
			for (int i = 0; i < items.Count; i++)
			{
				var shellItem = items[i];
				if (Routing.CompareRoutes(shellItem.Route, shellItemRoute, out var isImplicit))
				{
					ApplyQueryAttributes(shellItem, queryData, parts.Count == 1);

					if (CurrentItem != shellItem)
						SetValueFromRenderer(CurrentItemProperty, shellItem);

					if (!isImplicit)
						parts.RemoveAt(0);

					if (parts.Count > 0)
						await ((IShellItemController)shellItem).GoToPart(parts, queryData);

					break;
				}
			}

			if (Routing.CompareWithRegisteredRoutes(shellItemRoute))
			{
				var shellItem = ShellItem.GetShellItemFromRouteName(shellItemRoute);

				ApplyQueryAttributes(shellItem, queryData, parts.Count == 1);

				if (CurrentItem != shellItem)
					SetValueFromRenderer(CurrentItemProperty, shellItem);

				if (parts.Count > 0)
					await ((IShellItemController)shellItem).GoToPart(parts, queryData);
			}
			_accumulateNavigatedEvents = false;

			// this can be null in the event that no navigation actually took place!
			if (_accumulatedEvent != null)
				OnNavigated(_accumulatedEvent);
		}

		internal static void ApplyQueryAttributes(Element element, IDictionary<string, string> query, bool isLastItem)
		{
			if (query.Count == 0)
				return;

			string prefix = "";
			if (!isLastItem)
			{
				var route = Routing.GetRoute(element);
				if (string.IsNullOrEmpty(route) || route.StartsWith(Routing.ImplicitPrefix, StringComparison.Ordinal))
					return;
				prefix = route + ".";
			}

			//if the lastItem is implicitly wrapped, get the actual ShellContent
			if (isLastItem) {
				if (element is ShellItem shellitem && shellitem.Items.FirstOrDefault() is ShellSection section)
					element = section;
				if (element is ShellSection shellsection && shellsection.Items.FirstOrDefault() is ShellContent content)
					element = content;
				if (element is ShellContent shellcontent && shellcontent.Content is Element e)
					element = e;
			}

			if (!(element is BaseShellItem baseShellItem))
				baseShellItem = element?.Parent as BaseShellItem;

			//filter the query to only apply the keys with matching prefix
			var filteredQuery = new Dictionary<string, string>(query.Count);
			foreach (var q in query) {
				if (!q.Key.StartsWith(prefix, StringComparison.Ordinal))
					continue;
				var key = q.Key.Substring(prefix.Length);
				if (key.Contains("."))
					continue;
				filteredQuery.Add(key, q.Value);
			}

			if (baseShellItem != null)
				baseShellItem.ApplyQueryAttributes(filteredQuery);
			else if (isLastItem)
				ShellContent.ApplyQueryAttributes(element, query);
		}

		ShellNavigationState GetNavigationState(ShellItem shellItem, ShellSection shellSection, ShellContent shellContent, IReadOnlyList<Page> sectionStack)
		{
			StringBuilder stateBuilder = new StringBuilder($"{RouteScheme}://{RouteHost}/{Route}/");
			Dictionary<string, string> queryData = new Dictionary<string, string>();

			bool stackAtRoot = sectionStack == null || sectionStack.Count <= 1;

			if (shellItem != null)
			{
				var shellItemRoute = shellItem.Route;
				if (!shellItemRoute.StartsWith(Routing.ImplicitPrefix, StringComparison.Ordinal))
				{
					stateBuilder.Append(shellItemRoute);
					stateBuilder.Append("/");
				}

				if (shellSection != null)
				{
					var shellSectionRoute = shellSection.Route;
					if (!shellSectionRoute.StartsWith(Routing.ImplicitPrefix, StringComparison.Ordinal))
					{
						stateBuilder.Append(shellSectionRoute);
						stateBuilder.Append("/");
					}

					if (shellContent != null)
					{
						var shellContentRoute = shellContent.Route;
						if (!shellContentRoute.StartsWith(Routing.ImplicitPrefix, StringComparison.Ordinal))
						{
							stateBuilder.Append(shellContentRoute);
							stateBuilder.Append("/");
						}
					}

					if (!stackAtRoot)
					{
						for (int i = 1; i < sectionStack.Count; i++)
						{
							var page = sectionStack[i];
							stateBuilder.Append(Routing.GetRoute(page));
							if (i < sectionStack.Count - 1)
								stateBuilder.Append("/");
						}
					}
				}
			}

			return stateBuilder.ToString();
		}

		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(nameof(CurrentItem), typeof(ShellItem), typeof(Shell), null, BindingMode.TwoWay,
				propertyChanging: OnCurrentItemChanging,
				propertyChanged: OnCurrentItemChanged);

		public static readonly BindableProperty CurrentStateProperty = CurrentStatePropertyKey.BindableProperty;

		public static readonly BindableProperty FlyoutBackgroundColorProperty =
			BindableProperty.Create(nameof(FlyoutBackgroundColor), typeof(Color), typeof(Shell), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty FlyoutHeaderBehaviorProperty =
			BindableProperty.Create(nameof(FlyoutHeaderBehavior), typeof(FlyoutHeaderBehavior), typeof(Shell), FlyoutHeaderBehavior.Default, BindingMode.OneTime);

		public static readonly BindableProperty FlyoutHeaderProperty =
			BindableProperty.Create(nameof(FlyoutHeader), typeof(object), typeof(Shell), null, BindingMode.OneTime,
				propertyChanged: OnFlyoutHeaderChanged);

		public static readonly BindableProperty FlyoutHeaderTemplateProperty =
			BindableProperty.Create(nameof(FlyoutHeaderTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime,
				propertyChanged: OnFlyoutHeaderTemplateChanged);

		public static readonly BindableProperty FlyoutIsPresentedProperty =
			BindableProperty.Create(nameof(FlyoutIsPresented), typeof(bool), typeof(Shell), false, BindingMode.TwoWay);

		public static readonly BindableProperty GroupHeaderTemplateProperty =
			BindableProperty.Create(nameof(GroupHeaderTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty ItemsProperty = ItemsPropertyKey.BindableProperty;

		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty MenuItemsProperty = MenuItemsPropertyKey.BindableProperty;

		public static readonly BindableProperty MenuItemTemplateProperty =
			BindableProperty.Create(nameof(MenuItemTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty FlyoutIconProperty =
			BindableProperty.Create(nameof(FlyoutIcon), typeof(ImageSource), typeof(Shell), null);

		ShellNavigatedEventArgs _accumulatedEvent;
		bool _accumulateNavigatedEvents;
		View _flyoutHeaderView;
		bool _checkExperimentalFlag = true;

		public Shell() : this(true)
		{
		}

		internal Shell(bool checkFlag)
		{
			_checkExperimentalFlag = checkFlag;
			VerifyShellFlagEnabled(constructorHint: nameof(Shell));
			((INotifyCollectionChanged)Items).CollectionChanged += (s, e) => SendStructureChanged();
		}

		internal const string ShellExperimental = ExperimentalFlags.ShellExperimental;

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal void VerifyShellFlagEnabled(string constructorHint = null, [CallerMemberName] string memberName = "")
		{
			if(_checkExperimentalFlag)
				ExperimentalFlags.VerifyFlagEnabled("Shell", ShellExperimental, constructorHint, memberName);
		}

		public event EventHandler<ShellNavigatedEventArgs> Navigated;
		public event EventHandler<ShellNavigatingEventArgs> Navigating;


		public ImageSource FlyoutIcon
		{
			get => (ImageSource)GetValue(FlyoutIconProperty);
			set => SetValue(FlyoutIconProperty, value);
		}

		public ShellItem CurrentItem {
			get => (ShellItem)GetValue(CurrentItemProperty);
			set => SetValue(CurrentItemProperty, value);
		}

		public ShellNavigationState CurrentState => (ShellNavigationState)GetValue(CurrentStateProperty);

		public Color FlyoutBackgroundColor {
			get => (Color)GetValue(FlyoutBackgroundColorProperty);
			set => SetValue(FlyoutBackgroundColorProperty, value);
		}

		public FlyoutBehavior FlyoutBehavior {
			get => (FlyoutBehavior)GetValue(FlyoutBehaviorProperty);
			set => SetValue(FlyoutBehaviorProperty, value);
		}

		public object FlyoutHeader {
			get => GetValue(FlyoutHeaderProperty);
			set => SetValue(FlyoutHeaderProperty, value);
		}

		public FlyoutHeaderBehavior FlyoutHeaderBehavior {
			get => (FlyoutHeaderBehavior)GetValue(FlyoutHeaderBehaviorProperty);
			set => SetValue(FlyoutHeaderBehaviorProperty, value);
		}

		public DataTemplate FlyoutHeaderTemplate {
			get => (DataTemplate)GetValue(FlyoutHeaderTemplateProperty);
			set => SetValue(FlyoutHeaderTemplateProperty, value);
		}

		public bool FlyoutIsPresented {
			get => (bool)GetValue(FlyoutIsPresentedProperty);
			set => SetValue(FlyoutIsPresentedProperty, value);
		}

		public DataTemplate GroupHeaderTemplate {
			get => (DataTemplate)GetValue(GroupHeaderTemplateProperty);
			set => SetValue(GroupHeaderTemplateProperty, value);
		}

		public ShellItemCollection Items => (ShellItemCollection)GetValue(ItemsProperty);
		public ShellItemCollection Flyout => Items;

		public DataTemplate ItemTemplate {
			get => (DataTemplate)GetValue(ItemTemplateProperty);
			set => SetValue(ItemTemplateProperty, value);
		}

		public MenuItemCollection MenuItems => (MenuItemCollection)GetValue(MenuItemsProperty);

		public DataTemplate MenuItemTemplate {
			get => (DataTemplate)GetValue(MenuItemTemplateProperty);
			set => SetValue(MenuItemTemplateProperty, value);
		}

		public string Route {
			get => Routing.GetRoute(this);
			set => Routing.SetRoute(this, value);
		}

		public string RouteHost { get; set; }

		public string RouteScheme { get; set; } = "app";

		View FlyoutHeaderView {
			get => _flyoutHeaderView;
			set {
				if (_flyoutHeaderView == value)
					return;

				if (_flyoutHeaderView != null)
					OnChildRemoved(_flyoutHeaderView);
				_flyoutHeaderView = value;
				if (_flyoutHeaderView != null)
					OnChildAdded(_flyoutHeaderView);
				_headerChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		List<List<Element>> IShellController.GenerateFlyoutGrouping()
		{
			// The idea here is to create grouping such that the Flyout would
			// render correctly if it renderered each item in the groups in order
			// but put a line between the groups. This is needed because our grouping can
			// actually go 3 layers deep.

			// Ideally this lets us control where lines are drawn in the core code
			// just by changing how we generate these groupings

			var result = new List<List<Element>>();

			var currentGroup = new List<Element>();
			result.Add(currentGroup);

			void IncrementGroup()
			{
				if (currentGroup.Count > 0)
				{
					currentGroup = new List<Element>();
					result.Add(currentGroup);
				}
			}

			foreach (var shellItem in Items)
			{
				if (shellItem.FlyoutDisplayOptions == FlyoutDisplayOptions.AsMultipleItems)
				{
					IncrementGroup();

					foreach (var shellSection in shellItem.Items)
					{
						if (shellSection.FlyoutDisplayOptions == FlyoutDisplayOptions.AsMultipleItems)
						{
							IncrementGroup();

							foreach (var shellContent in shellSection.Items)
							{
								currentGroup.Add(shellContent);
								if (shellContent == shellSection.CurrentItem)
								{
									currentGroup.AddRange(shellContent.MenuItems);
								}
							}
							IncrementGroup();
						}
						else
						{
							currentGroup.Add(shellSection);

							// If we have only a single child we will also show the items menu items
							if (shellSection.Items.Count == 1 && shellSection == shellItem.CurrentItem)
							{
								currentGroup.AddRange(shellSection.CurrentItem.MenuItems);
							}
						}
					}
					IncrementGroup();
				}
				else
				{
					currentGroup.Add(shellItem);
				}
			}

			IncrementGroup();

			currentGroup.AddRange(MenuItems);

			return result;
		}

		internal void SendStructureChanged()
		{
			UpdateChecked(this);
			_structureChanged?.Invoke(this, EventArgs.Empty);
		}

		protected override bool OnBackButtonPressed()
		{
			var currentContent = CurrentItem?.CurrentItem;
			if (currentContent != null && currentContent.Stack.Count > 1)
			{
				currentContent.Navigation.PopAsync();
				return true;
			}
			return false;
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);

			if (child is ShellItem shellItem && CurrentItem == null && !(child is MenuShellItem))
			{
				((IShellController)this).OnFlyoutItemSelected(shellItem);
			}
		}

		protected override void OnChildRemoved(Element child)
		{
			base.OnChildRemoved(child);

			if (child == CurrentItem && Items.Count > 0)
			{
				((IShellController)this).OnFlyoutItemSelected(Items[0]);
			}
		}

		protected virtual void OnNavigated(ShellNavigatedEventArgs args)
		{
			if (_accumulateNavigatedEvents)
				_accumulatedEvent = args;
			else {
				/* Removing this check for now as it doesn't properly cover all implicit scenarios
				 * if (args.Current.Location.AbsolutePath.TrimEnd('/') != _lastNavigating.Location.AbsolutePath.TrimEnd('/'))
					throw new InvalidOperationException($"Navigation: Current location doesn't match navigation uri {args.Current.Location.AbsolutePath} != {_lastNavigating.Location.AbsolutePath}");
					*/
				Navigated?.Invoke(this, args);
				//System.Diagnostics.Debug.WriteLine("Navigated: " + args.Current.Location);
			}
		}

		ShellNavigationState _lastNavigating;
		protected virtual void OnNavigating(ShellNavigatingEventArgs args)
		{
			Navigating?.Invoke(this, args);
			_lastNavigating = args.Target;
		}

		static void OnCurrentItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			UpdateChecked(shell);

			((IShellController)shell).AppearanceChanged(shell, false);
			((IShellController)shell).UpdateCurrentState(ShellNavigationSource.ShellItemChanged);
		}

		static void OnCurrentItemChanging(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			if (!shell._accumulateNavigatedEvents)
			{
				// We are not in the middle of a GoToAsync so this is a user requested change.
				// We need to emit the Navigating event since GoToAsync wont be emitting it.

				var shellItem = (ShellItem)newValue;
				var shellSection = shellItem.CurrentItem;
				var shellContent = shellSection.CurrentItem;
				var stack = shellSection.Stack;
				((IShellController)shell).ProposeNavigation(ShellNavigationSource.ShellItemChanged, shellItem, shellSection, shellContent, stack, false);
			}
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
				var items = shell.Items;
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
				var items = shellItem.Items;
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
				var items = shellSection.Items;
				var count = items.Count;
				for (int i = 0; i < count; i++)
				{
					ShellContent item = items[i];
					UpdateChecked(item, isChecked && item == currentItem);
				}
			}
		}

		static void OnFlyoutHeaderChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			shell.OnFlyoutHeaderChanged(oldValue, newValue);
		}

		static void OnFlyoutHeaderTemplateChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			shell.OnFlyoutHeaderTemplateChanged((DataTemplate)oldValue, (DataTemplate)newValue);
		}

		static void OnTitleViewChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!(bindable is Element owner))
				return;

			var oldView = (View)oldValue;
			var newView = (View)newValue;

			if (oldView != null)
				oldView.Parent = null;

			if (newView != null)
				newView.Parent = owner;
		}

		static Dictionary<string, string> ParseQueryString(string query)
		{
			if (query.StartsWith("?", StringComparison.Ordinal))
				query = query.Substring(1);
			Dictionary<string, string> lookupDict = new Dictionary<string, string>();
			if (query == null)
				return lookupDict;
			foreach (var part in query.Split('&'))
			{
				var p = part.Split('=');
				if (p.Length != 2)
					continue;
				lookupDict[p[0]] = p[1];
			}

			return lookupDict;
		}

		FlyoutBehavior GetEffectiveFlyoutBehavior()
		{
			var page = WalkToPage(this);

			while (page != this && page != null)
			{
				if (page.IsSet(FlyoutBehaviorProperty))
					return GetFlyoutBehavior(page);
				page = page.Parent;
			}
			return FlyoutBehavior;
		}

		ShellAppearance GetShellAppearanceForPivot(Element pivot)
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
			while (!Application.IsApplicationOrNull(pivot))
			{
				if (pivot is ShellContent)
					foundShellContent = true;

				// One minor deviation here. Even though a pushed page is technically the child of
				// a ShellSection and not the ShellContent, we want the root ShellContent to 
				// be taken into account. Yes this could behavior oddly if the developer switches
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
			if (CurrentItem == null)
				return;

			var behavior = GetEffectiveFlyoutBehavior();
			for (int i = 0; i < _flyoutBehaviorObservers.Count; i++)
				_flyoutBehaviorObservers[i].OnFlyoutBehaviorChanged(behavior);
		}

		void OnFlyoutHeaderChanged(object oldVal, object newVal)
		{
			if (FlyoutHeaderTemplate == null)
			{
				if (newVal is View newFlyoutHeader)
					FlyoutHeaderView = newFlyoutHeader;
				else
					FlyoutHeaderView = null;
			}
			else
			{
				FlyoutHeaderView.BindingContext = newVal;
			}
		}

		void OnFlyoutHeaderTemplateChanged(DataTemplate oldValue, DataTemplate newValue)
		{
			if (newValue == null)
			{
				if (FlyoutHeader is View flyoutHeaderView)
					FlyoutHeaderView = flyoutHeaderView;
				else
					FlyoutHeaderView = null;
			}
			else
			{
				var newHeaderView = (View)newValue.CreateContent(FlyoutHeader, this);
				newHeaderView.BindingContext = FlyoutHeader;
				FlyoutHeaderView = newHeaderView;
			}
		}

		bool ProposeNavigation(ShellNavigationSource source, ShellNavigationState proposedState, bool canCancel)
		{
			if (_accumulateNavigatedEvents)
				return true;

			var navArgs = new ShellNavigatingEventArgs(CurrentState, proposedState, source, canCancel);

			OnNavigating(navArgs);
			//System.Diagnostics.Debug.WriteLine("Proposed: " + proposedState.Location);
			return !navArgs.Cancelled;
		}

		Element WalkToPage(Element element)
		{
			switch (element) {
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
			PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, LogicalChildren);
			if (FlyoutHeaderView != null)
				PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, new[] { FlyoutHeaderView });
		}
	}
}
