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

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	public class TabbedPageRenderer : VisualElementRenderer<TabbedPage>, TabLayout.IOnTabSelectedListener, ViewPager.IOnPageChangeListener, IManageFragments, BottomNavigationView.IOnNavigationItemSelectedListener
	{
		Drawable _backgroundDrawable;
		int? _defaultColor;
		bool _disposed;
		FragmentManager _fragmentManager;
		TabLayout _tabLayout;
		BottomNavigationView _bottomNavigationView;
		AWidget.RelativeLayout _relativeLayout;
		bool _useAnimations = true;
		FormsViewPager _viewPager;
		Page _previousPage;

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
			if (!Element.OnThisPlatform().UseBottomNavigation())
			{
				UpdateTabBarTranslation(position, positionOffset);
			}
		}

		void ViewPager.IOnPageChangeListener.OnPageScrollStateChanged(int state)
		{
		}

		void ViewPager.IOnPageChangeListener.OnPageSelected(int position)
		{
			if(_previousPage != Element.CurrentPage)
			{
				_previousPage?.SendDisappearing();
				_previousPage = Element.CurrentPage;
			}
			Element.CurrentPage = Element.Children[position];
			Element.CurrentPage.SendAppearing();
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
		}

		void TabLayout.IOnTabSelectedListener.OnTabUnselected(TabLayout.Tab tab)
		{
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
				if (Element.OnThisPlatform().UseBottomNavigation())
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

						FormsViewPager pager =
							_viewPager =
								new FormsViewPager(activity)
								{
									OverScrollMode = OverScrollMode.Never,
									EnableGesture = UseAnimations,
									LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent),
									Adapter = new FormsFragmentPagerAdapter<Page>(e.NewElement, FragmentManager)
									{
										CountOverride = e.NewElement.Children.Count
									}
								};

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
						{
							tabs = _tabLayout = activity.LayoutInflater.Inflate(FormsAppCompatActivity.TabLayoutResource, null).JavaCast<TabLayout>();
						}
						else
							tabs = _tabLayout = new TabLayout(activity) { TabMode = TabLayout.ModeFixed, TabGravity = TabLayout.GravityFill };

						FormsViewPager pager =
							_viewPager =
							new FormsViewPager(activity)
							{
								OverScrollMode = OverScrollMode.Never,
								EnableGesture = UseAnimations,
								LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent),
								Adapter = new FormsFragmentPagerAdapter<Page>(e.NewElement, FragmentManager) { CountOverride = e.NewElement.Children.Count }
							};
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
			else if (e.PropertyName == NavigationPage.BarTextColorProperty.PropertyName)
				UpdateBarTextColor();
			else if (e.PropertyName == PlatformConfiguration.AndroidSpecific.TabbedPage.IsSwipePagingEnabledProperty.PropertyName)
				UpdateSwipePaging();
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			FormsViewPager pager = _viewPager;
			Context context = Context;

			var width = r - l;
			var height = b - t;

			if (Element.OnThisPlatform().UseBottomNavigation())
			{
				if (width <= 0 || height <= 0)
				{
					return;
				}
				_relativeLayout.Measure(
					MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
					MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
				
				pager.Measure(MeasureSpecFactory.MakeMeasureSpec(width, MeasureSpecMode.AtMost), MeasureSpecFactory.MakeMeasureSpec(height, MeasureSpecMode.AtMost));

				if (width > 0 && height > 0)
				{
					PageController.ContainerArea = new Rectangle(0, 0, context.FromPixels(width), context.FromPixels(height - _bottomNavigationView.Height));

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

					for (var i = 0; i < PageController.InternalChildren.Count; i++)
					{
						var child = PageController.InternalChildren[i] as VisualElement;
						if (child == null)
							continue;
						IVisualElementRenderer renderer = Android.Platform.GetRenderer(child);
						var navigationRenderer = renderer as NavigationPageRenderer;
						if (navigationRenderer != null)
							navigationRenderer.ContainerPadding = tabsHeight;
					}

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

			if (Element.OnThisPlatform().UseBottomNavigation())
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
					SetupBottomNavigationView();
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
				TabLayout.Tab tab = _tabLayout.GetTabAt(index);
				tab.SetText(page.Title);
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

		void SetupBottomNavigationView()
		{
			BottomNavigationView bottomNavigationView = _bottomNavigationView;
			bottomNavigationView.Menu.Clear();
			for (var i = 0; i < Element.Children.Count; i++)
			{
				Page child = Element.Children[i];
				bottomNavigationView.Menu.Add(0, i, i, child.Title).SetEnabled(child.IsEnabled);
			}

			if (Element.Children.Count > 0)
			{
				Element.CurrentPage = Element.Children[0];
			}
		}

		void UpdateBottomNavigationViewIcons()
		{
			BottomNavigationView bottomNavigationView = _bottomNavigationView;

			for (var i = 0; i < Element.Children.Count; i++)
			{
				Page child = Element.Children[i];

				FileImageSource icon = child.Icon;
				if (string.IsNullOrEmpty(icon))
					continue;

				var menuItem = bottomNavigationView.Menu.GetItem(i);

				menuItem.SetIcon(ResourceManager.IdFromTitle(icon, ResourceManager.DrawableClass));
			}
		}

		void UpdateTabIcons()
		{
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

		protected virtual void SetTabIcon(TabLayout.Tab tab, FileImageSource icon)
		{
			tab.SetIcon(ResourceManager.IdFromTitle(icon, ResourceManager.DrawableClass));
		}

		void UpdateBarBackgroundColor()
		{
			if (Element.OnThisPlatform().UseBottomNavigation())
			{
				if (_disposed || _relativeLayout == null || _bottomNavigationView == null)
					return;

				Color tintColor = Element.BarBackgroundColor;

				if (Forms.IsLollipopOrNewer)
				{
					if (tintColor.IsDefault)
						_bottomNavigationView.SetBackgroundColor(Color.Default.ToAndroid());
					else
						_bottomNavigationView.SetBackgroundColor(tintColor.ToAndroid());
				}
				else
				{
					if (tintColor.IsDefault && _backgroundDrawable != null)
						_bottomNavigationView.SetBackground(_backgroundDrawable);
					else if (!tintColor.IsDefault)
					{
						if (_backgroundDrawable == null)
							_backgroundDrawable = _bottomNavigationView.Background;
						_bottomNavigationView.SetBackgroundColor(tintColor.ToAndroid());
					}
				}
			}
			else
			{
				if (_disposed || _tabLayout == null)
					return;

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
						if (_backgroundDrawable == null)
							_backgroundDrawable = _tabLayout.Background;
						_tabLayout.SetBackgroundColor(tintColor.ToAndroid());
					}
				}
			}
		}

		void UpdateBarTextColor()
		{
			if (Element.OnThisPlatform().UseBottomNavigation())
			{
				if (_disposed || _relativeLayout == null || _bottomNavigationView == null)
					return;

				int currentColor = _bottomNavigationView.ItemTextColor.DefaultColor;

        if (!_defaultColor.HasValue)
					_defaultColor = currentColor;

				Color newTextColor = Element.BarTextColor;
				int newTextColorArgb = newTextColor.ToAndroid().ToArgb();
				var color = newTextColor.ToAndroid();
				color.A = 128;
				int newTextColorArgbFade = color.ToArgb();

				int[] sChecked = { global::Android.Resource.Attribute.StateChecked };
				int[] sDefault = { };

				if (!newTextColor.IsDefault && currentColor != newTextColorArgb)
				{
					_bottomNavigationView.ItemTextColor = new ColorStateList(new[] { sChecked, sDefault }, new[] { newTextColorArgb, color });
					_bottomNavigationView.ItemIconTintList = new ColorStateList(new[] { sChecked, sDefault }, new[] { newTextColorArgb, color });

				}
			}
			else
			{
				if (_disposed || _tabLayout == null)
					return;

				int currentColor = _tabLayout.TabTextColors.DefaultColor;

				if (!_defaultColor.HasValue)
					_defaultColor = currentColor;

				Color newTextColor = Element.BarTextColor;
				int newTextColorArgb = newTextColor.ToAndroid().ToArgb();

				if (!newTextColor.IsDefault && currentColor != newTextColorArgb)
					_tabLayout.SetTabTextColors(newTextColorArgb, newTextColorArgb);
				else if (newTextColor.IsDefault && _defaultColor.HasValue && currentColor != _defaultColor)
					_tabLayout.SetTabTextColors(_defaultColor.Value, _defaultColor.Value);
			}

		}

		public bool OnNavigationItemSelected(IMenuItem item)
		{
			if (Element == null)
				return false;

			int selectedIndex = item.Order;
			if (_bottomNavigationView.SelectedItemId != item.Order && Element.Children.Count > selectedIndex && selectedIndex >= 0)
			{
				Element.CurrentPage = Element.Children[selectedIndex];
			}
			return true;
		}
	}
}
