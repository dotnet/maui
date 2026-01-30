#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.AppCompat.Graphics.Drawable;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.AppBar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using ActionBarDrawerToggle = AndroidX.AppCompat.App.ActionBarDrawerToggle;
using ADrawableCompat = AndroidX.Core.Graphics.Drawable.DrawableCompat;
using AndroidResource = Android.Resource;
using ATextView = global::Android.Widget.TextView;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;
using LP = Android.Views.ViewGroup.LayoutParams;
using Paint = Android.Graphics.Paint;
using R = Android.Resource;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellToolbarTracker : Java.Lang.Object, AView.IOnClickListener, IShellToolbarTracker, IFlyoutBehaviorObserver
	{
		const int _placeholderMenuItemId = 100;
		#region IFlyoutBehaviorObserver

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			if (_flyoutBehavior == behavior)
				return;

			_flyoutBehavior = behavior;

			if (Page != null)
				UpdateLeftBarButtonItem();
		}

		#endregion IFlyoutBehaviorObserver

		bool _canNavigateBack;
		bool _disposed;
		DrawerLayout _drawerLayout;
		ActionBarDrawerToggle _drawerToggle;
		FlyoutBehavior _flyoutBehavior = FlyoutBehavior.Flyout;
		Page _page;
		SearchHandler _searchHandler;
		IShellSearchView _searchView;
		protected IShellContext ShellContext { get; private set; }
		//assume the default
		Color _tintColor = null;
		AToolbar _platformToolbar;
		AppBarLayout _appBar;
		float _appBarElevation;
		GenericGlobalLayoutListener _globalLayoutListener;
		DrawerArrowDrawable _drawerArrowDrawable;
		FlyoutIconDrawerDrawable _flyoutIconDrawerDrawable;
		IToolbar _toolbar;
		protected IMauiContext MauiContext => _shell.Handler.MauiContext;

		Toolbar _shellRootToolBar;
		Shell _shell;

		public ShellToolbarTracker(IShellContext shellContext, AToolbar toolbar, DrawerLayout drawerLayout)
		{
			ShellContext = shellContext ?? throw new ArgumentNullException(nameof(shellContext));
			_shell = shellContext.Shell;

			_platformToolbar = toolbar ?? throw new ArgumentNullException(nameof(toolbar));
			_drawerLayout = drawerLayout ?? throw new ArgumentNullException(nameof(drawerLayout));
			_appBar = _platformToolbar.Parent.GetParentOfType<AppBarLayout>();

			_globalLayoutListener = new GenericGlobalLayoutListener((_, _) => UpdateNavBarHasShadow(Page), _appBar);
			_platformToolbar.SetNavigationOnClickListener(this);
			((IShellController)ShellContext.Shell).AddFlyoutBehaviorObserver(this);
			_shellRootToolBar = _shell.Toolbar;
			_shellRootToolBar.PropertyChanged += OnToolbarPropertyChanged;
			ShellContext.Shell.Navigated += OnShellNavigated;
			ShellContext.Shell.PropertyChanged += HandleShellPropertyChanged;
		}

		void IShellToolbarTracker.SetToolbar(IToolbar toolbar)
		{
			_ = toolbar ?? throw new ArgumentNullException(nameof(toolbar));
			_toolbar = toolbar;
		}

		public bool CanNavigateBack
		{
			get
			{
				if (_page?.Navigation?.NavigationStack?.Count > 1)
					return true;

				return _canNavigateBack;
			}
			set
			{
				if (_canNavigateBack == value)
					return;

				_canNavigateBack = value;
			}
		}

		public Page Page
		{
			get { return _page; }
			set
			{
				if (_page == value)
					return;
				var oldPage = _page;
				_page = value;
				OnPageChanged(oldPage, _page);
			}
		}

		public Color TintColor
		{
			get { return _tintColor; }
			set
			{
				_tintColor = value;
				if (Page != null)
				{
					UpdateToolbarItemsTintColors();
					UpdateLeftBarButtonItem();
				}
			}
		}

		protected SearchHandler SearchHandler
		{
			get => _searchHandler;
			set
			{
				if (value == _searchHandler)
					return;

				var oldValue = _searchHandler;
				_searchHandler = value;
				OnSearchHandlerChanged(oldValue, _searchHandler);
			}
		}

		void AView.IOnClickListener.OnClick(AView v)
		{
			var backButtonHandler = Shell.GetBackButtonBehavior(Page);
			var isEnabled = backButtonHandler.GetPropertyIfSet(BackButtonBehavior.IsEnabledProperty, true);

			if (isEnabled)
			{
				if (backButtonHandler?.Command != null)
					backButtonHandler.Command.Execute(backButtonHandler.CommandParameter);
				else if (CanNavigateBack)
					OnNavigateBack();
				else
					_shell.FlyoutIsPresented = !_shell.FlyoutIsPresented;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				_globalLayoutListener.Invalidate();

				if (_backButtonBehavior != null)
					_backButtonBehavior.PropertyChanged -= OnBackButtonBehaviorChanged;

				((IShellController)ShellContext.Shell)?.RemoveFlyoutBehaviorObserver(this);
				_shellRootToolBar.PropertyChanged -= OnToolbarPropertyChanged;
				_shell.Navigated -= OnShellNavigated;
				_shell.PropertyChanged -= HandleShellPropertyChanged;
				UpdateTitleView(ShellContext.AndroidContext, _platformToolbar, null);

				if (_searchView != null)
				{
					_searchView.View.RemoveFromParent();
					_searchView.View.ViewAttachedToWindow -= OnSearchViewAttachedToWindow;
					_searchView.SearchConfirmed -= OnSearchConfirmed;
					_searchView.Dispose();
				}

				_drawerLayout.RemoveDrawerListener(_drawerToggle);
				_drawerToggle?.Dispose();

				_toolbar?.Handler?.DisconnectHandler();
				_toolbar = null;
				_platformToolbar.RemoveAllViews();
			}

			_globalLayoutListener = null;
			_backButtonBehavior = null;
			SearchHandler = null;
			ShellContext = null;
			_drawerToggle = null;
			_searchView = null;
			Page = null;
			_platformToolbar = null;
			_appBar = null;
			_drawerLayout = null;
			_shell = null;

			base.Dispose(disposing);
		}

		protected virtual IShellSearchView GetSearchView(Context context)
		{
			return new ShellSearchView(context, ShellContext);
		}

		protected async virtual void OnNavigateBack()
		{
			try
			{
				await Page.Navigation.PopAsync();
			}
			catch (Exception exc)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<Shell>()?.LogWarning(exc, "Failed to Navigate Back");
			}
		}

		protected virtual void OnPageChanged(Page oldPage, Page newPage)
		{
			if (oldPage != null)
			{
				if (_backButtonBehavior != null)
					_backButtonBehavior.PropertyChanged -= OnBackButtonBehaviorChanged;

				oldPage.PropertyChanged -= OnPagePropertyChanged;
				((INotifyCollectionChanged)oldPage.ToolbarItems).CollectionChanged -= OnPageToolbarItemsChanged;
			}

			if (newPage != null)
			{
				newPage.PropertyChanged += OnPagePropertyChanged;
				_backButtonBehavior = Shell.GetBackButtonBehavior(newPage);

				if (_backButtonBehavior != null)
					_backButtonBehavior.PropertyChanged += OnBackButtonBehaviorChanged;

				((INotifyCollectionChanged)newPage.ToolbarItems).CollectionChanged += OnPageToolbarItemsChanged;

				UpdatePageTitle(_platformToolbar, newPage);
				UpdateLeftBarButtonItem();
				UpdateToolbarItems();
				UpdateNavBarVisible(_platformToolbar, newPage);
				UpdateNavBarHasShadow(newPage);
				UpdateTitleView();

				if (_shell.Toolbar is ShellToolbar shellToolbar &&
					newPage == _shell.GetCurrentShellPage())
				{
					shellToolbar.ApplyChanges();
				}
			}
		}

		void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
		{
			if (_disposed || Page == null)
				return;

			if (_shell?.Toolbar is ShellToolbar &&
				Page == _shell?.GetCurrentShellPage())
			{
				UpdateLeftBarButtonItem();
				UpdateTitleView();
			}
		}

		void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is(Shell.FlyoutIconProperty))
				UpdateLeftBarButtonItem();
		}

		BackButtonBehavior _backButtonBehavior = null;
		protected virtual void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdatePageTitle(_platformToolbar, Page);
			else if (e.PropertyName == Shell.SearchHandlerProperty.PropertyName)
				UpdateToolbarItems();
			else if (e.PropertyName == Shell.NavBarIsVisibleProperty.PropertyName)
				UpdateNavBarVisible(_platformToolbar, Page);
			else if (e.PropertyName == Shell.NavBarHasShadowProperty.PropertyName)
				UpdateNavBarHasShadow(Page);
			else if (e.PropertyName == Shell.BackButtonBehaviorProperty.PropertyName)
			{
				var backButtonHandler = Shell.GetBackButtonBehavior(Page);

				if (_backButtonBehavior != null)
					_backButtonBehavior.PropertyChanged -= OnBackButtonBehaviorChanged;

				UpdateLeftBarButtonItem();

				_backButtonBehavior = backButtonHandler;
				if (_backButtonBehavior != null)
					_backButtonBehavior.PropertyChanged += OnBackButtonBehaviorChanged;
			}
			else if (e.PropertyName == Shell.TitleViewProperty.PropertyName)
				UpdateTitleView();
		}

		void OnBackButtonBehaviorChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!e.Is(BackButtonBehavior.CommandParameterProperty))
				UpdateLeftBarButtonItem();
		}


		protected virtual void OnPageToolbarItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateToolbarItems();
		}

		protected virtual void OnSearchConfirmed(object sender, EventArgs e)
		{
			_platformToolbar.CollapseActionView();
		}

		protected virtual void OnSearchHandlerChanged(SearchHandler oldValue, SearchHandler newValue)
		{
			if (oldValue != null)
			{
				oldValue.PropertyChanged -= OnSearchHandlerPropertyChanged;
			}

			if (newValue != null)
			{
				newValue.PropertyChanged += OnSearchHandlerPropertyChanged;
			}
		}

		protected virtual void OnSearchHandlerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == SearchHandler.SearchBoxVisibilityProperty.PropertyName ||
				e.PropertyName == SearchHandler.IsSearchEnabledProperty.PropertyName)
			{
				UpdateToolbarItems();
			}
		}

		ImageSource GetFlyoutIcon(BackButtonBehavior backButtonHandler, Page page)
		{
			var image = backButtonHandler.GetPropertyIfSet<ImageSource>(BackButtonBehavior.IconOverrideProperty, null);
			if (image == null)
			{
				Element item = page;
				while (!Application.IsApplicationOrNull(item))
				{
					if (item is IShellController shell)
					{
						image = shell.FlyoutIcon;
						break;
					}
					item = item?.Parent;
				}
			}

			return image;
		}

		protected virtual async void UpdateLeftBarButtonItem(Context context, AToolbar toolbar, DrawerLayout drawerLayout, Page page)
		{
			if (_drawerToggle == null)
			{
				_drawerToggle = new ActionBarDrawerToggle(context.GetActivity(), drawerLayout, toolbar, Resource.String.nav_app_bar_open_drawer_description, R.String.Ok)
				{
					ToolbarNavigationClickListener = this,
				};

				// TODO: Obsolete and Remove `UpdateDrawerArrowFromFlyoutIcon`
				// Its original purpose was to set the icon from the FlyoutIcon which is now handled by GetFlyoutIcon below.
				// See: https://github.com/xamarin/Xamarin.Forms/pull/6762
				await UpdateDrawerArrowFromFlyoutIcon(context, _drawerToggle);

				// Fragment might have been disposed while we were awaiting
				if (_disposed)
				{
					return;
				}

				_drawerToggle.DrawerSlideAnimationEnabled = false;
				drawerLayout.AddDrawerListener(_drawerToggle);
			}

			var backButtonHandler = Shell.GetBackButtonBehavior(page);
			var text = backButtonHandler.GetPropertyIfSet(BackButtonBehavior.TextOverrideProperty, String.Empty);
			var command = backButtonHandler.GetPropertyIfSet<ICommand>(BackButtonBehavior.CommandProperty, null);
			var backButtonVisibleFromBehavior = backButtonHandler.GetPropertyIfSet(BackButtonBehavior.IsVisibleProperty, true);
			bool isEnabled = _shell.Toolbar.BackButtonEnabled;
			//Add the FlyoutIcon only if the FlyoutBehavior is Flyout
			var image = _flyoutBehavior == FlyoutBehavior.Flyout ? GetFlyoutIcon(backButtonHandler, page) : null;
			var backButtonVisible = _toolbar.BackButtonVisible;

			DrawerArrowDrawable icon = null;
			bool defaultDrawerArrowDrawable = false;

			var tintColor = Colors.White;
			if (TintColor != null)
				tintColor = TintColor;

			if (image != null)
			{
				FlyoutIconDrawerDrawable fid = toolbar.NavigationIcon as FlyoutIconDrawerDrawable;
				Drawable customIcon;

				if (fid?.IconBitmapSource == image)
				{
					customIcon = fid.IconBitmap;
				}
				else
				{
					customIcon = (await image.GetPlatformImageAsync(MauiContext))?.Value;

					// Fragment might have been disposed while we were waiting for the image drawable
					if (_disposed)
					{
						return;
					}
				}

				if (customIcon != null)
				{
					if (fid == null)
					{
						fid = new FlyoutIconDrawerDrawable(MauiContext.Context, tintColor, customIcon, text);
					}
					else
					{
						fid.TintColor = tintColor;
						fid.IconBitmap = customIcon;
						fid.Text = text;
					}

					fid.IconBitmapSource = image;
					icon = fid;
				}
			}

			if (!string.IsNullOrWhiteSpace(text) && icon == null)
			{
				_flyoutIconDrawerDrawable ??= new FlyoutIconDrawerDrawable(MauiContext.Context, tintColor, null, text);
				icon = _flyoutIconDrawerDrawable;
			}

			if (icon == null && (_flyoutBehavior == FlyoutBehavior.Flyout || CanNavigateBack))
			{
				_drawerArrowDrawable ??= new DrawerArrowDrawable(context.GetThemedContext());
				icon = _drawerArrowDrawable;
				defaultDrawerArrowDrawable = true;
			}

			icon?.Progress = (CanNavigateBack) ? 1 : 0;

			if (command != null || CanNavigateBack)
			{
				_drawerToggle.DrawerIndicatorEnabled = false;

				if (backButtonVisibleFromBehavior && (backButtonVisible || !defaultDrawerArrowDrawable))
					toolbar.NavigationIcon = icon;
			}
			else if (_flyoutBehavior == FlyoutBehavior.Flyout || !defaultDrawerArrowDrawable)
			{
				bool drawerEnabled = isEnabled && icon != null;
				_drawerToggle.DrawerIndicatorEnabled = drawerEnabled;
				if (drawerEnabled)
				{
					_drawerToggle.DrawerArrowDrawable = icon;
				}
				else
				{
					toolbar.NavigationIcon = icon;
				}
			}
			else
			{
				_drawerToggle.DrawerIndicatorEnabled = false;
			}

			_drawerToggle.SyncState();


			//this needs to be set after SyncState
			UpdateToolbarIconAccessibilityText(toolbar, _shell);
			_toolbar?.Handler?.UpdateValue(nameof(Toolbar.IconColor));
		}


		internal static void ApplyToolbarChanges(Toolbar shellToolbar, Toolbar destination)
		{
			// Shell creates a new toolbar for every single screen it's on
			// I don't really know the initial reasoning behind this.
			// So we have to create a unique toolbar class for each platform instance
			// Once we move Shell to use the same handlers and NavigationPage this
			// should all be vastly simplified
			destination.ToolbarItems = shellToolbar.ToolbarItems;
			destination.BarHeight = shellToolbar.BarHeight;
			destination.BackButtonTitle = shellToolbar.BackButtonTitle;
			destination.TitleIcon = shellToolbar.TitleIcon;
			destination.BarBackground = shellToolbar.BarBackground;
			destination.BarTextColor = shellToolbar.BarTextColor;
			destination.IconColor = shellToolbar.IconColor;
			destination.Title = shellToolbar.Title;
			destination.TitleView = shellToolbar.TitleView;
			destination.DynamicOverflowEnabled = shellToolbar.DynamicOverflowEnabled;
			destination.DrawerToggleVisible = shellToolbar.DrawerToggleVisible;
			destination.BackButtonVisible = shellToolbar.BackButtonVisible;
			destination.BackButtonEnabled = shellToolbar.BackButtonEnabled;
			destination.IsVisible = shellToolbar.IsVisible;
		}

		protected virtual Task UpdateDrawerArrow(Context context, AToolbar toolbar, DrawerLayout drawerLayout)
		{
			return Task.CompletedTask;
		}

		protected virtual void UpdateToolbarIconAccessibilityText(AToolbar toolbar, Shell shell)
		{
			var backButtonHandler = Shell.GetBackButtonBehavior(Page);
			var image = GetFlyoutIcon(backButtonHandler, Page);
			var text = backButtonHandler.GetPropertyIfSet(BackButtonBehavior.TextOverrideProperty, String.Empty);
			var automationId = image?.AutomationId ?? text;

			//if AutomationId was specified the user wants to use UITests and interact with FlyoutIcon
			if (!string.IsNullOrEmpty(automationId))
			{
				toolbar.NavigationContentDescription = automationId;
			}
			else if (image == null ||
				toolbar.SetNavigationContentDescription(image) == null)
			{
				if (CanNavigateBack)
					toolbar.SetNavigationContentDescription(Resource.String.nav_app_bar_navigate_up_description);
				else
					toolbar.SetNavigationContentDescription(Resource.String.nav_app_bar_open_drawer_description);
			}
		}

		protected virtual Task UpdateDrawerArrowFromBackButtonBehavior(Context context, AToolbar toolbar, DrawerLayout drawerLayout, BackButtonBehavior backButtonHandler)
		{
			return Task.CompletedTask;
		}

		protected virtual Task UpdateDrawerArrowFromFlyoutIcon(Context context, ActionBarDrawerToggle actionBarDrawerToggle)
		{
			return Task.CompletedTask;
		}

		protected virtual void UpdateMenuItemIcon(Context context, IMenuItem menuItem, ToolbarItem toolBarItem)
		{
			toolBarItem.IconImageSource.LoadImage(MauiContext, finished =>
			{
				var baseDrawable = finished.Value;
				if (baseDrawable != null)
				{
					using (var constant = baseDrawable.GetConstantState())
					using (var newDrawable = constant.NewDrawable())
					using (var iconDrawable = newDrawable.Mutate())
					{
						iconDrawable.SetColorFilter(TintColor.ToPlatform(Colors.White), FilterMode.SrcAtop);
						menuItem.SetIcon(iconDrawable);
					}
				}
			});
		}

		protected virtual void UpdateNavBarVisible(AToolbar toolbar, Page page)
		{
		}

		void UpdateNavBarHasShadow(Page page)
		{
			if (page == null || !_appBar.IsAlive())
				return;

			if (Shell.GetNavBarHasShadow(page))
			{
				if (_appBarElevation <= 0)
					_appBarElevation = _appBar.Context.ToPixels(4);

				_appBar.SetElevation(_appBarElevation);
			}
			else
			{
				// 4 is the default
				_appBar.SetElevation(0f);
			}
		}

		void OnToolbarPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_toolbar != null && _shell?.GetCurrentShellPage() == Page)
			{
				ApplyToolbarChanges((Toolbar)sender, (Toolbar)_toolbar);
				UpdateToolbarIconAccessibilityText(_platformToolbar, _shell);
			}
		}

		protected virtual void UpdatePageTitle(AToolbar toolbar, Page page)
		{
		}

		protected virtual void UpdateTitleView(Context context, AToolbar toolbar, View titleView)
		{
			if (_toolbar != null && _shell?.GetCurrentShellPage() == Page)
				_toolbar.Handler?.UpdateValue(nameof(Toolbar.TitleView));
		}

		private void UpdateToolbarItemsTintColors(AToolbar toolbar)
		{
			var menu = toolbar.Menu;
			if (menu.FindItem(_placeholderMenuItemId) is IMenuItem item)
			{
				using (var icon = item.Icon)
					icon.SetColorFilter(TintColor.ToPlatform(Colors.White), FilterMode.SrcAtop);
			}
		}

		protected virtual void UpdateToolbarItems(AToolbar toolbar, Page page)
		{
			var menu = toolbar.Menu;
			SearchHandler = Shell.GetSearchHandler(page);
			if (SearchHandler != null && SearchHandler.SearchBoxVisibility != SearchBoxVisibility.Hidden)
			{
				var context = ShellContext.AndroidContext;
				if (_searchView == null)
				{
					_searchView = GetSearchView(context);
					_searchView.SearchHandler = SearchHandler;

					_searchView.LoadView();
					_searchView.View.ViewAttachedToWindow += OnSearchViewAttachedToWindow;

					_searchView.View.LayoutParameters = new LP(LP.MatchParent, LP.MatchParent);
					_searchView.SearchConfirmed += OnSearchConfirmed;
				}
				// If the existing _searchView is present but its SearchHandler doesn't match the current page SearchHandler,
				// update it to ensure the correct handler is used for search operations.
				else if (_searchView.SearchHandler != SearchHandler)
				{
					_searchView.SearchHandler = SearchHandler;
					_searchView.LoadView();
				}

				if (SearchHandler.SearchBoxVisibility == SearchBoxVisibility.Collapsible)
				{
					menu.RemoveItem(_placeholderMenuItemId);

					var placeholder = new Java.Lang.String(SearchHandler.Placeholder);
					var item = menu.Add(0, _placeholderMenuItemId, 0, placeholder);
					placeholder.Dispose();

					item.SetEnabled(SearchHandler.IsSearchEnabled);
					item.SetIcon(Resource.Drawable.abc_ic_search_api_material);
					using (var icon = item.Icon)
						icon.SetColorFilter(TintColor.ToPlatform(Colors.White), FilterMode.SrcAtop);
					item.SetShowAsAction(ShowAsAction.IfRoom | ShowAsAction.CollapseActionView);

					if (_searchView.View.Parent != null)
						_searchView.View.RemoveFromParent();

					item.SetActionView(_searchView.View);
					item.Dispose();
				}
				else if (SearchHandler.SearchBoxVisibility == SearchBoxVisibility.Expanded)
				{
					if (_searchView.View.Parent != _platformToolbar)
						_platformToolbar.AddView(_searchView.View);
				}
			}
			else
			{
				if (_searchView != null)
				{
					_searchView.View.RemoveFromParent();
					_searchView.View.ViewAttachedToWindow -= OnSearchViewAttachedToWindow;
					_searchView.SearchConfirmed -= OnSearchConfirmed;
					_searchView.Dispose();
					_searchView = null;
				}
			}

			menu.Dispose();
		}

		void OnSearchViewAttachedToWindow(object sender, AView.ViewAttachedToWindowEventArgs e)
		{
			// We only need to do this tint hack when using collapsed search handlers
			if (SearchHandler.SearchBoxVisibility != SearchBoxVisibility.Collapsible)
				return;

			for (int i = 0; i < _platformToolbar.ChildCount; i++)
			{
				var child = _platformToolbar.GetChildAt(i);
				if (child is AppCompatImageButton button)
				{
					// we want the newly added button which will need layout
					if (child.IsLayoutRequested)
					{
						button.SetColorFilter(TintColor.ToPlatform(Colors.White), PorterDuff.Mode.SrcAtop);
					}

					button.Dispose();
				}
			}
		}

		void UpdateLeftBarButtonItem()
		{
			UpdateLeftBarButtonItem(ShellContext.AndroidContext, _platformToolbar, _drawerLayout, Page);
		}

		void UpdateTitleView()
		{
			UpdateTitleView(ShellContext.AndroidContext, _platformToolbar,
				(_toolbar as Toolbar).TitleView as View ??
				Shell.GetTitleView(Page));
		}

		void UpdateToolbarItems()
		{
			UpdateToolbarItems(_platformToolbar, Page);
		}

		void UpdateToolbarItemsTintColors()
		{
			UpdateToolbarItemsTintColors(_platformToolbar);
		}

		class FlyoutIconDrawerDrawable : DrawerArrowDrawable
		{
			public Drawable IconBitmap { get; set; }
			public string Text { get; set; }
			public Color TintColor { get; set; }
			public ImageSource IconBitmapSource { get; set; }
			float _defaultSize;

			Color _pressedBackgroundColor => TintColor.AddLuminosity(-.12f);//<item name="highlight_alpha_material_light" format="float" type="dimen">0.12</item>

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing && IconBitmap != null)
				{
					IconBitmap.Dispose();
				}
			}

			public FlyoutIconDrawerDrawable(Context context, Color defaultColor, Drawable icon, string text) : base(context)
			{
				TintColor = defaultColor;
				if (context.TryResolveAttribute(AndroidResource.Attribute.TextSize, out float? value) &&
					value != null)
				{
					_defaultSize = value.Value;
				}
				else
				{
					_defaultSize = 50;
				}

				IconBitmap = icon;
				Text = text;
			}

			public override void Draw(Canvas canvas)
			{
				bool pressed = false;
				if (IconBitmap != null)
				{
					ADrawableCompat.SetTint(IconBitmap, TintColor.ToPlatform());
					ADrawableCompat.SetTintMode(IconBitmap, PorterDuff.Mode.SrcAtop);

					IconBitmap.SetBounds(Bounds.Left, Bounds.Top, Bounds.Right, Bounds.Bottom);
					IconBitmap.Draw(canvas);
				}
				else if (!string.IsNullOrEmpty(Text))
				{
					var paint = new Paint { AntiAlias = true };
					paint.TextSize = _defaultSize;
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-android/issues/6962
					paint.Color = pressed ? _pressedBackgroundColor.ToPlatform() : TintColor.ToPlatform();
#pragma warning restore CA1416
					paint.SetStyle(Paint.Style.Fill);
					var y = (Bounds.Height() + paint.TextSize) / 2;
					canvas.DrawText(Text, 0, y, paint);
				}
			}
		}
	}
}
