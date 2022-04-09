using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Navigation;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using ADrawableCompat = AndroidX.Core.Graphics.Drawable.DrawableCompat;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Handlers
{
	internal class TabbedPageManager
	{
		Fragment _tabLayoutFragment;
		ColorStateList _originalTabTextColors;
		ColorStateList _orignalTabIconColors;
		ColorStateList _newTabTextColors;
		ColorStateList _newTabIconColors;
		FragmentManager _fragmentManager;
		TabLayout _tabLayout;
		BottomNavigationView _bottomNavigationView;
		ViewPager2 _viewPager;
		Page _previousPage;
		int[] _checkedStateSet = null;
		int[] _selectedStateSet = null;
		int[] _emptyStateSet = null;
		int _defaultARGBColor = Colors.Transparent.ToPlatform().ToArgb();
		AColor _defaultAndroidColor = Colors.Transparent.ToPlatform();
		readonly IMauiContext _context;
		readonly Listeners _listeners;
		TabbedPage Element { get; set; }
		internal TabLayout TabLayout => _tabLayout;
		internal BottomNavigationView BottomNavigationView => _bottomNavigationView;
		internal ViewPager2 ViewPager => _viewPager;
		int _tabplacementId;
		Brush _currentBarBackground;
		Color _currentBarItemColor;
		Color _currentBarTextColor;
		Color _currentBarSelectedItemColor;
		ColorStateList _currentBarTextColorStateList;
		bool _tabItemStyleLoaded;

		NavigationRootManager NavigationRootManager { get; }

		public TabbedPageManager(IMauiContext context)
		{
			_context = context;
			NavigationRootManager = _context.GetNavigationRootManager();
			_listeners = new Listeners(this);
			_viewPager = new ViewPager2(context.Context)
			{
				OverScrollMode = OverScrollMode.Never,
				LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
			};
		}

		FragmentManager FragmentManager => _fragmentManager ?? (_fragmentManager = _context.GetFragmentManager());
		public bool IsBottomTabPlacement => (Element != null) ? Element.OnThisPlatform().GetToolbarPlacement() == ToolbarPlacement.Bottom : false;

		public Color BarItemColor
		{
			get
			{
				if (Element != null)
				{
					if (Element.IsSet(TabbedPage.UnselectedTabColorProperty))
						return Element.UnselectedTabColor;
				}

				return null;
			}
		}

		public Color BarSelectedItemColor
		{
			get
			{
				if (Element != null)
				{
					if (Element.IsSet(TabbedPage.SelectedTabColorProperty))
						return Element.SelectedTabColor;
				}

				return null;
			}
		}

		internal void SetElement(TabbedPage tabbedPage)
		{
			var activity = _context.GetActivity();
			var themeContext = activity;

			if (Element != null)
			{
				((IPageController)Element).InternalChildren.CollectionChanged -= OnChildrenCollectionChanged;
				Element.Appearing -= OnTabbedPageAppearing;
				Element.Disappearing -= OnTabbedPageDisappearing;
				RemoveTabs();
				_viewPager.LayoutChange -= OnLayoutChanged;
			}

			Element = tabbedPage;
			if (Element != null)
			{
				_viewPager.LayoutChange += OnLayoutChanged;
				Element.Appearing += OnTabbedPageAppearing;
				Element.Disappearing += OnTabbedPageDisappearing;
				_viewPager.Adapter = new MultiPageFragmentStateAdapter<Page>(tabbedPage, FragmentManager, _context) { CountOverride = tabbedPage.Children.Count };

				if (IsBottomTabPlacement)
				{
					_bottomNavigationView = new BottomNavigationView(_context.Context)
					{
						LayoutParameters = new CoordinatorLayout.LayoutParams(AppBarLayout.LayoutParams.MatchParent, AppBarLayout.LayoutParams.WrapContent)
						{
							Gravity = (int)GravityFlags.Bottom
						}
					};
				}
				else
				{
					if (_tabLayout == null)
					{
						var layoutInflater = Element.Handler.MauiContext.GetLayoutInflater();
						_tabLayout = new TabLayout(_context.Context)
						{
							TabMode = TabLayout.ModeFixed,
							TabGravity = TabLayout.GravityFill,
							LayoutParameters = new AppBarLayout.LayoutParams(AppBarLayout.LayoutParams.MatchParent, AppBarLayout.LayoutParams.WrapContent)
						};
					}
				}

				OnChildrenCollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

				ScrollToCurrentPage();

				_previousPage = tabbedPage.CurrentPage;

				((IPageController)tabbedPage).InternalChildren.CollectionChanged += OnChildrenCollectionChanged;

				SetTabLayout();
			}
		}

		void OnLayoutChanged(object sender, AView.LayoutChangeEventArgs e)
		{
			Element.Arrange(e);
		}

		void RemoveTabs()
		{
			if (_tabLayoutFragment != null)
			{
				var fragment = _tabLayoutFragment;
				_tabLayoutFragment = null;
				_ = _context
						.GetNavigationRootManager()
						.FragmentManager
						.BeginTransaction()
						.Remove(fragment)
						.SetReorderingAllowed(true)
						.Commit();

				_tabplacementId = 0;
			}
		}

		void OnTabbedPageDisappearing(object sender, EventArgs e)
		{
			RemoveTabs();
		}

		void OnTabbedPageAppearing(object sender, EventArgs e)
		{
			SetTabLayout();
		}

		internal void SetTabLayout()
		{
			int id;
			var rootManager =
				_context.GetNavigationRootManager();
			_tabItemStyleLoaded = false;
			if (rootManager.RootView == null)
			{
				rootManager.RootViewChanged += RootViewChanged;

				void RootViewChanged(object sender, EventArgs e)
				{
					rootManager.RootViewChanged -= RootViewChanged;
					SetTabLayout();
				}

				return;
			}

			if (IsBottomTabPlacement)
			{
				id = Resource.Id.navigationlayout_bottomtabs;
				if (_tabplacementId == id)
					return;

				_tabLayoutFragment = new ViewFragment(BottomNavigationView);

				var layoutContent = rootManager.RootView.FindViewById(Resource.Id.navigationlayout_content);
				if (layoutContent.LayoutParameters is ViewGroup.MarginLayoutParams cl)
				{
					cl.BottomMargin = _context.Context.Resources.GetDimensionPixelSize(Resource.Dimension.design_bottom_navigation_height);
				}
			}
			else
			{
				id = Resource.Id.navigationlayout_toptabs;
				if (_tabplacementId == id)
					return;

				_tabLayoutFragment = new ViewFragment(TabLayout);
				var layoutContent = rootManager.RootView.FindViewById(Resource.Id.navigationlayout_content);
				if (layoutContent.LayoutParameters is ViewGroup.MarginLayoutParams cl)
				{
					cl.BottomMargin = 0;
				}
			}

			_tabplacementId = id;
			_ = rootManager
					.FragmentManager
					.BeginTransaction()
					.Replace(id, _tabLayoutFragment)
					.SetReorderingAllowed(true)
					.Commit();
		}

		void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			e.Apply((o, i, c) => SetupPage((Page)o), (o, i) => TeardownPage((Page)o), Reset);

			ViewPager2 pager = _viewPager;
			var adapter = (MultiPageFragmentStateAdapter<Page>)pager.Adapter;
			adapter.CountOverride = Element.Children.Count;
			if (IsBottomTabPlacement)
			{
				BottomNavigationView bottomNavigationView = _bottomNavigationView;

				adapter.NotifyDataSetChanged();

				if (Element.Children.Count == 0)
				{
					bottomNavigationView.Menu.Clear();
				}
				else
				{
					SetupBottomNavigationView(e);
					bottomNavigationView.SetOnItemSelectedListener(_listeners);
				}

				UpdateIgnoreContainerAreas();
			}
			else
			{
				TabLayout tabs = _tabLayout;

				adapter.NotifyDataSetChanged();
				if (Element.Children.Count == 0)
				{
					tabs.RemoveAllTabs();
					tabs.SetupWithViewPager(null);
				}
				else
				{
					new TabLayoutMediator(tabs, _viewPager, _listeners)
						.Attach();

					UpdateTabIcons();
#pragma warning disable CS0618 // Type or member is obsolete
					tabs.AddOnTabSelectedListener(_listeners);
#pragma warning restore CS0618 // Type or member is obsolete
				}

				UpdateIgnoreContainerAreas();
			}
		}

		void TeardownPage(Page page)
		{
			page.PropertyChanged -= OnPagePropertyChanged;
		}

		void SetupPage(Page page)
		{
			page.PropertyChanged += OnPagePropertyChanged;
		}

		void Reset()
		{
			foreach (var page in Element.Children)
				SetupPage(page);
		}

		void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
			{
				var page = (Page)sender;
				var index = Element.Children.IndexOf(page);

				if (IsBottomTabPlacement)
				{
					IMenuItem tab = _bottomNavigationView.Menu.GetItem(index);
					tab.SetTitle(page.Title);
				}
				else
				{
					TabLayout.Tab tab = _tabLayout.GetTabAt(index);
					tab.SetText(page.Title);
				}
			}
			else if (e.PropertyName == Page.IconImageSourceProperty.PropertyName)
			{
				var page = (Page)sender;
				var index = Element.Children.IndexOf(page);
				if (IsBottomTabPlacement)
				{
					var menuItem = _bottomNavigationView.Menu.GetItem(index);
					page.IconImageSource.LoadImage(
						_context,
						result =>
						{
							menuItem.SetIcon(result.Value);
						});
				}
				else
				{
					TabLayout.Tab tab = _tabLayout.GetTabAt(index);
					SetTabIconImageSource(page, tab);
				}
			}
		}

		internal void ScrollToCurrentPage()
		{
			if (Element.CurrentPage == null)
				return;

			// TODO MAUI
			//if (Platform != null)
			//{
			//	Platform.NavAnimationInProgress = true;
			//}

			_viewPager.SetCurrentItem(Element.Children.IndexOf(Element.CurrentPage), Element.OnThisPlatform().IsSmoothScrollEnabled());

			//if (Platform != null)
			//{
			//	Platform.NavAnimationInProgress = false;
			//}
		}

		void UpdateIgnoreContainerAreas()
		{
			foreach (IPageController child in Element.Children)
				child.IgnoresContainerArea = child is NavigationPage;
		}

		void UpdateOffscreenPageLimit()
		{
			_viewPager.OffscreenPageLimit = Element.OnThisPlatform().OffscreenPageLimit();
		}

		internal void UpdateSwipePaging()
		{
			// TODO MAUI
			//_viewPager.EnableGesture = Element.OnThisPlatform().IsSwipePagingEnabled();
		}

		List<(string title, ImageSource icon, bool tabEnabled)> CreateTabList()
		{
			var items = new List<(string title, ImageSource icon, bool tabEnabled)>();

			for (int i = 0; i < Element.Children.Count; i++)
			{
				var item = Element.Children[i];
				items.Add((item.Title, item.IconImageSource, item.IsEnabled));
			}

			return items;
		}

		void SetupBottomNavigationView(NotifyCollectionChangedEventArgs e)
		{
			var currentIndex = Element.Children.IndexOf(Element.CurrentPage);
			var items = CreateTabList();

			BottomNavigationViewUtils.SetupMenu(
				_bottomNavigationView.Menu,
				_bottomNavigationView.MaxItemCount,
				items,
				currentIndex,
				_bottomNavigationView,
				Element.FindMauiContext());

			if (Element.CurrentPage == null && Element.Children.Count > 0)
				Element.CurrentPage = Element.Children[0];
		}

		void UpdateTabIcons()
		{
			TabLayout tabs = _tabLayout;

			if (tabs.TabCount != Element.Children.Count)
				return;

			for (var i = 0; i < Element.Children.Count; i++)
			{
				Page child = Element.Children[i];
				TabLayout.Tab tab = tabs.GetTabAt(i);
				SetTabIconImageSource(child, tab);
			}
		}

		protected virtual void SetTabIconImageSource(TabLayout.Tab tab, Drawable icon)
		{
			tab.SetIcon(icon);
			SetIconColorFilter(tab);
		}

		void SetTabIconImageSource(Page page, TabLayout.Tab tab)
		{
			page.IconImageSource.LoadImage(
				_context,
				result =>
				{
					SetTabIconImageSource(tab, result?.Value);
				});
		}

		internal void UpdateBarBackgroundColor()
		{
			if (Element.BarBackground != null)
				return;

			if (IsBottomTabPlacement)
			{
				Color tintColor = Element.BarBackgroundColor;

				if (tintColor == null)
					_bottomNavigationView.SetBackground(null);
				else if (tintColor != null)
					_bottomNavigationView.SetBackgroundColor(tintColor.ToPlatform());
			}
			else
			{
				Color tintColor = Element.BarBackgroundColor;

				if (tintColor == null)
					_tabLayout.BackgroundTintMode = null;
				else
				{
					_tabLayout.BackgroundTintMode = PorterDuff.Mode.Src;
					_tabLayout.BackgroundTintList = ColorStateList.ValueOf(tintColor.ToPlatform());
				}
			}
		}

		internal void UpdateBarBackground()
		{
			if (_currentBarBackground == Element.BarBackground)
				return;

			_currentBarBackground = Element.BarBackground;

			if (IsBottomTabPlacement)
				_bottomNavigationView.UpdateBackground(_currentBarBackground);
			else
				_tabLayout.UpdateBackground(_currentBarBackground);
		}

		protected virtual ColorStateList GetItemTextColorStates()
		{
			if (_originalTabTextColors == null)
				_originalTabTextColors = (IsBottomTabPlacement) ? _bottomNavigationView.ItemTextColor : _tabLayout.TabTextColors;

			Color barItemColor = BarItemColor;
			Color barTextColor = Element.BarTextColor;
			Color barSelectedItemColor = BarSelectedItemColor;

			if (barItemColor == null && barTextColor == null && barSelectedItemColor == null)
				return _originalTabTextColors;

			if (_newTabTextColors != null)
				return _newTabTextColors;

			int checkedColor;
			int defaultColor;

			if (barTextColor != null)
			{
				checkedColor = barTextColor.ToPlatform().ToArgb();
				defaultColor = checkedColor;
			}
			else
			{
				defaultColor = barItemColor.ToPlatform().ToArgb();

				if (barItemColor == null && _originalTabTextColors != null)
					defaultColor = _originalTabTextColors.DefaultColor;

				checkedColor = defaultColor;

				if (barSelectedItemColor != null)
					checkedColor = barSelectedItemColor.ToPlatform().ToArgb();
			}

			_newTabTextColors = GetColorStateList(defaultColor, checkedColor);
			return _newTabTextColors;
		}

		protected virtual ColorStateList GetItemIconTintColorState()
		{
			if (IsBottomTabPlacement)
			{
				if (_orignalTabIconColors == null)
					_orignalTabIconColors = _bottomNavigationView.ItemIconTintList;
			}
			// this ensures that existing behavior doesn't change
			else if (!IsBottomTabPlacement && BarSelectedItemColor != null && BarItemColor == null)
				return null;

			Color barItemColor = BarItemColor;
			Color barSelectedItemColor = BarSelectedItemColor;

			if (barItemColor == null && barSelectedItemColor == null)
				return _orignalTabIconColors;

			if (_newTabIconColors != null)
				return _newTabIconColors;

			int defaultColor = barItemColor.ToPlatform().ToArgb();

			if (barItemColor == null && _orignalTabIconColors != null)
				defaultColor = _orignalTabIconColors.DefaultColor;

			int checkedColor = defaultColor;

			if (barSelectedItemColor != null)
				checkedColor = barSelectedItemColor.ToPlatform().ToArgb();

			_newTabIconColors = GetColorStateList(defaultColor, checkedColor);
			return _newTabIconColors;
		}


		void OnMoreSheetDismissed(object sender, EventArgs e)
		{
			var index = Element.Children.IndexOf(Element.CurrentPage);
			using (var menu = _bottomNavigationView.Menu)
			{
				index = Math.Min(index, menu.Size() - 1);
				if (index < 0)
					return;
				using (var menuItem = menu.GetItem(index))
					menuItem.SetChecked(true);
			}

			if (sender is BottomSheetDialog bsd)
				bsd.DismissEvent -= OnMoreSheetDismissed;
		}

		void OnMoreItemSelected(int selectedIndex, BottomSheetDialog dialog)
		{
			if (selectedIndex >= 0 && _bottomNavigationView.SelectedItemId != selectedIndex && Element.Children.Count > selectedIndex)
				Element.CurrentPage = Element.Children[selectedIndex];

			dialog.Dismiss();
			dialog.DismissEvent -= OnMoreSheetDismissed;
			dialog.Dispose();
		}

		void UpdateItemIconColor()
		{
			_newTabIconColors = null;

			if (IsBottomTabPlacement)
				_bottomNavigationView.ItemIconTintList = GetItemIconTintColorState() ?? _orignalTabIconColors;
			else
			{
				var colors = GetItemIconTintColorState() ?? _orignalTabIconColors;
				for (int i = 0; i < _tabLayout.TabCount; i++)
				{
					TabLayout.Tab tab = _tabLayout.GetTabAt(i);
					this.SetIconColorFilter(tab);
				}
			}
		}

		internal void UpdateTabItemStyle()
		{
			Color barItemColor = BarItemColor;
			Color barTextColor = Element.BarTextColor;
			Color barSelectedItemColor = BarSelectedItemColor;

			if (_tabItemStyleLoaded &&
				_currentBarItemColor == barItemColor &&
				_currentBarTextColor == barTextColor &&
				_currentBarSelectedItemColor == barSelectedItemColor)
			{
				return;
			}

			_tabItemStyleLoaded = true;
			_currentBarItemColor = BarItemColor;
			_currentBarTextColor = Element.BarTextColor;
			_currentBarSelectedItemColor = BarSelectedItemColor;

			UpdateBarTextColor();
			UpdateItemIconColor();
		}

		void UpdateBarTextColor()
		{
			_newTabTextColors = null;

			_currentBarTextColorStateList = GetItemTextColorStates() ?? _originalTabTextColors;
			if (IsBottomTabPlacement)
				_bottomNavigationView.ItemTextColor = _currentBarTextColorStateList;
			else
				_tabLayout.TabTextColors = _currentBarTextColorStateList;
		}

		void SetIconColorFilter(TabLayout.Tab tab)
		{
			SetIconColorFilter(tab, _tabLayout.GetTabAt(_tabLayout.SelectedTabPosition) == tab);
		}

		void SetIconColorFilter(TabLayout.Tab tab, bool selected)
		{
			var icon = tab.Icon;
			if (icon == null)
				return;

			var colors = GetItemIconTintColorState();
			if (colors == null)
				ADrawableCompat.SetTintList(icon, null);
			else
			{
				int[] _stateSet = null;

				if (selected)
					_stateSet = GetSelectedStateSet();
				else
					_stateSet = GetEmptyStateSet();

				if (colors.GetColorForState(_stateSet, _defaultAndroidColor) == _defaultARGBColor)
					ADrawableCompat.SetTintList(icon, null);
				else
				{
					var wrappedIcon = ADrawableCompat.Wrap(icon);
					if (wrappedIcon != icon)
					{
						icon = wrappedIcon;
						tab.SetIcon(wrappedIcon);
					}

					icon.Mutate();
					icon.SetState(_stateSet);
					ADrawableCompat.SetTintList(icon, colors);
				}
			}
			icon.InvalidateSelf();
		}

		int[] GetSelectedStateSet()
		{
			if (IsBottomTabPlacement)
			{
				if (_checkedStateSet == null)
					_checkedStateSet = new int[] { global::Android.Resource.Attribute.StateChecked };

				return _checkedStateSet;
			}
			else
			{
				if (_selectedStateSet == null)
					_selectedStateSet = GetStateSet(new TempView(_context.Context).SelectedStateSet);

				return _selectedStateSet;
			}
		}

		int[] GetEmptyStateSet()
		{
			if (_emptyStateSet == null)
				_emptyStateSet = GetStateSet(new TempView(_context.Context).EmptyStateSet);

			return _emptyStateSet;
		}

		class TempView : AView
		{
			// These are protected static so need to be inside a View Instance to retrieve these
			public new IList<int> EmptyStateSet => AView.EmptyStateSet;
			public new IList<int> SelectedStateSet => AView.SelectedStateSet;
			public TempView(Context context) : base(context)
			{
			}
		}

		int[] GetStateSet(IList<int> stateSet)
		{
			var results = new int[stateSet.Count];
			for (int i = 0; i < results.Length; i++)
				results[i] = stateSet[i];

			return results;
		}

		ColorStateList GetColorStateList(int defaultColor, int checkedColor)
		{
			int[][] states = new int[2][];
			int[] colors = new int[2];

			states[0] = GetSelectedStateSet();
			colors[0] = checkedColor;
			states[1] = GetEmptyStateSet();
			colors[1] = defaultColor;

			return new ColorStateList(states, colors);
		}

		class Listeners : Java.Lang.Object,
