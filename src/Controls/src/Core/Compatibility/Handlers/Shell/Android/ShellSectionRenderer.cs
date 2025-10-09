#nullable disable
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.Fragment.App;
using AndroidX.ViewPager.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;
using Google.Android.Material.AppBar;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellSectionRenderer : Fragment, IShellSectionRenderer//, ViewPager.IOnPageChangeListener
		, AView.IOnClickListener, IShellObservableFragment, IAppearanceObserver, TabLayoutMediator.ITabConfigurationStrategy
	{
		#region ITabConfigurationStrategy

		void TabLayoutMediator.ITabConfigurationStrategy.OnConfigureTab(TabLayout.Tab tab, int position)
		{
			tab.SetText(new String(SectionController.GetItems()[position].Title));
		}

		void UpdateCurrentItem(ShellContent content)
		{
			if (_toolbarTracker == null)
				return;

			var page = ((IShellContentController)content).GetOrCreateContent();
			if (page == null)
				throw new ArgumentNullException(nameof(page), "Shell Content Page is Null");

			ShellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, content);
			_toolbarTracker.Page = page;
		}

		#endregion IOnPageChangeListener

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				ResetAppearance();
			else
				SetAppearance(appearance);
		}

		#endregion IAppearanceObserver

		#region IOnClickListener

		void AView.IOnClickListener.OnClick(AView v)
		{
		}

		#endregion IOnClickListener

		readonly IShellContext _shellContext;
		AView _rootView;
		bool _selecting;
		TabLayout _tablayout;
		IShellTabLayoutAppearanceTracker _tabLayoutAppearanceTracker;
		AToolbar _toolbar;
		IShellToolbarAppearanceTracker _toolbarAppearanceTracker;
		IShellToolbarTracker _toolbarTracker;
		ViewPager2 _viewPager;
		bool _disposed;
		IShellController ShellController => _shellContext.Shell;
		public event EventHandler AnimationFinished;
		Fragment IShellObservableFragment.Fragment => this;
		public ShellSection ShellSection { get; set; }
		protected IShellContext ShellContext => _shellContext;
		IShellSectionController SectionController => (IShellSectionController)ShellSection;
		IMauiContext MauiContext => ShellContext.Shell.Handler.MauiContext;

		public ShellSectionRenderer(IShellContext shellContext)
		{
			_shellContext = shellContext;
		}


		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var shellSection = ShellSection;
			if (shellSection == null)
				return null;

			if (shellSection.CurrentItem == null)
				throw new InvalidOperationException($"Content not found for active {shellSection}. Title: {shellSection.Title}. Route: {shellSection.Route}.");

			var context = Context;
			var root = new MauiCoordinatorLayout(context);

			// Create AppBarLayout directly instead of using PlatformInterop
			var appbar = new AppBarLayout(context, null, Resource.Attribute.appBarLayoutStyle);
			appbar.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
			root.AddView(appbar);

			int actionBarHeight = context.GetActionBarHeight();

			var shellToolbar = new Toolbar(shellSection);
			ShellToolbarTracker.ApplyToolbarChanges(_shellContext.Shell.Toolbar, shellToolbar);
			_toolbar = (AToolbar)shellToolbar.ToPlatform(_shellContext.Shell.FindMauiContext());
			appbar.AddView(_toolbar);

			// Create TabLayout directly instead of using PlatformInterop
			_tablayout = new TabLayout(context);
			var layoutParams = new AppBarLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, actionBarHeight);
			layoutParams.Gravity = GravityFlags.Bottom;
			_tablayout.LayoutParameters = layoutParams;
			_tablayout.TabMode = TabLayout.ModeScrollable;
			appbar.AddView(_tablayout);

			var pagerContext = MauiContext.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager);
			var adapter = new ShellFragmentStateAdapter(shellSection, ChildFragmentManager, pagerContext);
			var pageChangedCallback = new ViewPagerPageChanged(this);

			// Create ViewPager2 directly instead of using PlatformInterop
			_viewPager = new ViewPager2(context);
			var viewPagerLayoutParams = new CoordinatorLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
			viewPagerLayoutParams.Behavior = new AppBarLayout.ScrollingViewBehavior();
			_viewPager.OverScrollMode = OverScrollMode.Never;
			_viewPager.Id = AView.GenerateViewId();
			_viewPager.LayoutParameters = viewPagerLayoutParams;
			_viewPager.Adapter = adapter;
			_viewPager.RegisterOnPageChangeCallback(pageChangedCallback);
			root.AddView(_viewPager);

			// Create TabLayoutMediator to connect TabLayout with ViewPager2
			new TabLayoutMediator(_tablayout, _viewPager, this).Attach();

			Page currentPage = null;
			int currentIndex = -1;
			var currentItem = shellSection.CurrentItem;
			var items = SectionController.GetItems();

			while (currentIndex < 0 && items.Count > 0 && shellSection.CurrentItem != null)
			{
				currentItem = shellSection.CurrentItem;
				currentPage = ((IShellContentController)shellSection.CurrentItem).GetOrCreateContent();

				// current item hasn't changed
				if (currentItem == shellSection.CurrentItem)
					currentIndex = items.IndexOf(currentItem);
			}

			_toolbarTracker = _shellContext.CreateTrackerForToolbar(_toolbar);
			_toolbarTracker.SetToolbar(shellToolbar);
			_toolbarTracker.Page = currentPage;

			_viewPager.CurrentItem = currentIndex;

			if (items.Count == 1)
			{
				UpdateTablayoutVisibility();
			}

			_tablayout.LayoutChange += OnTabLayoutChange;

			_tabLayoutAppearanceTracker = _shellContext.CreateTabLayoutAppearanceTracker(ShellSection);
			_toolbarAppearanceTracker = _shellContext.CreateToolbarAppearanceTracker();

			HookEvents();

			return _rootView = root;
		}

		void OnShellContentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellContent.TitleProperty.PropertyName && sender is ShellContent shellContent)
			{
				UpdateTabTitle(shellContent);
			}
		}

		void UpdateTabTitle(ShellContent shellContent)
		{
			if (_tablayout == null || SectionController.GetItems().Count == 0)
				return;

			int index = SectionController.GetItems().IndexOf(shellContent);
			if (index >= 0)
			{
				var tab = _tablayout.GetTabAt(index);
				tab?.SetText(new string(shellContent.Title));
			}
		}

		void OnTabLayoutChange(object sender, AView.LayoutChangeEventArgs e)
		{
			if (_disposed)
				return;

			var items = SectionController.GetItems();
			for (int i = 0; i < _tablayout.TabCount; i++)
			{
				if (items.Count <= i)
					break;

				var tab = _tablayout.GetTabAt(i);

				if (tab.View != null)
					AutomationPropertiesProvider.AccessibilitySettingsChanged(tab.View, items[i]);
			}
		}

		void Destroy()
		{
			if (_rootView != null)
			{
				UnhookEvents();

				_shellContext?.Shell?.Toolbar?.Handler?.DisconnectHandler();

				//_viewPager.RemoveOnPageChangeListener(this);
				var adapter = _viewPager.Adapter;
				_viewPager.Adapter = null;
				adapter.Dispose();

				_tablayout.LayoutChange -= OnTabLayoutChange;
				_toolbarAppearanceTracker.Dispose();
				_tabLayoutAppearanceTracker.Dispose();
				_toolbarTracker.Dispose();
				_tablayout.Dispose();
				_toolbar.Dispose();
				_viewPager.Dispose();
				_rootView.Dispose();
			}

			_toolbarAppearanceTracker = null;
			_tabLayoutAppearanceTracker = null;
			_toolbarTracker = null;
			_tablayout = null;
			_toolbar = null;
			_viewPager = null;
			_rootView = null;

		}

		// Use OnDestroy instead of OnDestroyView because OnDestroyView will be
		// called before the animation completes. This causes tons of tiny issues.
		public override void OnDestroy()
		{
			Destroy();
			base.OnDestroy();
		}
		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				Destroy();
			}
		}

		protected virtual void OnAnimationFinished(EventArgs e)
		{
			AnimationFinished?.Invoke(this, e);
		}

		protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateTablayoutVisibility();

			if (_viewPager?.Adapter is ShellFragmentStateAdapter adapter)
			{
				adapter.OnItemsCollectionChanged(sender, e);
				SafeNotifyDataSetChanged();
			}
		}

		void SafeNotifyDataSetChanged(int iteration = 0)
		{
			if (_disposed)
				return;

			if (!_viewPager.IsAlive())
				return;

			if (iteration >= 10)
			{
				// It's very unlikely this will happen but just in case there's a scenario
				// where we might hit an infinite loop we're adding an exit strategy
				MauiContext.CreateLogger<ShellSectionRenderer>()
					.LogWarning("ViewPager2 stuck in layout, unable to NotifyDataSetChanged;");

				return;
			}

			if (_viewPager?.Adapter is ShellFragmentStateAdapter adapter)
			{
				// https://stackoverflow.com/questions/43221847/cannot-call-this-method-while-recyclerview-is-computing-a-layout-or-scrolling-wh
				// ViewPager2 is based on RecyclerView which really doesn't like NotifyDataSetChanged when a layout is happening
				if (!_viewPager.IsInLayout)
				{
					adapter.NotifyDataSetChanged();
				}
				else
				{
					_viewPager.Post(() => SafeNotifyDataSetChanged(++iteration));
				}
			}
		}

		void UpdateTablayoutVisibility()
		{
			_tablayout.Visibility = (SectionController.GetItems().Count > 1) ? ViewStates.Visible : ViewStates.Gone;
			if (_tablayout.Visibility == ViewStates.Gone)
			{
				SetViewPager2UserInputEnabled(false);
				_viewPager.ImportantForAccessibility = ImportantForAccessibility.No;
			}
			else
			{
				SetViewPager2UserInputEnabled(true);
				_viewPager.ImportantForAccessibility = ImportantForAccessibility.Auto;
			}
		}

		protected virtual void SetViewPager2UserInputEnabled(bool value)
		{
			_viewPager.UserInputEnabled = value;
		}

		protected virtual void OnShellItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_rootView == null)
				return;

			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				var newIndex = SectionController.GetItems().IndexOf(ShellSection.CurrentItem);

				if (SectionController.GetItems().Count != _viewPager.ChildCount)
				{
					SafeNotifyDataSetChanged();
				}

				if (newIndex >= 0)
				{
					_viewPager.CurrentItem = newIndex;
				}
			}
		}

		protected virtual void ResetAppearance()
		{
			_toolbarAppearanceTracker.ResetAppearance(_toolbar, _toolbarTracker);
			_tabLayoutAppearanceTracker.ResetAppearance(_tablayout);
		}

		protected virtual void SetAppearance(ShellAppearance appearance)
		{
			_toolbarAppearanceTracker.SetAppearance(_toolbar, _toolbarTracker, appearance);
			_tabLayoutAppearanceTracker.SetAppearance(_tablayout, appearance);
		}

		void HookEvents()
		{
			SectionController.ItemsCollectionChanged += OnItemsCollectionChanged;
			((IShellController)_shellContext.Shell).AddAppearanceObserver(this, ShellSection);
			ShellSection.PropertyChanged += OnShellItemPropertyChanged;
			foreach (var item in SectionController.GetItems())
			{
				item.PropertyChanged += OnShellContentPropertyChanged;
			}
		}

		void UnhookEvents()
		{
			SectionController.ItemsCollectionChanged -= OnItemsCollectionChanged;
			((IShellController)_shellContext?.Shell)?.RemoveAppearanceObserver(this);
			ShellSection.PropertyChanged -= OnShellItemPropertyChanged;
			foreach (var item in SectionController.GetItems())
			{
				item.PropertyChanged -= OnShellContentPropertyChanged;
			}
		}

		protected virtual void OnPageSelected(int position)
		{
			if (_selecting)
				return;

			var shellSection = ShellSection;
			var visibleItems = SectionController.GetItems();

			// This mainly happens if all of the items that are part of this shell section 
			// vanish. Android calls `OnPageSelected` with position zero even though the view pager is
			// empty
			if (position >= visibleItems.Count)
				return;

			var shellContent = visibleItems[position];

			if (shellContent == shellSection.CurrentItem)
				return;

			var stack = shellSection.Stack.ToList();
			bool result = ShellController.ProposeNavigation(ShellNavigationSource.ShellContentChanged,
				(ShellItem)shellSection.Parent, shellSection, shellContent, stack, true);

			if (result)
			{
				UpdateCurrentItem(shellContent);
			}
			else if (shellSection?.CurrentItem != null)
			{
				var currentPosition = visibleItems.IndexOf(shellSection.CurrentItem);
				_selecting = true;

				// Android doesn't really appreciate you calling SetCurrentItem inside a OnPageSelected callback.
				// It wont crash but the way its programmed doesn't really anticipate re-entrancy around that method
				// and it ends up going to the wrong location. Thus we must invoke.

				_viewPager.Post(() =>
				{
					if (currentPosition < _viewPager.ChildCount && _toolbarTracker != null)
					{
						_viewPager.SetCurrentItem(currentPosition, false);
						UpdateCurrentItem(shellSection.CurrentItem);
					}

					_selecting = false;
				});
			}
		}

		class ViewPagerPageChanged : ViewPager2.OnPageChangeCallback
		{
			private ShellSectionRenderer _shellSectionRenderer;

			public ViewPagerPageChanged(ShellSectionRenderer shellSectionRenderer)
			{
				_shellSectionRenderer = shellSectionRenderer;
			}

			public override void OnPageSelected(int position)
			{
				base.OnPageSelected(position);
				_shellSectionRenderer.OnPageSelected(position);
			}
		}
	}
}