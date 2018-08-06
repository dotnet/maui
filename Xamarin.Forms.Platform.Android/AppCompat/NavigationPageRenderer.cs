using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Support.V7.Graphics.Drawable;
using Android.Util;
using Android.Views;
using Xamarin.Forms.Internals;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;
using AView = Android.Views.View;
using AToolbar = Android.Support.V7.Widget.Toolbar;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
using Object = Java.Lang.Object;
using static Xamarin.Forms.PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage;
using static Android.Views.View;
using System.IO;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	public class NavigationPageRenderer : VisualElementRenderer<NavigationPage>, IManageFragments, IOnClickListener, ILifeCycleState
	{
		readonly List<Fragment> _fragmentStack = new List<Fragment>();

		Drawable _backgroundDrawable;
		Page _current;

		bool _disposed;
		ActionBarDrawerToggle _drawerToggle;
		FragmentManager _fragmentManager;
		int _lastActionBarHeight = -1;
		int _statusbarHeight;
		AToolbar _toolbar;
		ToolbarTracker _toolbarTracker;
		DrawerMultiplexedListener _drawerListener;
		DrawerLayout _drawerLayout;
		MasterDetailPage _masterDetailPage;
		bool _toolbarVisible;
		IVisualElementRenderer _titleViewRenderer;
		Container _titleView;
		ImageView _titleIconView;
		ImageSource _imageSource;
		bool _isAttachedToWindow;
		bool _didInitialPushPages;


		// The following is based on https://android.googlesource.com/platform/frameworks/support.git/+/4a7e12af4ec095c3a53bb8481d8d92f63157c3b7/v4/java/android/support/v4/app/FragmentManager.java#677
		// Must be overriden in a custom renderer to match durations in XML animation resource files
		protected virtual int TransitionDuration { get; set; } = 220;
		bool ILifeCycleState.MarkedForDispose { get; set; } = false;

		public NavigationPageRenderer(Context context) : base(context)
		{
			AutoPackage = false;
			Id = Platform.GenerateViewId();
			Device.Info.PropertyChanged += DeviceInfoPropertyChanged;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use NavigationPageRenderer(Context) instead.")]
		public NavigationPageRenderer()
		{
			AutoPackage = false;
			Id = Platform.GenerateViewId();
			Device.Info.PropertyChanged += DeviceInfoPropertyChanged;
		}

		internal int ContainerTopPadding { get; set; }
		internal int ContainerBottomPadding { get; set; }

		Page Current
		{
			get { return _current; }
			set
			{
				if (_current == value)
					return;

				if (_current != null)
					_current.PropertyChanged -= CurrentOnPropertyChanged;

				_current = value;

				if (_current != null)
				{
					_current.PropertyChanged += CurrentOnPropertyChanged;
					ToolbarVisible = NavigationPage.GetHasNavigationBar(_current);
				}
			}
		}

		FragmentManager FragmentManager => _fragmentManager ?? (_fragmentManager = ((FormsAppCompatActivity)Context).SupportFragmentManager);

		IPageController PageController => Element;

		bool ToolbarVisible
		{
			get { return _toolbarVisible; }
			set
			{
				if (_toolbarVisible == value)
					return;
				_toolbarVisible = value;
				RequestLayout();
			}
		}

		void IManageFragments.SetFragmentManager(FragmentManager childFragmentManager)
		{
			if (_fragmentManager == null)
				_fragmentManager = childFragmentManager;
		}

		public Task<bool> PopToRootAsync(Page page, bool animated = true)
		{
			return OnPopToRootAsync(page, animated);
		}

		public Task<bool> PopViewAsync(Page page, bool animated = true)
		{
			return OnPopViewAsync(page, animated);
		}

		public Task<bool> PushViewAsync(Page page, bool animated = true)
		{
			return OnPushAsync(page, animated);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;

				if (_titleViewRenderer != null)
				{
					Android.Platform.ClearRenderer(_titleViewRenderer.View);
					_titleViewRenderer.Dispose();
					_titleViewRenderer = null;
				}

				_toolbar.RemoveView(_titleView);
				_titleView?.Dispose();
				_titleView = null;

				_toolbar.RemoveView(_titleIconView);
				_titleIconView?.Dispose();
				_titleIconView = null;

				_imageSource = null;

				if (_toolbarTracker != null)
				{
					_toolbarTracker.CollectionChanged -= ToolbarTrackerOnCollectionChanged;
					_toolbarTracker.Target = null;
					_toolbarTracker = null;
				}

				if (_toolbar != null)
				{
					_toolbar.SetNavigationOnClickListener(null);
					_toolbar.Dispose();
					_toolbar = null;
				}

				if (_drawerLayout != null && _drawerListener != null)
				{
					_drawerLayout.RemoveDrawerListener(_drawerListener);
				}

				if (_drawerListener != null)
				{
					_drawerListener.Dispose();
					_drawerListener = null;
				}

				if (_drawerToggle != null)
				{
					_drawerToggle.Dispose();
					_drawerToggle = null;
				}

				if (_backgroundDrawable != null)
				{
					_backgroundDrawable.Dispose();
					_backgroundDrawable = null;
				}

				Current = null;

				// We dispose the child renderers after cleaning up everything related to DrawerLayout in case
				// one of the children is a MasterDetailPage (which may dispose of the DrawerLayout).
				if (Element != null)
				{
					foreach (Element element in PageController.InternalChildren)
					{
						var child = element as VisualElement;
						if (child == null)
							continue;

						IVisualElementRenderer renderer = Android.Platform.GetRenderer(child);
						renderer?.Dispose();
					}

					var navController = (INavigationPageController)Element;

					navController.PushRequested -= OnPushed;
					navController.PopRequested -= OnPopped;
					navController.PopToRootRequested -= OnPoppedToRoot;
					navController.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;
					navController.RemovePageRequested -= OnRemovePageRequested;
				}

				Device.Info.PropertyChanged -= DeviceInfoPropertyChanged;

				// API only exists on newer android YAY
				if ((int)Build.VERSION.SdkInt >= 17)
				{
					FragmentManager fm = FragmentManager;

					if (!fm.IsDestroyed)
					{
						FragmentTransaction trans = fm.BeginTransactionEx();
						foreach (Fragment fragment in _fragmentStack)
							trans.RemoveEx(fragment);
						trans.CommitAllowingStateLossEx();
						fm.ExecutePendingTransactionsEx();
					}
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			PageController.SendAppearing();

			// If the Appearing handler changed the application's main page for some reason,
			// this page may no longer be part of the hierarchy; if so, we need to skip
			// updating the toolbar and pushing the pages to avoid crashing the app
			if (!Element.IsAttachedToRoot())
				return;

			RegisterToolbar();

			// If there is already stuff on the stack we need to push it
			PushCurrentPages();

			UpdateToolbar();
			_isAttachedToWindow = true;
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			PageController.SendDisappearing();
			_isAttachedToWindow = false;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				var oldNavController = (INavigationPageController)e.OldElement;

				oldNavController.PushRequested -= OnPushed;
				oldNavController.PopRequested -= OnPopped;
				oldNavController.PopToRootRequested -= OnPoppedToRoot;
				oldNavController.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;
				oldNavController.RemovePageRequested -= OnRemovePageRequested;

				RemoveAllViews();
				if (_toolbar != null)
					AddView(_toolbar);
			}

			if (e.NewElement != null)
			{
				if (_toolbarTracker == null)
				{
					SetupToolbar();
					_toolbarTracker = new ToolbarTracker();
					_toolbarTracker.CollectionChanged += ToolbarTrackerOnCollectionChanged;
				}

				var parents = new List<Page>();
				Page root = Element;
				while (!Application.IsApplicationOrNull(root.RealParent))
				{
					root = (Page)root.RealParent;
					parents.Add(root);
				}

				_toolbarTracker.Target = e.NewElement;
				_toolbarTracker.AdditionalTargets = parents;
				UpdateMenu();

				var navController = (INavigationPageController)e.NewElement;

				navController.PushRequested += OnPushed;
				navController.PopRequested += OnPopped;
				navController.PopToRootRequested += OnPoppedToRoot;
				navController.InsertPageBeforeRequested += OnInsertPageBeforeRequested;
				navController.RemovePageRequested += OnRemovePageRequested;

				if (_isAttachedToWindow && Element.IsAttachedToRoot())
				{
					PushCurrentPages();
				}
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == NavigationPage.BarBackgroundColorProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == NavigationPage.BarTextColorProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == NavigationPage.BackButtonTitleProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == BarHeightProperty.PropertyName)
				UpdateToolbar();
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			AToolbar bar = _toolbar;
			// make sure bar stays on top of everything
			bar.BringToFront();

			base.OnLayout(changed, l, t, r, b);

			int barHeight = ActionBarHeight();

			if (Element.IsSet(BarHeightProperty))
				barHeight = Element.OnThisPlatform().GetBarHeight();

			if (barHeight != _lastActionBarHeight && _lastActionBarHeight > 0)
			{
				ResetToolbar();
				bar = _toolbar;
			}
			_lastActionBarHeight = barHeight;

			bar.Measure(MeasureSpecFactory.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly), MeasureSpecFactory.MakeMeasureSpec(barHeight, MeasureSpecMode.Exactly));

			var barOffset = ToolbarVisible ? barHeight : 0;
			int containerHeight = b - t - ContainerTopPadding - barOffset - ContainerBottomPadding;

			PageController.ContainerArea = new Rectangle(0, 0, Context.FromPixels(r - l), Context.FromPixels(containerHeight));

			// Potential for optimization here, the exact conditions by which you don't need to do this are complex
			// and the cost of doing when it's not needed is moderate to low since the layout will short circuit pretty fast
			Element.ForceLayout();

			bool toolbarLayoutCompleted = false;
			for (var i = 0; i < ChildCount; i++)
			{
				AView child = GetChildAt(i);

				Page childPage = (child as PageContainer)?.Child?.Element as Page;

				if (childPage == null)
					return;

				// We need to base the layout of both the child and the bar on the presence of the NavBar on the child Page itself.
				// If we layout the bar based on ToolbarVisible, we get a white bar flashing at the top of the screen.
				// If we layout the child based on ToolbarVisible, we get a white bar flashing at the bottom of the screen.
				bool childHasNavBar = NavigationPage.GetHasNavigationBar(childPage);

				if (childHasNavBar)
				{
					bar.Layout(0, 0, r - l, barHeight);
					child.Layout(0, barHeight + ContainerTopPadding, r, b - ContainerBottomPadding);
				}
				else
				{
					bar.Layout(0, -1000, r, barHeight - 1000);
					child.Layout(0, ContainerTopPadding, r, b - ContainerBottomPadding);
				}
				toolbarLayoutCompleted = true;
			}

			// Making the layout of the toolbar dependant on having a child Page could potentially mean that the toolbar is not laid out.
			// We'll do one more check to make sure it isn't missed.
			if (!toolbarLayoutCompleted)
			{
				if (ToolbarVisible)
				{
					bar.Layout(0, 0, r - l, barHeight);
				}
				else
				{
					bar.Layout(0, -1000, r, barHeight - 1000);
				}
			}
		}

		protected virtual void SetupPageTransition(FragmentTransaction transaction, bool isPush)
		{
			if (isPush)
				transaction.SetTransitionEx((int)FragmentTransit.FragmentOpen);
			else
				transaction.SetTransitionEx((int)FragmentTransit.FragmentClose);
		}

		internal int GetNavBarHeight()
		{
			if (!ToolbarVisible)
				return 0;

			return ActionBarHeight();
		}

		int ActionBarHeight()
		{
			int attr = Resource.Attribute.actionBarSize;

			int actionBarHeight;
			using (var tv = new TypedValue())
			{
				actionBarHeight = 0;
				if (Context.Theme.ResolveAttribute(attr, tv, true))
					actionBarHeight = TypedValue.ComplexToDimensionPixelSize(tv.Data, Resources.DisplayMetrics);
			}

			if (actionBarHeight <= 0)
				return Device.Info.CurrentOrientation.IsPortrait() ? (int)Context.ToPixels(56) : (int)Context.ToPixels(48);

			if (((Activity)Context).Window.Attributes.Flags.HasFlag(WindowManagerFlags.TranslucentStatus) || ((Activity)Context).Window.Attributes.Flags.HasFlag(WindowManagerFlags.TranslucentNavigation))
			{
				if (_toolbar.PaddingTop == 0)
					_toolbar.SetPadding(0, GetStatusBarHeight(), 0, 0);

				return actionBarHeight + GetStatusBarHeight();
			}

			return actionBarHeight;
		}

		void AnimateArrowIn()
		{
			var icon = _toolbar.NavigationIcon as DrawerArrowDrawable;
			if (icon == null)
				return;

			ValueAnimator valueAnim = ValueAnimator.OfFloat(0, 1);
			valueAnim.SetDuration(200);
			valueAnim.Update += (s, a) => icon.Progress = (float)a.Animation.AnimatedValue;
			valueAnim.Start();
		}

		int GetStatusBarHeight()
		{
			if (_statusbarHeight > 0)
				return _statusbarHeight;

			int resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
			if (resourceId > 0)
				_statusbarHeight = Resources.GetDimensionPixelSize(resourceId);

			return _statusbarHeight;
		}

		void AnimateArrowOut()
		{
			var icon = _toolbar.NavigationIcon as DrawerArrowDrawable;
			if (icon == null)
				return;

			ValueAnimator valueAnim = ValueAnimator.OfFloat(1, 0);
			valueAnim.SetDuration(200);
			valueAnim.Update += (s, a) => icon.Progress = (float)a.Animation.AnimatedValue;
			valueAnim.Start();
		}

		public void OnClick(AView v)
		{
			Element?.PopAsync();
		}

		void CurrentOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == NavigationPage.HasNavigationBarProperty.PropertyName)
				ToolbarVisible = NavigationPage.GetHasNavigationBar(Current);
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == NavigationPage.HasBackButtonProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == NavigationPage.TitleIconProperty.PropertyName ||
					 e.PropertyName == NavigationPage.TitleViewProperty.PropertyName)
				UpdateToolbar();
		}