#pragma warning disable CS0618 // Type or member is obsolete
			TabLayout.IOnTabSelectedListener,
#pragma warning restore CS0618 // Type or member is obsolete
			ViewPager.IOnPageChangeListener,
			NavigationBarView.IOnItemSelectedListener,
			TabLayoutMediator.ITabConfigurationStrategy
		{
			readonly TabbedPageManager _tabbedPageManager;

			public Listeners(TabbedPageManager tabbedPageManager)
			{
				_tabbedPageManager = tabbedPageManager;
			}

			void TabLayoutMediator.ITabConfigurationStrategy.OnConfigureTab(TabLayout.Tab p0, int p1)
			{
				p0.SetText(_tabbedPageManager.Element.Children[p1].Title);
			}

			bool NavigationBarView.IOnItemSelectedListener.OnNavigationItemSelected(IMenuItem item)
			{
				if (_tabbedPageManager.Element == null)
					return false;

				var id = item.ItemId;
				if (id == BottomNavigationViewUtils.MoreTabId)
				{
					var items = _tabbedPageManager.CreateTabList();
					var bottomSheetDialog = BottomNavigationViewUtils.CreateMoreBottomSheet(_tabbedPageManager.OnMoreItemSelected, _tabbedPageManager.Element.FindMauiContext(), items, _tabbedPageManager._bottomNavigationView.MaxItemCount);
					bottomSheetDialog.DismissEvent += _tabbedPageManager.OnMoreSheetDismissed;
					bottomSheetDialog.Show();
				}
				else
				{
					if (_tabbedPageManager._bottomNavigationView.SelectedItemId != item.ItemId && _tabbedPageManager.Element.Children.Count > item.ItemId)
						_tabbedPageManager.Element.CurrentPage = _tabbedPageManager.Element.Children[item.ItemId];
				}

				return true;
			}


			void TabLayout.IOnTabSelectedListener.OnTabReselected(TabLayout.Tab tab)
			{
			}

			void TabLayout.IOnTabSelectedListener.OnTabSelected(TabLayout.Tab tab)
			{
				if (_tabbedPageManager.Element == null)
					return;

				int selectedIndex = tab.Position;
				if (_tabbedPageManager.Element.Children.Count > selectedIndex && selectedIndex >= 0)
					_tabbedPageManager.Element.CurrentPage = _tabbedPageManager.Element.Children[selectedIndex];

				_tabbedPageManager.SetIconColorFilter(tab, true);
			}

			void TabLayout.IOnTabSelectedListener.OnTabUnselected(TabLayout.Tab tab)
			{
				_tabbedPageManager.SetIconColorFilter(tab, false);
			}
			void ViewPager.IOnPageChangeListener.OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
			{
			}

			void ViewPager.IOnPageChangeListener.OnPageScrollStateChanged(int state)
			{
			}

			void ViewPager.IOnPageChangeListener.OnPageSelected(int position)
			{
				var _previousPage = _tabbedPageManager._previousPage;
				var Element = _tabbedPageManager.Element;
				var IsBottomTabPlacement = _tabbedPageManager.IsBottomTabPlacement;
				var _bottomNavigationView = _tabbedPageManager._bottomNavigationView;

				if (_previousPage != Element.CurrentPage)
				{
					_previousPage?.SendDisappearing();
					_previousPage = Element.CurrentPage;
					_tabbedPageManager._previousPage = Element.CurrentPage;
				}
				Element.CurrentPage = Element.Children[position];
				Element.CurrentPage.SendAppearing();

				if (IsBottomTabPlacement)
					_bottomNavigationView.SelectedItemId = position;
			}
		}
	}
}
