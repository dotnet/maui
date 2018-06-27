using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using AWidget = Android.Widget;
using Android.Views;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using ADrawableCompat = Android.Support.V4.Graphics.Drawable.DrawableCompat;
using AView = Android.Views.View;
using AMenu = Android.Views.Menu;
using AColor = Android.Graphics.Color;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	public class TabbedPageRenderer : VisualElementRenderer<TabbedPage>, TabLayout.IOnTabSelectedListener, ViewPager.IOnPageChangeListener, IManageFragments, BottomNavigationView.IOnNavigationItemSelectedListener
	{
		Drawable _backgroundDrawable;
		Drawable _wrappedBackgroundDrawable;
		ColorStateList _originalTabTextColors;
		ColorStateList _orignalTabIconColors;

		ColorStateList _newTabTextColors;
		ColorStateList _newTabIconColors;

		bool _disposed;
		FragmentManager _fragmentManager;
		TabLayout _tabLayout;
		BottomNavigationView _bottomNavigationView;
		AWidget.RelativeLayout _relativeLayout;
		bool _useAnimations = true;
		FormsViewPager _viewPager;
		Page _previousPage;
		int[] _checkedStateSet = null;
		int[] _selectedStateSet = null;
		int[] _emptyStateSet = null;
		int _defaultARGBColor = Color.Default.ToAndroid().ToArgb();
		AColor _defaultAndroidColor = Color.Default.ToAndroid();

		public TabbedPageRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use TabbedPageRenderer(Context) instead.")]
		public TabbedPageRenderer()
		{
			AutoPackage = false;
		}

		FragmentManager FragmentManager => _fragmentManager ?? (_fragmentManager = ((FormsAppCompatActivity)Context).SupportFragmentManager);
		bool IsBottomTabPlacement => (Element != null) ? Element.OnThisPlatform().GetToolbarPlacement() == ToolbarPlacement.Bottom : false;
		public Color BarItemColor => (Element != null) ? Element.OnThisPlatform().GetBarItemColor() : Color.Default;
		public Color BarSelectedItemColor => (Element != null) ? Element.OnThisPlatform().GetBarSelectedItemColor() : Color.Default;

		internal bool UseAnimations
		{
			get { return _useAnimations; }
			set
			{
				FormsViewPager pager = _viewPager;

				_useAnimations = value;
				if (pager != null)
					pager.EnableGesture = value;
			}
		}

		IPageController PageController => Element as IPageController;

		void IManageFragments.SetFragmentManager(FragmentManager childFragmentManager)
		{
			if (_fragmentManager == null)
				_fragmentManager = childFragmentManager;
		}

		void ViewPager.IOnPageChangeListener.OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
			if (!IsBottomTabPlacement)
				UpdateTabBarTranslation(position, positionOffset);
		}

		void ViewPager.IOnPageChangeListener.OnPageScrollStateChanged(int state)
		{
		}

		void ViewPager.IOnPageChangeListener.OnPageSelected(int position)
		{
			if (_previousPage != Element.CurrentPage)
			{
				_previousPage?.SendDisappearing();
				_previousPage = Element.CurrentPage;
			}
			Element.CurrentPage = Element.Children[position];
			Element.CurrentPage.SendAppearing();

			if (IsBottomTabPlacement)
				_bottomNavigationView.SelectedItemId = position;
		}

		void TabLayout.IOnTabSelectedListener.OnTabReselected(TabLayout.Tab tab)
		{
		}

		void TabLayout.IOnTabSelectedListener.OnTabSelected(TabLayout.Tab tab)
		{
			if (Element == null)
				return;

			int selectedIndex = tab.Position;
			if (Element.Children.Count > selectedIndex && selectedIndex >= 0)
				Element.CurrentPage = Element.Children[selectedIndex];

			SetIconColorFilter(tab, true);
		}

		void TabLayout.IOnTabSelectedListener.OnTabUnselected(TabLayout.Tab tab)
		{
			SetIconColorFilter(tab, false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				RemoveAllViews();
				foreach (Page pageToRemove in Element.Children)
				{
					IVisualElementRenderer pageRenderer = Android.Platform.GetRenderer(pageToRemove);
					if (pageRenderer != null)
					{
						pageRenderer.View.RemoveFromParent();
						pageRenderer.Dispose();
					}
					pageToRemove.PropertyChanged -= OnPagePropertyChanged;
					pageToRemove.ClearValue(Android.Platform.RendererProperty);
				}

				if (_viewPager != null)
				{
					_viewPager.Adapter.Dispose();
					_viewPager.Dispose();
					_viewPager = null;
				}

				if (_tabLayout != null)
				{
					_tabLayout.AddOnTabSelectedListener(null);
					_tabLayout.Dispose();
					_tabLayout = null;
				}

				if (_bottomNavigationView != null)
				{
					_bottomNavigationView.SetOnNavigationItemSelectedListener(null);
					_bottomNavigationView.Dispose();
					_bottomNavigationView = null;
				}

				if (_relativeLayout != null)
				{
					_relativeLayout.Dispose();
					_relativeLayout = null;
				}

				if (Element != null)
					PageController.InternalChildren.CollectionChanged -= OnChildrenCollectionChanged;

				_previousPage = null;
			}

			base.Dispose(disposing);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			PageController.SendAppearing();
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			PageController.SendDisappearing();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
		{
			base.OnElementChanged(e);

			var activity = (FormsAppCompatActivity)Context;

			if (e.OldElement != null)
				((IPageController)e.OldElement).InternalChildren.CollectionChanged -= OnChildrenCollectionChanged;

			if (e.NewElement != null)
			{
				if (IsBottomTabPlacement)
				{
					if (_relativeLayout == null)
					{
						_relativeLayout = new AWidget.RelativeLayout(Context)
						{
							LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent),
						};

						if (_bottomNavigationView != null)
						{
							_relativeLayout.RemoveView(_bottomNavigationView);
							_bottomNavigationView.SetOnNavigationItemSelectedListener(null);
						}

						var bottomNavigationViewLayoutParams = new AWidget.RelativeLayout.LayoutParams(
							LayoutParams.MatchParent,
							LayoutParams.WrapContent);

						bottomNavigationViewLayoutParams.AddRule(AWidget.LayoutRules.AlignParentBottom);

						_bottomNavigationView = new BottomNavigationView(Context)
						{
							LayoutParameters = bottomNavigationViewLayoutParams,
							Id = Platform.GenerateViewId()
						};

						var viewPagerParams = new AWidget.RelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
						viewPagerParams.AddRule(AWidget.LayoutRules.Above, _bottomNavigationView.Id);

						FormsViewPager pager = _viewPager = CreateFormsViewPager(activity, e.NewElement);

						pager.Id = Platform.GenerateViewId();
						pager.AddOnPageChangeListener(this);

						_relativeLayout.AddView(pager, viewPagerParams);
						_relativeLayout.AddView(_bottomNavigationView, bottomNavigationViewLayoutParams);

						AddView(_relativeLayout);
					}
				}
				else
				{
					if (_tabLayout == null)
					{
						TabLayout tabs;
						if (FormsAppCompatActivity.TabLayoutResource > 0)
							tabs = _tabLayout = activity.LayoutInflater.Inflate(FormsAppCompatActivity.TabLayoutResource, null).JavaCast<TabLayout>();
						else
							tabs = _tabLayout = new TabLayout(activity) { TabMode = TabLayout.ModeFixed, TabGravity = TabLayout.GravityFill };

						FormsViewPager pager = _viewPager = CreateFormsViewPager(activity, e.NewElement);

						pager.Id = Platform.GenerateViewId();
						pager.AddOnPageChangeListener(this);

						AddView(pager);
						AddView(tabs);
					}
				}

				OnChildrenCollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

				TabbedPage tabbedPage = e.NewElement;
				if (tabbedPage.CurrentPage != null)
					ScrollToCurrentPage();

				_previousPage = tabbedPage.CurrentPage;

				((IPageController)tabbedPage).InternalChildren.CollectionChanged += OnChildrenCollectionChanged;
				UpdateBarBackgroundColor();
				UpdateBarTextColor();
				UpdateItemIconColor();
				UpdateSwipePaging();
				UpdateOffscreenPageLimit();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == nameof(TabbedPage.CurrentPage))
			{
				if (Element.CurrentPage != null)
					ScrollToCurrentPage();
			}
			else if (e.PropertyName == NavigationPage.BarBackgroundColorProperty.PropertyName)
				UpdateBarBackgroundColor();
			else if (e.PropertyName == NavigationPage.BarTextColorProperty.PropertyName ||
				e.PropertyName == PlatformConfiguration.AndroidSpecific.TabbedPage.BarItemColorProperty.PropertyName ||
				e.PropertyName == PlatformConfiguration.AndroidSpecific.TabbedPage.BarSelectedItemColorProperty.PropertyName)
			{
				_newTabTextColors = null;
				_newTabIconColors = null;
				UpdateBarTextColor();
				UpdateItemIconColor();
			}
			else if (e.PropertyName == PlatformConfiguration.AndroidSpecific.TabbedPage.IsSwipePagingEnabledProperty.PropertyName)
				UpdateSwipePaging();
		}

		void SetNavigationRendererPadding(int paddingTop, int paddingBottom)
		{
			for (var i = 0; i < PageController.InternalChildren.Count; i++)
			{
				var child = PageController.InternalChildren[i] as VisualElement;
				if (child == null)
					continue;
				IVisualElementRenderer renderer = Android.Platform.GetRenderer(child);
				var navigationRenderer = renderer as NavigationPageRenderer;
				if (navigationRenderer != null)
				{
					navigationRenderer.ContainerTopPadding = paddingTop;
					navigationRenderer.ContainerBottomPadding = paddingBottom;
				}
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			FormsViewPager pager = _viewPager;
			Context context = Context;

			var width = r - l;
			var height = b - t;

			if (IsBottomTabPlacement)
			{
				if (width <= 0 || height <= 0)
					return;

				_relativeLayout.Measure(
					MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
					MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));

				pager.Measure(MeasureSpecFactory.MakeMeasureSpec(width, MeasureSpecMode.AtMost), MeasureSpecFactory.MakeMeasureSpec(height, MeasureSpecMode.AtMost));

				if (width > 0 && height > 0)
				{
					PageController.ContainerArea = new Rectangle(0, 0, context.FromPixels(width), context.FromPixels(height - _bottomNavigationView.MeasuredHeight));

					SetNavigationRendererPadding(0, _bottomNavigationView.MeasuredHeight);

					pager.Layout(0, 0, width, b);
					// We need to measure again to ensure that the tabs show up
					_relativeLayout.Measure(
						MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
						MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
					_relativeLayout.Layout(0, 0, _relativeLayout.MeasuredWidth, _relativeLayout.MeasuredHeight);
				}
			}
			else
			{
				TabLayout tabs = _tabLayout;

				tabs.Measure(MeasureSpecFactory.MakeMeasureSpec(width, MeasureSpecMode.Exactly), MeasureSpecFactory.MakeMeasureSpec(height, MeasureSpecMode.AtMost));
				var tabsHeight = 0;

				if (tabs.Visibility != ViewStates.Gone)
				{
					//MinimumHeight is only available on API 16+
					if ((int)Build.VERSION.SdkInt >= 16)
						tabsHeight = Math.Min(height, Math.Max(tabs.MeasuredHeight, tabs.MinimumHeight));
					else
						tabsHeight = Math.Min(height, tabs.MeasuredHeight);
				}

				pager.Measure(MeasureSpecFactory.MakeMeasureSpec(width, MeasureSpecMode.AtMost), MeasureSpecFactory.MakeMeasureSpec(height, MeasureSpecMode.AtMost));

				if (width > 0 && height > 0)
				{
					PageController.ContainerArea = new Rectangle(0, context.FromPixels(tabsHeight), context.FromPixels(width), context.FromPixels(height - tabsHeight));

					SetNavigationRendererPadding(tabsHeight, 0);

					pager.Layout(0, 0, width, b);
					// We need to measure again to ensure that the tabs show up
					tabs.Measure(MeasureSpecFactory.MakeMeasureSpec(width, MeasureSpecMode.Exactly), MeasureSpecFactory.MakeMeasureSpec(tabsHeight, MeasureSpecMode.Exactly));
					tabs.Layout(0, 0, width, tabsHeight);

					UpdateTabBarTranslation(pager.CurrentItem, 0);
				}
			}

			base.OnLayout(changed, l, t, r, b);
		}

		void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			e.Apply((o, i, c) => SetupPage((Page)o), (o, i) => TeardownPage((Page)o), Reset);

			if (IsBottomTabPlacement)
			{
				FormsViewPager pager = _viewPager;
				BottomNavigationView bottomNavigationView = _bottomNavigationView;

				((FormsFragmentPagerAdapter<Page>)pager.Adapter).CountOverride = Element.Children.Count;

				pager.Adapter.NotifyDataSetChanged();

				if (Element.Children.Count == 0)
				{
					bottomNavigationView.Menu.Clear();
				}
				else
				{
					SetupBottomNavigationView(e);
					UpdateBottomNavigationViewIcons();
					bottomNavigationView.SetOnNavigationItemSelectedListener(this);
				}

				UpdateIgnoreContainerAreas();
			}
			else
			{
				FormsViewPager pager = _viewPager;
				TabLayout tabs = _tabLayout;

				((FormsFragmentPagerAdapter<Page>)pager.Adapter).CountOverride = Element.Children.Count;
				pager.Adapter.NotifyDataSetChanged();
				if (Element.Children.Count == 0)
				{
					tabs.RemoveAllTabs();
					tabs.SetupWithViewPager(null);
				}
				else
				{
					tabs.SetupWithViewPager(pager);
					UpdateTabIcons();
					tabs.AddOnTabSelectedListener(this);
				}

				UpdateIgnoreContainerAreas();
			}
		}

		FormsViewPager CreateFormsViewPager(Context context, TabbedPage tabbedPage)
		{
			return new FormsViewPager(context)
			{
				OverScrollMode = OverScrollMode.Never,
				EnableGesture = UseAnimations,
				LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent),
				Adapter = new FormsFragmentPagerAdapter<Page>(tabbedPage, FragmentManager) { CountOverride = tabbedPage.Children.Count }
			};
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
			else if (e.PropertyName == Page.IconProperty.PropertyName)
			{
				var page = (Page)sender;
				var index = Element.Children.IndexOf(page);
				FileImageSource icon = page.Icon;

				if (IsBottomTabPlacement)
				{
					var menuItem = _bottomNavigationView.Menu.GetItem(index);
					menuItem.SetIcon(GetIconDrawable(icon));
				}
				else
				{
					TabLayout.Tab tab = _tabLayout.GetTabAt(index);
					SetTabIcon(tab, icon);
				}
			}
		}

		void ScrollToCurrentPage()
		{
			((Platform)Element.Platform).NavAnimationInProgress = true;
			_viewPager.SetCurrentItem(Element.Children.IndexOf(Element.CurrentPage), UseAnimations);
			((Platform)Element.Platform).NavAnimationInProgress = false;
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

		void UpdateSwipePaging()
		{
			_viewPager.EnableGesture = Element.OnThisPlatform().IsSwipePagingEnabled();
		}

		void UpdateTabBarTranslation(int position, float offset)
		{
			if (IsDisposed)
				return;

			TabLayout tabs = _tabLayout;

			if (position >= PageController.InternalChildren.Count)
				return;

			var leftPage = (Page)PageController.InternalChildren[position];
			IVisualElementRenderer leftRenderer = Android.Platform.GetRenderer(leftPage);

			if (leftRenderer == null)
				return;

			if (offset <= 0 || position >= PageController.InternalChildren.Count - 1)
			{
				var leftNavRenderer = leftRenderer as NavigationPageRenderer;
				if (leftNavRenderer != null)
					tabs.TranslationY = leftNavRenderer.GetNavBarHeight();
				else
					tabs.TranslationY = 0;
			}
			else
			{
				var rightPage = (Page)PageController.InternalChildren[position + 1];
				IVisualElementRenderer rightRenderer = Android.Platform.GetRenderer(rightPage);

				var leftHeight = 0;
				var leftNavRenderer = leftRenderer as NavigationPageRenderer;
				if (leftNavRenderer != null)
					leftHeight = leftNavRenderer.GetNavBarHeight();

				var rightHeight = 0;
				var rightNavRenderer = rightRenderer as NavigationPageRenderer;
				if (rightNavRenderer != null)
					rightHeight = rightNavRenderer.GetNavBarHeight();

				tabs.TranslationY = leftHeight + (rightHeight - leftHeight) * offset;
			}
		}

		void SetupBottomNavigationView(NotifyCollectionChangedEventArgs e)
		{
			if (IsDisposed)
				return;

			BottomNavigationView bottomNavigationView = _bottomNavigationView;

			int startingIndex = 0;

			if (e.Action == NotifyCollectionChangedAction.Add && e.NewStartingIndex == bottomNavigationView.Menu.Size())
				startingIndex = e.NewStartingIndex;
			else if (e.Action == NotifyCollectionChangedAction.Remove && (e.OldStartingIndex + 1) == bottomNavigationView.Menu.Size())
			{
				startingIndex = Element.Children.Count;
				bottomNavigationView.Menu.RemoveItem(e.OldStartingIndex);
			}
			else
				bottomNavigationView.Menu.Clear();


			for (var i = startingIndex; i < Element.Children.Count; i++)
			{
				Page child = Element.Children[i];
				var menuItem = bottomNavigationView.Menu.Add(AMenu.None, i, i, child.Title);
				if (Element.CurrentPage == child)
					bottomNavigationView.SelectedItemId = menuItem.ItemId;
			}

			if (Element.CurrentPage == null && Element.Children.Count > 0)
				Element.CurrentPage = Element.Children[0];
		}

		void UpdateBottomNavigationViewIcons()
		{
			if (IsDisposed)
				return;

			BottomNavigationView bottomNavigationView = _bottomNavigationView;

			for (var i = 0; i < Element.Children.Count; i++)
			{
				Page child = Element.Children[i];
				FileImageSource icon = child.Icon;
				if (string.IsNullOrEmpty(icon))
					continue;

				var menuItem = bottomNavigationView.Menu.GetItem(i);
				menuItem.SetIcon(GetIconDrawable(icon));
			}
		}

		void UpdateTabIcons()
		{
			if (IsDisposed)
				return;

			TabLayout tabs = _tabLayout;

			if (tabs.TabCount != Element.Children.Count)
				return;

			for (var i = 0; i < Element.Children.Count; i++)
			{
				Page child = Element.Children[i];
				FileImageSource icon = child.Icon;
				if (string.IsNullOrEmpty(icon))
					continue;

				TabLayout.Tab tab = tabs.GetTabAt(i);
				SetTabIcon(tab, icon);
			}
		}

		Drawable GetIconDrawable(FileImageSource icon) =>
			Context.GetDrawable(icon);

		protected virtual void SetTabIcon(TabLayout.Tab tab, FileImageSource icon)
		{
			tab.SetIcon(GetIconDrawable(icon));
			this.SetIconColorFilter(tab);
		}

		void UpdateBarBackgroundColor()
		{
			if (IsDisposed)
				return;

			if (IsBottomTabPlacement)
			{
				Color tintColor = Element.BarBackgroundColor;

				if (tintColor.IsDefault)
					_bottomNavigationView.SetBackground(null);
				else if (!tintColor.IsDefault)
					_bottomNavigationView.SetBackgroundColor(tintColor.ToAndroid());
			}
			else
			{
				Color tintColor = Element.BarBackgroundColor;

				if (Forms.IsLollipopOrNewer)
				{
					if (tintColor.IsDefault)
						_tabLayout.BackgroundTintMode = null;
					else
					{
						_tabLayout.BackgroundTintMode = PorterDuff.Mode.Src;
						_tabLayout.BackgroundTintList = ColorStateList.ValueOf(tintColor.ToAndroid());
					}
				}
				else
				{
					if (tintColor.IsDefault && _backgroundDrawable != null)
						_tabLayout.SetBackground(_backgroundDrawable);
					else if (!tintColor.IsDefault)
					{
						// if you don't create a new drawable then SetBackgroundColor
						// just sets the color on the background drawable that's saved
						// it doesn't create a new one
						if (_backgroundDrawable == null && _tabLayout.Background != null)
						{
							_backgroundDrawable = _tabLayout.Background;
							_wrappedBackgroundDrawable = ADrawableCompat.Wrap(_tabLayout.Background).Mutate();
						}

						if (_wrappedBackgroundDrawable != null)
							_tabLayout.Background = _wrappedBackgroundDrawable;

						_tabLayout.SetBackgroundColor(tintColor.ToAndroid());
					}
				}
			}
		}

		protected virtual ColorStateList GetItemTextColorStates()
		{
			if (IsDisposed)
				return null;

			if (_originalTabTextColors == null)
				_originalTabTextColors = (IsBottomTabPlacement) ? _bottomNavigationView.ItemTextColor : _tabLayout.TabTextColors;

			Color barItemColor = BarItemColor;
			Color barTextColor = Element.BarTextColor;
			Color barSelectedItemColor = BarSelectedItemColor;

			if (barItemColor.IsDefault && barTextColor.IsDefault && barSelectedItemColor.IsDefault)
				return _originalTabTextColors;

			if (_newTabTextColors != null)
				return _newTabTextColors;

			int checkedColor;
			int defaultColor;

			if (!barTextColor.IsDefault)
			{
				checkedColor = barTextColor.ToAndroid().ToArgb();
				defaultColor = checkedColor;
			}
			else
			{
				defaultColor = barItemColor.ToAndroid().ToArgb();

				if (barItemColor.IsDefault && _originalTabTextColors != null)
					defaultColor = _originalTabTextColors.DefaultColor;

				checkedColor = defaultColor;

				if (!barSelectedItemColor.IsDefault)
					checkedColor = barSelectedItemColor.ToAndroid().ToArgb();
			}

			_newTabTextColors = GetColorStateList(defaultColor, checkedColor);
			return _newTabTextColors;
		}

		protected virtual ColorStateList GetItemIconTintColorState()
		{
			if (IsDisposed)
				return null;

			if (IsBottomTabPlacement)
			{
				if (_orignalTabIconColors == null)
					_orignalTabIconColors = _bottomNavigationView.ItemIconTintList;
			}
			// this ensures that existing behavior doesn't change
			else if (!IsBottomTabPlacement && BarSelectedItemColor.IsDefault && BarItemColor.IsDefault)
				return null;

			Color barItemColor = BarItemColor;
			Color barSelectedItemColor = BarSelectedItemColor;

			if (barItemColor.IsDefault && barSelectedItemColor.IsDefault)
				return _orignalTabIconColors;

			if (_newTabIconColors != null)
				return _newTabIconColors;

			int defaultColor = barItemColor.ToAndroid().ToArgb();

			if (barItemColor.IsDefault && _orignalTabIconColors != null)
				defaultColor = _orignalTabIconColors.DefaultColor;

			int checkedColor = defaultColor;

			if (!barSelectedItemColor.IsDefault)
				checkedColor = barSelectedItemColor.ToAndroid().ToArgb();

			_newTabIconColors = GetColorStateList(defaultColor, checkedColor);
			return _newTabIconColors;
		}

		public bool OnNavigationItemSelected(IMenuItem item)
		{
			if (Element == null || IsDisposed)
				return false;

			int selectedIndex = item.Order;
			if (_bottomNavigationView.SelectedItemId != item.ItemId && Element.Children.Count > selectedIndex && selectedIndex >= 0)
				Element.CurrentPage = Element.Children[selectedIndex];

			return true;
		}

		bool IsDisposed
		{
			get
			{
				if (IsBottomTabPlacement)
				{
					if (_disposed || _relativeLayout == null || _bottomNavigationView == null)
						return true;
				}
				else
				{
					if (_disposed || _tabLayout == null)
						return true;
				}

				return false;
			}
		}

		void UpdateItemIconColor()
		{
			if (IsDisposed)
				return;

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

		void UpdateBarTextColor()
		{
			if (IsDisposed)
				return;

			var colors = GetItemTextColorStates() ?? _originalTabTextColors;
			if (IsBottomTabPlacement)
				_bottomNavigationView.ItemTextColor = colors;
			else
				_tabLayout.TabTextColors = colors;
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
					_selectedStateSet = GetStateSet(AView.SelectedStateSet);

				return _selectedStateSet;
			}
		}

		int[] GetEmptyStateSet()
		{
			if (_emptyStateSet == null)
				_emptyStateSet = GetStateSet(AView.EmptyStateSet);

			return _emptyStateSet;
		}

		int[] GetStateSet(System.Collections.Generic.IList<int> stateSet)
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
	}
}