#pragma warning disable 1998 // considered for removal
		async void DeviceInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
#pragma warning restore 1998
		{
			if (nameof(Device.Info.CurrentOrientation) == e.PropertyName)
				ResetToolbar();
		}

		protected virtual void OnToolbarItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == MenuItem.IsEnabledProperty.PropertyName || e.PropertyName == MenuItem.TextProperty.PropertyName || e.PropertyName == MenuItem.IconProperty.PropertyName)
				UpdateMenu();
		}

		void InsertPageBefore(Page page, Page before)
		{
			if (!_isAttachedToWindow)
				PushCurrentPages();

			UpdateToolbar();

			int index = PageController.InternalChildren.IndexOf(before);
			if (index == -1)
				throw new InvalidOperationException("This should never happen, please file a bug");

			Fragment fragment = FragmentContainer.CreateInstance(page);
			_fragmentStack.Insert(index, fragment);
		}

		void OnInsertPageBeforeRequested(object sender, NavigationRequestedEventArgs e)
		{
			InsertPageBefore(e.Page, e.BeforePage);
		}

		void OnPopped(object sender, NavigationRequestedEventArgs e)
		{
			e.Task = PopViewAsync(e.Page, e.Animated);
		}

		void OnPoppedToRoot(object sender, NavigationRequestedEventArgs e)
		{
			e.Task = PopToRootAsync(e.Page, e.Animated);
		}

		protected virtual Task<bool> OnPopToRootAsync(Page page, bool animated)
		{
			return SwitchContentAsync(page, animated, true, true);
		}

		protected virtual Task<bool> OnPopViewAsync(Page page, bool animated)
		{
			Page pageToShow = ((INavigationPageController)Element).Peek(1);
			if (pageToShow == null)
				return Task.FromResult(false);

			return SwitchContentAsync(pageToShow, animated, true);
		}

		protected virtual Task<bool> OnPushAsync(Page view, bool animated)
		{
			return SwitchContentAsync(view, animated);
		}

		void OnPushed(object sender, NavigationRequestedEventArgs e)
		{
			e.Task = PushViewAsync(e.Page, e.Animated);
		}

		void OnRemovePageRequested(object sender, NavigationRequestedEventArgs e)
		{
			RemovePage(e.Page);
		}

		void RegisterToolbar()
		{
			Context context = Context;
			AToolbar bar = _toolbar;
			Element page = Element.RealParent;

			_masterDetailPage = null;
			while (page != null)
			{
				if (page is MasterDetailPage)
				{
					_masterDetailPage = page as MasterDetailPage;
					break;
				}
				page = page.RealParent;
			}

			if (_masterDetailPage == null)
			{
				_masterDetailPage = PageController.InternalChildren[0] as MasterDetailPage;
				if (_masterDetailPage == null)
					return;
			}

			if (((IMasterDetailPageController)_masterDetailPage).ShouldShowSplitMode)
				return;

			var renderer = Android.Platform.GetRenderer(_masterDetailPage) as MasterDetailPageRenderer;
			if (renderer == null)
				return;

			_drawerLayout = renderer;
			_drawerToggle = new ActionBarDrawerToggle((Activity)context, _drawerLayout, bar, global::Android.Resource.String.Ok, global::Android.Resource.String.Ok)
			{
				ToolbarNavigationClickListener = new ClickListener(Element)
			};

			if (_drawerListener != null)
			{
				_drawerLayout.RemoveDrawerListener(_drawerListener);
			}

			_drawerListener = new DrawerMultiplexedListener { Listeners = { _drawerToggle, renderer } };
			_drawerLayout.AddDrawerListener(_drawerListener);
		}

		Fragment GetPageFragment(Page page)
		{
			for (int n = 0; n < _fragmentStack.Count; n++)
			{
				if ((_fragmentStack[n] as FragmentContainer)?.Page == page)
				{
					return _fragmentStack[n];
				}
			}

			return null;
		}

		void RemovePage(Page page)
		{
			if (!_isAttachedToWindow)
				PushCurrentPages();

			Fragment fragment = GetPageFragment(page);

			if (fragment == null)
			{
				return;
			}

#if DEBUG
			// Enables logging of moveToState operations to logcat
			FragmentManager.EnableDebugLogging(true);
#endif

			// Go ahead and take care of the fragment bookkeeping for the page being removed
			FragmentTransaction transaction = FragmentManager.BeginTransactionEx();
			transaction.RemoveEx(fragment);
			transaction.CommitAllowingStateLossEx();

			// And remove the fragment from our own stack
			_fragmentStack.Remove(fragment);

			Device.StartTimer(TimeSpan.FromMilliseconds(10), () =>
			{
				UpdateToolbar();
				return false;
			});
		}

		void ResetToolbar()
		{
			AToolbar oldToolbar = _toolbar;

			if (_titleViewRenderer != null)
			{
				Android.Platform.ClearRenderer(_titleViewRenderer.View);
				_titleViewRenderer = null;
			}

			_toolbar.RemoveView(_titleView);
			_titleView = null;

			_toolbar.RemoveView(_titleIconView);
			_titleIconView = null;

			_imageSource = null;

			_toolbar.RemoveFromParent();
			_toolbar.SetNavigationOnClickListener(null);
			_toolbar = null;

			SetupToolbar();

			// if the old toolbar had padding from transluscentflags, set it to the new toolbar
			if (oldToolbar.PaddingTop != 0)
				_toolbar.SetPadding(0, oldToolbar.PaddingTop, 0, 0);

			RegisterToolbar();
			UpdateToolbar();
			UpdateMenu();

			// Preserve old values that can't be replicated by calling methods above
			if (_toolbar != null)
				_toolbar.Subtitle = oldToolbar.Subtitle;
		}

		void SetupToolbar()
		{
			Context context = Context;
			var activity = (FormsAppCompatActivity)context;

			AToolbar bar;
			if (FormsAppCompatActivity.ToolbarResource != 0)
				bar = activity.LayoutInflater.Inflate(FormsAppCompatActivity.ToolbarResource, null).JavaCast<AToolbar>();
			else
				bar = new AToolbar(context);

			bar.SetNavigationOnClickListener(this);

			AddView(bar);
			_toolbar = bar;
		}

		Task<bool> SwitchContentAsync(Page page, bool animated, bool removed = false, bool popToRoot = false)
		{
			if (!Element.IsAttachedToRoot())
				return Task.FromResult(false);

			var tcs = new TaskCompletionSource<bool>();
			Fragment fragment = GetFragment(page, removed, popToRoot);

#if DEBUG
			// Enables logging of moveToState operations to logcat
			FragmentManager.EnableDebugLogging(true);
#endif

			Current?.SendDisappearing();
			Current = page;

			((Platform)Element.Platform).NavAnimationInProgress = true;
			FragmentTransaction transaction = FragmentManager.BeginTransactionEx();

			if (animated)
				SetupPageTransition(transaction, !removed);

			var fragmentsToRemove = new List<Fragment>();

			if (_fragmentStack.Count == 0)
			{
				transaction.AddEx(Id, fragment);
				_fragmentStack.Add(fragment);
			}
			else
			{
				if (removed)
				{
					// pop only one page, or pop everything to the root
					var popPage = true;
					while (_fragmentStack.Count > 1 && popPage)
					{
						Fragment currentToRemove = _fragmentStack.Last();
						_fragmentStack.RemoveAt(_fragmentStack.Count - 1);
						transaction.HideEx(currentToRemove);
						fragmentsToRemove.Add(currentToRemove);
						popPage = popToRoot;
					}

					Fragment toShow = _fragmentStack.Last();
					// Execute pending transactions so that we can be sure the fragment list is accurate.
					FragmentManager.ExecutePendingTransactionsEx();
					if (FragmentManager.Fragments.Contains(toShow))
						transaction.ShowEx(toShow);
					else
						transaction.AddEx(Id, toShow);
				}
				else
				{
					// push
					Fragment currentToHide = _fragmentStack.Last();
					transaction.HideEx(currentToHide);
					transaction.AddEx(Id, fragment);
					_fragmentStack.Add(fragment);
				}
			}

			// We don't currently support fragment restoration, so we don't need to worry about
			// whether the commit loses state
			transaction.CommitAllowingStateLossEx();

			// The fragment transitions don't really SUPPORT telling you when they end
			// There are some hacks you can do, but they actually are worse than just doing this:

			if (animated)
			{
				if (!removed)
				{
					UpdateToolbar();
					if (_drawerToggle != null && ((INavigationPageController)Element).StackDepth == 2)
						AnimateArrowIn();
				}
				else if (_drawerToggle != null && ((INavigationPageController)Element).StackDepth == 2)
					AnimateArrowOut();

				AddTransitionTimer(tcs, fragment, FragmentManager, fragmentsToRemove, TransitionDuration, removed);
			}
			else
				AddTransitionTimer(tcs, fragment, FragmentManager, fragmentsToRemove, 1, true);

			Context.HideKeyboard(this);
			((Platform)Element.Platform).NavAnimationInProgress = false;

			return tcs.Task;
		}

		Fragment GetFragment(Page page, bool removed, bool popToRoot)
		{
			// pop
			if (removed)
				return _fragmentStack[_fragmentStack.Count - 2];

			// pop to root
			if (popToRoot)
				return _fragmentStack[0];

			// push
			return FragmentContainer.CreateInstance(page);
		}

		void ToolbarTrackerOnCollectionChanged(object sender, EventArgs eventArgs)
		{
			UpdateMenu();
		}

		void UpdateMenu()
		{
			if (_disposed)
				return;

			AToolbar bar = _toolbar;
			Context context = Context;
			IMenu menu = bar.Menu;

			foreach (ToolbarItem item in _toolbarTracker.ToolbarItems)
				item.PropertyChanged -= OnToolbarItemPropertyChanged;
			menu.Clear();

			foreach (ToolbarItem item in _toolbarTracker.ToolbarItems)
			{
				IMenuItemController controller = item;
				item.PropertyChanged += OnToolbarItemPropertyChanged;
				if (item.Order == ToolbarItemOrder.Secondary)
				{
					IMenuItem menuItem = menu.Add(item.Text);
					menuItem.SetEnabled(controller.IsEnabled);
					menuItem.SetOnMenuItemClickListener(new GenericMenuClickListener(controller.Activate));
				}
				else
				{
					IMenuItem menuItem = menu.Add(item.Text);
					UpdateMenuItemIcon(context, menuItem, item);
					menuItem.SetEnabled(controller.IsEnabled);
					menuItem.SetShowAsAction(ShowAsAction.Always);
					menuItem.SetOnMenuItemClickListener(new GenericMenuClickListener(controller.Activate));
				}
			}
		}

		protected virtual void UpdateMenuItemIcon(Context context, IMenuItem menuItem, ToolbarItem toolBarItem)
		{
			FileImageSource icon = toolBarItem.Icon;
			if (!string.IsNullOrEmpty(icon))
			{
				Drawable iconDrawable = context.GetFormsDrawable(icon);
				if (iconDrawable != null)
				{
					menuItem.SetIcon(iconDrawable);
					iconDrawable.Dispose();
				}
			}
		}

		void UpdateToolbar()
		{
			if (_disposed)
				return;

			Context context = Context;
			var activity = (FormsAppCompatActivity)context;
			AToolbar bar = _toolbar;
			ActionBarDrawerToggle toggle = _drawerToggle;

			if (bar == null)
				return;

			bool isNavigated = ((INavigationPageController)Element).StackDepth > 1;
			bar.NavigationIcon = null;
			Page currentPage = Element.CurrentPage;

			if (isNavigated)
			{
				if (toggle != null)
				{
					toggle.DrawerIndicatorEnabled = false;
					toggle.SyncState();
				}

				if (NavigationPage.GetHasBackButton(currentPage))
				{
					var icon = new DrawerArrowDrawable(activity.SupportActionBar.ThemedContext);
					icon.Progress = 1;
					bar.NavigationIcon = icon;
				}
			}
			else
			{
				if (toggle != null && _masterDetailPage != null)
				{
					toggle.DrawerIndicatorEnabled = _masterDetailPage.ShouldShowToolbarButton();
					toggle.SyncState();
				}
			}

			Color tintColor = Element.BarBackgroundColor;

			if (Forms.IsLollipopOrNewer)
			{
				if (tintColor.IsDefault)
					bar.BackgroundTintMode = null;
				else
				{
					bar.BackgroundTintMode = PorterDuff.Mode.Src;
					bar.BackgroundTintList = ColorStateList.ValueOf(tintColor.ToAndroid());
				}
			}
			else
			{
				if (tintColor.IsDefault && _backgroundDrawable != null)
					bar.SetBackground(_backgroundDrawable);
				else if (!tintColor.IsDefault)
				{
					if (_backgroundDrawable == null)
						_backgroundDrawable = bar.Background;
					bar.SetBackgroundColor(tintColor.ToAndroid());
				}
			}

			Color textColor = Element.BarTextColor;
			if (!textColor.IsDefault)
				bar.SetTitleTextColor(textColor.ToAndroid().ToArgb());

			bar.Title = currentPage.Title ?? "";

			UpdateTitleIcon();

			UpdateTitleView();
		}

		void UpdateTitleIcon()
		{
			Page currentPage = Element.CurrentPage;
			var source = NavigationPage.GetTitleIcon(currentPage);

			if (source == null)
			{
				_toolbar.RemoveView(_titleIconView);
				_titleIconView?.Dispose();
				_titleIconView = null;
				_imageSource = null;
				return;
			}

			if (_titleIconView == null)
			{
				_titleIconView = new ImageView(Context);
				_toolbar.AddView(_titleIconView, 0);
			}

			UpdateBitmap(source, _imageSource);
			_imageSource = source;
		}

		async void UpdateBitmap(ImageSource source, ImageSource previousSource = null)
		{
			if (Equals(source, previousSource))
				return;

			_titleIconView.SetImageResource(global::Android.Resource.Color.Transparent);

			Bitmap bitmap = null;
			IImageSourceHandler handler;

			if (source != null && (handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				try
				{
					bitmap = await handler.LoadImageAsync(source, Context);
				}
				catch (TaskCanceledException)
				{
				}
				catch (IOException ex)
				{
					Internals.Log.Warning("Xamarin.Forms.Platform.Android.AppCompat.NavigationPageRenderer", "Error updating bitmap: {0}", ex);
				}
			}

			if (bitmap == null && source is FileImageSource)
				_titleIconView.SetImageResource(ResourceManager.GetDrawableByName(((FileImageSource)source).File));
			else
				_titleIconView.SetImageBitmap(bitmap);

			bitmap?.Dispose();
		}

		void UpdateTitleView()
		{
			AToolbar bar = _toolbar;

			if (bar == null)
				return;

			Page currentPage = Element.CurrentPage;
			VisualElement titleView = NavigationPage.GetTitleView(currentPage);
			if (_titleViewRenderer != null)
			{
				var reflectableType = _titleViewRenderer as System.Reflection.IReflectableType;
				var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _titleViewRenderer.GetType();
				if (titleView == null || Registrar.Registered.GetHandlerTypeForObject(titleView) != rendererType)
				{
					if (_titleView != null)
						_titleView.Child = null;
					Android.Platform.ClearRenderer(_titleViewRenderer.View);
					_titleViewRenderer.Dispose();
					_titleViewRenderer = null;
				}
			}

			if (titleView == null)
				return;

			if (_titleViewRenderer != null)
				_titleViewRenderer.SetElement(titleView);
			else
			{
				_titleViewRenderer = Android.Platform.CreateRenderer(titleView, Context);

				if (_titleView == null)
				{
					_titleView = new Container(Context);
					bar.AddView(_titleView);
				}

				_titleView.Child = _titleViewRenderer;
			}

			Android.Platform.SetRenderer(titleView, _titleViewRenderer);
		}

		void AddTransitionTimer(TaskCompletionSource<bool> tcs, Fragment fragment, FragmentManager fragmentManager, IReadOnlyCollection<Fragment> fragmentsToRemove, int duration, bool shouldUpdateToolbar)
		{
			Device.StartTimer(TimeSpan.FromMilliseconds(duration), () =>
			{
				tcs.TrySetResult(true);
				Current?.SendAppearing();
				if (shouldUpdateToolbar)
					UpdateToolbar();

				if (fragmentsToRemove.Count > 0)
				{
					FragmentTransaction fragmentTransaction = fragmentManager.BeginTransactionEx();

					foreach (Fragment frag in fragmentsToRemove)
						fragmentTransaction.RemoveEx(frag);

					fragmentTransaction.CommitAllowingStateLossEx();
				}

				return false;
			});
		}

		void PushCurrentPages()
		{
			if (_didInitialPushPages)
				return;

			var navController = (INavigationPageController)Element;

			foreach (Page page in navController.Pages)
			{
				PushViewAsync(page, false);
			}
			_didInitialPushPages = true;
		}

		class ClickListener : Object, IOnClickListener
		{
			readonly NavigationPage _element;

			public ClickListener(NavigationPage element)
			{
				_element = element;
			}

			public void OnClick(AView v)
			{
				_element?.PopAsync();
			}
		}

		internal class Container : ViewGroup
		{
			IVisualElementRenderer _child;

			public Container(IntPtr p, global::Android.Runtime.JniHandleOwnership o) : base(p, o)
			{
				// Added default constructor to prevent crash in Dispose
			}

			public Container(Context context) : base(context)
			{
			}

			public IVisualElementRenderer Child
			{
				set
				{
					if (_child != null)
						RemoveView(_child.View);

					_child = value;

					if (value != null)
						AddView(value.View);
				}
			}

			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				if (_child == null)
					return;

				_child.UpdateLayout();
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (_child == null)
				{
					SetMeasuredDimension(0, 0);
					return;
				}

				VisualElement element = _child.Element;

				Context ctx = Context;

				var width = (int)ctx.FromPixels(MeasureSpecFactory.GetSize(widthMeasureSpec));

				SizeRequest request = _child.Element.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
				Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion(_child.Element, new Rectangle(0, 0, width, request.Request.Height));

				int widthSpec = MeasureSpecFactory.MakeMeasureSpec((int)ctx.ToPixels(width), MeasureSpecMode.Exactly);
				int heightSpec = MeasureSpecFactory.MakeMeasureSpec((int)ctx.ToPixels(request.Request.Height), MeasureSpecMode.Exactly);

				_child.View.Measure(widthMeasureSpec, heightMeasureSpec);
				SetMeasuredDimension(widthSpec, heightSpec);
			}
		}

		class DrawerMultiplexedListener : Object, DrawerLayout.IDrawerListener
		{
			public List<DrawerLayout.IDrawerListener> Listeners { get; } = new List<DrawerLayout.IDrawerListener>(2);

			public void OnDrawerClosed(AView drawerView)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerClosed(drawerView);
			}

			public void OnDrawerOpened(AView drawerView)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerOpened(drawerView);
			}

			public void OnDrawerSlide(AView drawerView, float slideOffset)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerSlide(drawerView, slideOffset);
			}

			public void OnDrawerStateChanged(int newState)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerStateChanged(newState);
			}
		}
	}
}
