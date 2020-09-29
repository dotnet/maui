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
using Xamarin.Forms.Internals;
using AColor = Android.Graphics.Color;
using ActionBarDrawerToggle = AndroidX.AppCompat.App.ActionBarDrawerToggle;
using ADrawableCompat = AndroidX.Core.Graphics.Drawable.DrawableCompat;
using ATextView = global::Android.Widget.TextView;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;
using R = Android.Resource;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellToolbarTracker : Java.Lang.Object, AView.IOnClickListener, IShellToolbarTracker, IFlyoutBehaviorObserver
	{
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
		ContainerView _titleViewContainer;
		IShellContext _shellContext;
		//assume the default
		Color _tintColor = Color.Default;
		Toolbar _toolbar;
		AppBarLayout _appBar;
		float _appBarElevation;
		GenericGlobalLayoutListener _globalLayoutListener;
		List<IMenuItem> _currentMenuItems = new List<IMenuItem>();
		List<ToolbarItem> _currentToolbarItems = new List<ToolbarItem>();

		public ShellToolbarTracker(IShellContext shellContext, Toolbar toolbar, DrawerLayout drawerLayout)
		{
			_shellContext = shellContext ?? throw new ArgumentNullException(nameof(shellContext));
			_toolbar = toolbar ?? throw new ArgumentNullException(nameof(toolbar));
			_drawerLayout = drawerLayout ?? throw new ArgumentNullException(nameof(drawerLayout));
			_appBar = _toolbar.Parent.GetParentOfType<AppBarLayout>();

			_globalLayoutListener = new GenericGlobalLayoutListener(() => UpdateNavBarHasShadow(Page));
			_appBar.ViewTreeObserver.AddOnGlobalLayoutListener(_globalLayoutListener);
			_toolbar.SetNavigationOnClickListener(this);
			((IShellController)_shellContext.Shell).AddFlyoutBehaviorObserver(this);
		}

		public bool CanNavigateBack
		{
			get { return _canNavigateBack; }
			set
			{
				if (_canNavigateBack == value)
					return;
				_canNavigateBack = value;
				UpdateLeftBarButtonItem();
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
					UpdateToolbarItems();
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
					_shellContext.Shell.FlyoutIsPresented = !_shellContext.Shell.FlyoutIsPresented;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (_appBar.IsAlive() && _appBar.ViewTreeObserver.IsAlive())
					_appBar.ViewTreeObserver.RemoveOnGlobalLayoutListener(_globalLayoutListener);

				_globalLayoutListener.Invalidate();

				if (_backButtonBehavior != null)
					_backButtonBehavior.PropertyChanged -= OnBackButtonBehaviorChanged;

				_toolbar.DisposeMenuItems(_currentToolbarItems, OnToolbarItemPropertyChanged);

				((IShellController)_shellContext.Shell)?.RemoveFlyoutBehaviorObserver(this);

				UpdateTitleView(_shellContext.AndroidContext, _toolbar, null);

				if (_searchView != null)
				{
					_searchView.View.RemoveFromParent();
					_searchView.View.ViewAttachedToWindow -= OnSearchViewAttachedToWindow;
					_searchView.SearchConfirmed -= OnSearchConfirmed;
					_searchView.Dispose();
				}

				_currentMenuItems?.Clear();
				_currentToolbarItems?.Clear();

				_drawerToggle?.Dispose();
			}

			_currentMenuItems = null;
			_currentToolbarItems = null;
			_globalLayoutListener = null;
			_backButtonBehavior = null;
			SearchHandler = null;
			_shellContext = null;
			_drawerToggle = null;
			_searchView = null;
			Page = null;
			_toolbar = null;
			_appBar = null;
			_drawerLayout = null;

			base.Dispose(disposing);
		}

		protected virtual IShellSearchView GetSearchView(Context context)
		{
			return new ShellSearchView(context, _shellContext);
		}

		protected async virtual void OnNavigateBack()
		{
			try
			{
				await Page.Navigation.PopAsync();
			}
			catch (Exception exc)
			{
				Internals.Log.Warning(nameof(Shell), $"Failed to Navigate Back: {exc}");
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

				UpdatePageTitle(_toolbar, newPage);
				UpdateLeftBarButtonItem();
				UpdateToolbarItems();
				UpdateNavBarVisible(_toolbar, newPage);
				UpdateNavBarHasShadow(newPage);
				UpdateTitleView();
			}
		}

		BackButtonBehavior _backButtonBehavior = null;
		protected virtual void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdatePageTitle(_toolbar, Page);
			else if (e.PropertyName == Shell.SearchHandlerProperty.PropertyName)
				UpdateToolbarItems();
			else if (e.PropertyName == Shell.NavBarIsVisibleProperty.PropertyName)
				UpdateNavBarVisible(_toolbar, Page);
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
			_toolbar.CollapseActionView();
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

		protected virtual async void UpdateLeftBarButtonItem(Context context, Toolbar toolbar, DrawerLayout drawerLayout, Page page)
		{
			if (_drawerToggle == null && !context.IsDesignerContext())
			{
				_drawerToggle = new ActionBarDrawerToggle(context.GetActivity(), drawerLayout, toolbar, R.String.Ok, R.String.Ok)
				{
					ToolbarNavigationClickListener = this,
				};

				await UpdateDrawerArrowFromFlyoutIcon(context, _drawerToggle);

				_drawerToggle.DrawerSlideAnimationEnabled = false;
				drawerLayout.AddDrawerListener(_drawerToggle);
			}

			var backButtonHandler = Shell.GetBackButtonBehavior(page);
			var text = backButtonHandler.GetPropertyIfSet(BackButtonBehavior.TextOverrideProperty, String.Empty);
			var command = backButtonHandler.GetPropertyIfSet<ICommand>(BackButtonBehavior.CommandProperty, null);
			bool isEnabled = _backButtonBehavior.GetPropertyIfSet(BackButtonBehavior.IsEnabledProperty, true);
			var image = GetFlyoutIcon(backButtonHandler, page);

			DrawerArrowDrawable icon = null;
			bool defaultDrawerArrowDrawable = false;

			var tintColor = Color.White;
			if (TintColor != Color.Default)
				tintColor = TintColor;

			if (image != null)
			{
				FlyoutIconDrawerDrawable fid = toolbar.NavigationIcon as FlyoutIconDrawerDrawable;
				Drawable customIcon;

				if (fid?.IconBitmapSource == image)
					customIcon = fid.IconBitmap;
				else
					customIcon = await context.GetFormsDrawableAsync(image);

				if (customIcon != null)
				{
					if (fid == null)
					{
						fid = new FlyoutIconDrawerDrawable(context, tintColor, customIcon, text);
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
				icon = new FlyoutIconDrawerDrawable(context, tintColor, null, text);
			}

			if (icon == null && (_flyoutBehavior == FlyoutBehavior.Flyout || CanNavigateBack))
			{
				icon = new DrawerArrowDrawable(context.GetThemedContext());
				icon.SetColorFilter(tintColor, FilterMode.SrcAtop);
				defaultDrawerArrowDrawable = true;
			}

			if (icon != null)
				icon.Progress = (CanNavigateBack) ? 1 : 0;

			if (command != null || CanNavigateBack)
			{
				_drawerToggle.DrawerIndicatorEnabled = false;
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
			UpdateToolbarIconAccessibilityText(toolbar, _shellContext.Shell);
		}


		protected virtual Task UpdateDrawerArrow(Context context, Toolbar toolbar, DrawerLayout drawerLayout)
		{
			return Task.CompletedTask;
		}

		protected virtual void UpdateToolbarIconAccessibilityText(Toolbar toolbar, Shell shell)
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
				toolbar.SetNavigationContentDescription(R.String.Ok);
			}
		}

		protected virtual Task UpdateDrawerArrowFromBackButtonBehavior(Context context, Toolbar toolbar, DrawerLayout drawerLayout, BackButtonBehavior backButtonHandler)
		{
			return Task.CompletedTask;
		}

		protected virtual Task UpdateDrawerArrowFromFlyoutIcon(Context context, ActionBarDrawerToggle actionBarDrawerToggle)
		{
			return Task.CompletedTask;
		}

		protected virtual void UpdateMenuItemIcon(Context context, IMenuItem menuItem, ToolbarItem toolBarItem)
		{
			_shellContext.ApplyDrawableAsync(toolBarItem, ToolbarItem.IconImageSourceProperty, baseDrawable =>
			{
				if (baseDrawable != null)
				{
					using (var constant = baseDrawable.GetConstantState())
					using (var newDrawable = constant.NewDrawable())
					using (var iconDrawable = newDrawable.Mutate())
					{
						iconDrawable.SetColorFilter(TintColor.ToAndroid(Color.White), FilterMode.SrcAtop);
						menuItem.SetIcon(iconDrawable);
					}
				}
			});
		}

		protected virtual void UpdateNavBarVisible(Toolbar toolbar, Page page)
		{
			var navBarVisible = Shell.GetNavBarIsVisible(page);
			toolbar.Visibility = navBarVisible ? ViewStates.Visible : ViewStates.Gone;
		}

		void UpdateNavBarHasShadow(Page page)
		{
			if (page == null || !_appBar.IsAlive())
				return;

			if (Shell.GetNavBarHasShadow(page))
			{
				if (_appBarElevation > 0)
					_appBar.SetElevation(_appBarElevation);
			}
			else
			{
				// 4 is the default
				_appBarElevation = _appBar.Context.ToPixels(4);
				_appBar.SetElevation(0f);
			}
		}

		protected virtual void UpdatePageTitle(Toolbar toolbar, Page page)
		{
			_toolbar.Title = page.Title;
		}

		protected virtual void UpdateTitleView(Context context, Toolbar toolbar, View titleView)
		{
			if (titleView == null)
			{
				if (_titleViewContainer != null)
				{
					_titleViewContainer.RemoveFromParent();
					_titleViewContainer.Dispose();
					_titleViewContainer = null;
				}
			}
			else if (_titleViewContainer == null)
			{
				_titleViewContainer = new ContainerView(context, titleView);
				_titleViewContainer.MatchHeight = _titleViewContainer.MatchWidth = true;
				_titleViewContainer.LayoutParameters = new Toolbar.LayoutParams(LP.MatchParent, LP.MatchParent)
				{
					LeftMargin = (int)context.ToPixels(titleView.Margin.Left),
					TopMargin = (int)context.ToPixels(titleView.Margin.Top),
					RightMargin = (int)context.ToPixels(titleView.Margin.Right),
					BottomMargin = (int)context.ToPixels(titleView.Margin.Bottom)
				};

				_toolbar.AddView(_titleViewContainer);
			}
			else
			{
				_titleViewContainer.View = titleView;
			}
		}

		protected virtual void UpdateToolbarItems(Toolbar toolbar, Page page)
		{
			var menu = toolbar.Menu;
			var sortedItems = page.ToolbarItems.OrderBy(x => x.Order);

			toolbar.UpdateMenuItems(sortedItems, _shellContext.AndroidContext, TintColor, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems);

			SearchHandler = Shell.GetSearchHandler(page);
			if (SearchHandler != null && SearchHandler.SearchBoxVisibility != SearchBoxVisibility.Hidden)
			{
				var context = _shellContext.AndroidContext;
				if (_searchView == null)
				{
					_searchView = GetSearchView(context);
					_searchView.SearchHandler = SearchHandler;

					_searchView.LoadView();
					_searchView.View.ViewAttachedToWindow += OnSearchViewAttachedToWindow;

					_searchView.View.LayoutParameters = new LP(LP.MatchParent, LP.MatchParent);
					_searchView.SearchConfirmed += OnSearchConfirmed;
				}

				if (SearchHandler.SearchBoxVisibility == SearchBoxVisibility.Collapsible)
				{
					var placeholder = new Java.Lang.String(SearchHandler.Placeholder);
					var item = menu.Add(placeholder);
					placeholder.Dispose();

					item.SetEnabled(SearchHandler.IsSearchEnabled);
					item.SetIcon(Resource.Drawable.abc_ic_search_api_material);
					using (var icon = item.Icon)
						icon.SetColorFilter(TintColor.ToAndroid(Color.White), FilterMode.SrcAtop);
					item.SetShowAsAction(ShowAsAction.IfRoom | ShowAsAction.CollapseActionView);

					if (_searchView.View.Parent != null)
						_searchView.View.RemoveFromParent();

					_searchView.ShowKeyboardOnAttached = true;
					item.SetActionView(_searchView.View);
					item.Dispose();
				}
				else if (SearchHandler.SearchBoxVisibility == SearchBoxVisibility.Expanded)
				{
					_searchView.ShowKeyboardOnAttached = false;
					if (_searchView.View.Parent != _toolbar)
						_toolbar.AddView(_searchView.View);
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

		void OnToolbarItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var sortedItems = Page.ToolbarItems.OrderBy(x => x.Order).ToList();
			_toolbar.OnToolbarItemPropertyChanged(e, (ToolbarItem)sender, sortedItems, _shellContext.AndroidContext, TintColor, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems);
		}

		void OnSearchViewAttachedToWindow(object sender, AView.ViewAttachedToWindowEventArgs e)
		{
			// We only need to do this tint hack when using collapsed search handlers
			if (SearchHandler.SearchBoxVisibility != SearchBoxVisibility.Collapsible)
				return;

			for (int i = 0; i < _toolbar.ChildCount; i++)
			{
				var child = _toolbar.GetChildAt(i);
				if (child is AppCompatImageButton button)
				{
					// we want the newly added button which will need layout
					if (child.IsLayoutRequested)
					{
						button.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
					}

					button.Dispose();
				}
			}
		}

		void UpdateLeftBarButtonItem()
		{
			UpdateLeftBarButtonItem(_shellContext.AndroidContext, _toolbar, _drawerLayout, Page);
		}

		void UpdateTitleView()
		{
			UpdateTitleView(_shellContext.AndroidContext, _toolbar, Shell.GetTitleView(Page));
		}

		void UpdateToolbarItems()
		{
			UpdateToolbarItems(_toolbar, Page);
		}

		class FlyoutIconDrawerDrawable : DrawerArrowDrawable
		{
			public Drawable IconBitmap { get; set; }
			public string Text { get; set; }
			public Color TintColor { get; set; }
			public ImageSource IconBitmapSource { get; set; }
			float _defaultSize;

			Color _pressedBackgroundColor => TintColor.AddLuminosity(-.12);//<item name="highlight_alpha_material_light" format="float" type="dimen">0.12</item>

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
				_defaultSize = Forms.GetFontSizeNormal(context);
				IconBitmap = icon;
				Text = text;
			}

			public override void Draw(Canvas canvas)
			{
				bool pressed = false;
				if (IconBitmap != null)
				{
					ADrawableCompat.SetTint(IconBitmap, TintColor.ToAndroid());
					ADrawableCompat.SetTintMode(IconBitmap, PorterDuff.Mode.SrcAtop);

					IconBitmap.SetBounds(Bounds.Left, Bounds.Top, Bounds.Right, Bounds.Bottom);
					IconBitmap.Draw(canvas);
				}
				else if (!string.IsNullOrEmpty(Text))
				{
					var paint = new Paint { AntiAlias = true };
					paint.TextSize = _defaultSize;
					paint.Color = pressed ? _pressedBackgroundColor.ToAndroid() : TintColor.ToAndroid();
					paint.SetStyle(Paint.Style.Fill);
					var y = (Bounds.Height() + paint.TextSize) / 2;
					canvas.DrawText(Text, 0, y, paint);
				}
			}
		}
	}
}