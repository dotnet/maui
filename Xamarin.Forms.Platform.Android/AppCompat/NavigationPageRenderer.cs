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
using static Android.Views.View;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	public class NavigationPageRenderer : VisualElementRenderer<NavigationPage>, IManageFragments, IOnClickListener
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

		// The following is based on https://android.googlesource.com/platform/frameworks/support.git/+/4a7e12af4ec095c3a53bb8481d8d92f63157c3b7/v4/java/android/support/v4/app/FragmentManager.java#677
		// Must be overriden in a custom renderer to match durations in XML animation resource files
		protected virtual int TransitionDuration { get; set; } = 220;

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

		internal int ContainerPadding { get; set; }

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
						FragmentTransaction trans = fm.BeginTransaction();
						foreach (Fragment fragment in _fragmentStack)
							trans.Remove(fragment);
						trans.CommitAllowingStateLoss();
						fm.ExecutePendingTransactions();
					}
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			PageController.SendAppearing();
			_fragmentStack.Last().UserVisibleHint = true;
			RegisterToolbar();
			UpdateToolbar();
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			PageController.SendDisappearing();
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

				// If there is already stuff on the stack we need to push it
				foreach (Page page in navController.Pages)
				{
					PushViewAsync(page, false);
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
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			AToolbar bar = _toolbar;
			// make sure bar stays on top of everything
			bar.BringToFront();

			base.OnLayout(changed, l, t, r, b);

			int barHeight = ActionBarHeight();

			if (barHeight != _lastActionBarHeight && _lastActionBarHeight > 0)
			{
				ResetToolbar();
				bar = _toolbar;
			}
			_lastActionBarHeight = barHeight;

			bar.Measure(MeasureSpecFactory.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly), MeasureSpecFactory.MakeMeasureSpec(barHeight, MeasureSpecMode.Exactly));

			var barOffset = ToolbarVisible ? barHeight : 0;
			int containerHeight = b - t - ContainerPadding - barOffset;

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
					child.Layout(0, barHeight + ContainerPadding, r, b);
				}
				else
				{
					bar.Layout(0, -1000, r, barHeight - 1000);
					child.Layout(0, ContainerPadding, r, b);
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
				transaction.SetTransition((int)FragmentTransit.FragmentOpen);
			else
				transaction.SetTransition((int)FragmentTransit.FragmentClose);
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
		}

#pragma warning disable 1998 // considered for removal
		async void DeviceInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
#pragma warning restore 1998
		{
			if (nameof(Device.Info.CurrentOrientation) == e.PropertyName)
				ResetToolbar();
		}

		void HandleToolbarItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == MenuItem.IsEnabledProperty.PropertyName || e.PropertyName == MenuItem.TextProperty.PropertyName || e.PropertyName == MenuItem.IconProperty.PropertyName)
				UpdateMenu();
		}

		void InsertPageBefore(Page page, Page before)
		{
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
			FragmentTransaction transaction = FragmentManager.BeginTransaction();
			transaction.Remove(fragment);
			transaction.CommitAllowingStateLoss();

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
			var tcs = new TaskCompletionSource<bool>();
			Fragment fragment = GetFragment(page, removed, popToRoot);

#if DEBUG
			// Enables logging of moveToState operations to logcat
			FragmentManager.EnableDebugLogging(true);
#endif

			Current = page;

			((Platform)Element.Platform).NavAnimationInProgress = true;
			FragmentTransaction transaction = FragmentManager.BeginTransaction();

			if (animated)
				SetupPageTransition(transaction, !removed);

			var fragmentsToRemove = new List<Fragment>();

			if (_fragmentStack.Count == 0)
			{
				transaction.Add(Id, fragment);
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
						transaction.Hide(currentToRemove);
						fragmentsToRemove.Add(currentToRemove);
						popPage = popToRoot;
					}

					Fragment toShow = _fragmentStack.Last();
					// Execute pending transactions so that we can be sure the fragment list is accurate.
					FragmentManager.ExecutePendingTransactions();
					if (FragmentManager.Fragments.Contains(toShow))
						transaction.Show(toShow);
					else
						transaction.Add(Id, toShow);
				}
				else
				{
					// push
					Fragment currentToHide = _fragmentStack.Last();
					transaction.Hide(currentToHide);
					transaction.Add(Id, fragment);
					_fragmentStack.Add(fragment);
				}
			}

			// We don't currently support fragment restoration, so we don't need to worry about
			// whether the commit loses state
			transaction.CommitAllowingStateLoss();

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

			// TransitionDuration is how long the built-in animations are, and they are "reversible" in the sense that starting another one slightly before it's done is fine

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
				item.PropertyChanged -= HandleToolbarItemPropertyChanged;
			menu.Clear();

			foreach (ToolbarItem item in _toolbarTracker.ToolbarItems)
			{
				IMenuItemController controller = item;
				item.PropertyChanged += HandleToolbarItemPropertyChanged;
				if (item.Order == ToolbarItemOrder.Secondary)
				{
					IMenuItem menuItem = menu.Add(item.Text);
					menuItem.SetEnabled(controller.IsEnabled);
					menuItem.SetOnMenuItemClickListener(new GenericMenuClickListener(controller.Activate));
				}
				else
				{
					IMenuItem menuItem = menu.Add(item.Text);
					FileImageSource icon = item.Icon;
					if (!string.IsNullOrEmpty(icon))
					{
						Drawable iconDrawable = context.GetFormsDrawable(icon);
						if (iconDrawable != null)
							menuItem.SetIcon(iconDrawable);
					}
					menuItem.SetEnabled(controller.IsEnabled);
					menuItem.SetShowAsAction(ShowAsAction.Always);
					menuItem.SetOnMenuItemClickListener(new GenericMenuClickListener(controller.Activate));
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

			if (isNavigated)
			{
				if (toggle != null)
				{
					toggle.DrawerIndicatorEnabled = false;
					toggle.SyncState();
				}

				if (NavigationPage.GetHasBackButton(Element.CurrentPage))
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

			bar.Title = Element.CurrentPage.Title ?? "";
		}

		void AddTransitionTimer(TaskCompletionSource<bool> tcs, Fragment fragment, FragmentManager fragmentManager, IReadOnlyCollection<Fragment> fragmentsToRemove, int duration, bool shouldUpdateToolbar)
		{
			Device.StartTimer(TimeSpan.FromMilliseconds(duration), () =>
			{
				tcs.TrySetResult(true);
				fragment.UserVisibleHint = true;
				if (shouldUpdateToolbar)
					UpdateToolbar();

				FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();

				foreach (Fragment frag in fragmentsToRemove)
					fragmentTransaction.Remove(frag);

				fragmentTransaction.CommitAllowingStateLoss();

				return false;
			});
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
