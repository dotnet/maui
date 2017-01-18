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
		AToolbar _toolbar;
		ToolbarTracker _toolbarTracker;
		bool _toolbarVisible;

		public NavigationPageRenderer()
		{
			AutoPackage = false;
			Id = FormsAppCompatActivity.GetUniqueId();
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

		IPageController PageController => Element as IPageController;

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

				var activity = (FormsAppCompatActivity)Context;

				// API only exists on newer android YAY
				if ((int)Build.VERSION.SdkInt >= 17)
				{
					if (!activity.IsDestroyed)
					{
						FragmentManager fm = FragmentManager;
						FragmentTransaction trans = fm.BeginTransaction();
						foreach (Fragment fragment in _fragmentStack)
							trans.Remove(fragment);
						trans.CommitAllowingStateLoss();
						fm.ExecutePendingTransactions();
					}
				}

				if (Element != null)
				{
					foreach(Element element in PageController.InternalChildren)
					{
						var child = element as VisualElement;
						if (child == null)
						{
							continue;
						}
							
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
					_drawerListener = null;
				}

				_drawerToggle = null;

				Current = null;

				Device.Info.PropertyChanged -= DeviceInfoPropertyChanged;
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
				foreach (Page page in navController.StackCopy.Reverse())
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

			int internalHeight = b - t - barHeight;
			int containerHeight = ToolbarVisible ? internalHeight : b - t;
			containerHeight -= ContainerPadding;

			PageController.ContainerArea = new Rectangle(0, 0, Context.FromPixels(r - l), Context.FromPixels(containerHeight));
			// Potential for optimization here, the exact conditions by which you don't need to do this are complex
			// and the cost of doing when it's not needed is moderate to low since the layout will short circuit pretty fast
			Element.ForceLayout();

			for (var i = 0; i < ChildCount; i++)
			{
				AView child = GetChildAt(i);
				bool isBar = JNIEnv.IsSameObject(child.Handle, bar.Handle);

				if (ToolbarVisible)
				{
					if (isBar)
						bar.Layout(0, 0, r - l, barHeight);
					else
						child.Layout(0, barHeight + ContainerPadding, r, b);
				}
				else
				{
					if (isBar)
						bar.Layout(0, -1000, r, barHeight - 1000);
					else
						child.Layout(0, ContainerPadding, r, b);
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

		void FilterPageFragment(Page page)
		{
			_fragmentStack.RemoveAll(f => ((FragmentContainer)f).Page == page);
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

		Task<bool> OnPopToRootAsync(Page page, bool animated)
		{
			return SwitchContentAsync(page, animated, true, true);
		}

		Task<bool> OnPopViewAsync(Page page, bool animated)
		{
			Page pageToShow = ((INavigationPageController)Element).StackCopy.Skip(1).FirstOrDefault();
			if (pageToShow == null)
				return Task.FromResult(false);

			return SwitchContentAsync(pageToShow, animated, true);
		}

		Task<bool> OnPushAsync(Page view, bool animated)
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

		private DrawerMultiplexedListener _drawerListener;
		private DrawerLayout _drawerLayout;

		void RegisterToolbar()
		{
			Context context = Context;
			AToolbar bar = _toolbar;
			Element page = Element.RealParent;

			MasterDetailPage masterDetailPage = null;
			while (page != null)
			{
				if (page is MasterDetailPage)
				{
					masterDetailPage = page as MasterDetailPage;
					break;
				}
				page = page.RealParent;
			}

			if (masterDetailPage == null)
			{
				masterDetailPage = PageController.InternalChildren[0] as MasterDetailPage;
				if (masterDetailPage == null)
					return;
			}

			if (((IMasterDetailPageController)masterDetailPage).ShouldShowSplitMode)
				return;

			var renderer = Android.Platform.GetRenderer(masterDetailPage) as MasterDetailPageRenderer;
			if (renderer == null)
				return;

			_drawerLayout = (DrawerLayout)renderer;
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

			_drawerToggle.DrawerIndicatorEnabled = true;
		}

		void RemovePage(Page page)
		{
			IVisualElementRenderer rendererToRemove = Android.Platform.GetRenderer(page);
			var containerToRemove = (PageContainer)rendererToRemove?.ViewGroup.Parent;

			// Also remove this page from the fragmentStack
			FilterPageFragment(page);

			containerToRemove.RemoveFromParent();
			if (rendererToRemove != null)
			{
				rendererToRemove.ViewGroup.RemoveFromParent();
				rendererToRemove.Dispose();
			}
			containerToRemove?.Dispose();

			Device.StartTimer(TimeSpan.FromMilliseconds(10), () =>
			{
				UpdateToolbar();
				return false;
			});
		}

		void ResetToolbar()
		{
			_toolbar.RemoveFromParent();
			_toolbar.SetNavigationOnClickListener(null);
			_toolbar = null;

			SetupToolbar();
			RegisterToolbar();
			UpdateToolbar();
			UpdateMenu();
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

		Task<bool> SwitchContentAsync(Page view, bool animated, bool removed = false, bool popToRoot = false)
		{
			var tcs = new TaskCompletionSource<bool>();
			Fragment fragment = FragmentContainer.CreateInstance(view);
			FragmentManager fm = FragmentManager;
			List<Fragment> fragments = _fragmentStack;

			Current = view;

			((Platform)Element.Platform).NavAnimationInProgress = true;
			FragmentTransaction transaction = fm.BeginTransaction();

			if (animated)
				SetupPageTransition(transaction, !removed);

			transaction.DisallowAddToBackStack();

			if (fragments.Count == 0)
			{
				transaction.Add(Id, fragment);
				fragments.Add(fragment);
			}
			else
			{
				if (removed)
				{
					// pop only one page, or pop everything to the root
					var popPage = true;
					while (fragments.Count > 1 && popPage)
					{
						Fragment currentToRemove = fragments.Last();
						fragments.RemoveAt(fragments.Count - 1);
						transaction.Remove(currentToRemove);
						popPage = popToRoot;
					}

					Fragment toShow = fragments.Last();
					// Execute pending transactions so that we can be sure the fragment list is accurate.
					fm.ExecutePendingTransactions();
					if (fm.Fragments.Contains(toShow))
						transaction.Show(toShow);
					else
						transaction.Add(Id, toShow);
				}
				else
				{
					// push
					Fragment currentToHide = fragments.Last();
					transaction.Hide(currentToHide);
					transaction.Add(Id, fragment);
					fragments.Add(fragment);
				}
			}
			transaction.Commit();

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

				Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
				{
					tcs.TrySetResult(true);
					fragment.UserVisibleHint = true;
					if (removed)
						UpdateToolbar();
					return false;
				});
			}
			else
			{
				Device.StartTimer(TimeSpan.FromMilliseconds(1), () =>
				{
					tcs.TrySetResult(true);
					fragment.UserVisibleHint = true;
					UpdateToolbar();
					return false;
				});
			}

			Context.HideKeyboard(this);
			((Platform)Element.Platform).NavAnimationInProgress = false;

			// 200ms is how long the animations are, and they are "reversible" in the sense that starting another one slightly before it's done is fine

			return tcs.Task;
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
						Drawable iconBitmap = context.Resources.GetDrawable(icon);
						if (iconBitmap != null)
							menuItem.SetIcon(iconBitmap);
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
				if (toggle != null)
				{
					toggle.DrawerIndicatorEnabled = true;
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