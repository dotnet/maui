using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using static Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage;
using static Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page;
using PageUIStatusBarAnimation = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIStatusBarAnimation;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[Obsolete("Use Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer instead")]
	public class NavigationRenderer : UINavigationController, IVisualElementRenderer, IEffectControlProvider
	{
		internal const string UpdateToolbarButtons = "Xamarin.UpdateToolbarButtons";
		bool _appeared;
		bool _ignorePopCall;
		bool _loaded;
		FlyoutPage _parentFlyoutPage;
		Size _queuedSize;
		UIViewController[] _removeControllers;
		UIToolbar _secondaryToolbar;
		VisualElementTracker _tracker;
		nfloat _navigationBottom = 0;
		bool _hasNavigationBar;
		UIImage _defaultNavBarShadowImage;
		UIImage _defaultNavBarBackImage;
		bool _disposed;

		[Preserve(Conditional = true)]
		public NavigationRenderer() : base(typeof(FormsNavigationBar), null)
		{
			MessagingCenter.Subscribe<IVisualElementRenderer>(this, UpdateToolbarButtons, sender =>
			{
				if (!ViewControllers.Any())
					return;
				var parentingViewController = GetParentingViewController();
				parentingViewController?.UpdateLeftBarButtonItem();
			});
		}

		Page Current { get; set; }

		IPageController PageController => Element as IPageController;

		NavigationPage NavPage => Element as NavigationPage;
		INavigationPageController NavPageController => NavPage;

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public UIView NativeView
		{
			get { return View; }
		}

		public void SetElement(VisualElement element)
		{
			var oldElement = Element;
			Element = element;
			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			if (element != null)
				element.SendViewInitialized(NativeView);

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);
		}

		public void SetElementSize(Size size)
		{
			if (_loaded)
				Element.Layout(new Rect(Element.X, Element.Y, size.Width, size.Height));
			else
				_queuedSize = size;
		}

		public UIViewController ViewController
		{
			get { return this; }
		}

		//TODO: this was deprecated in iOS8.0 and is not called in 9.0+
		[UnsupportedOSPlatform("ios8.0")]
		[UnsupportedOSPlatform("tvos")]
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);

			View.SetNeedsLayout();

			var parentingViewController = GetParentingViewController();
			parentingViewController?.UpdateLeftBarButtonItem();
		}

		public Task<bool> PopToRootAsync(Page page, bool animated = true)
		{
			return OnPopToRoot(page, animated);
		}

		public override UIViewController[] PopToRootViewController(bool animated)
		{
			if (!_ignorePopCall && ViewControllers.Length > 1)
				RemoveViewControllers(animated);

			return base.PopToRootViewController(animated);
		}

		public Task<bool> PopViewAsync(Page page, bool animated = true)
		{
			return OnPopViewAsync(page, animated);
		}

		public override UIViewController PopViewController(bool animated)
		{
			RemoveViewControllers(animated);
			return base.PopViewController(animated);
		}

		public Task<bool> PushPageAsync(Page page, bool animated = true)
		{
			return OnPushAsync(page, animated);
		}

		public override void ViewDidAppear(bool animated)
		{
			if (!_appeared)
			{
				_appeared = true;
				PageController?.SendAppearing();
			}

			base.ViewDidAppear(animated);

			View.SetNeedsLayout();
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			SetStatusBarStyle();
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);

			if (!_appeared || Element == null)
				return;

			_appeared = false;
			PageController.SendDisappearing();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			if (Current == null)
				return;
			UpdateToolBarVisible();

			var navBarFrameBottom = Math.Min(NavigationBar.Frame.Bottom, 140);
			_navigationBottom = (nfloat)navBarFrameBottom;
			var toolbar = _secondaryToolbar;

			//save the state of the Current page we are calculating, this will fire before Current is updated
			_hasNavigationBar = NavigationPage.GetHasNavigationBar(Current);

			// Use 0 if the NavBar is hidden or will be hidden
			var toolbarY = NavigationBarHidden || NavigationBar.Translucent || !_hasNavigationBar ? 0 : navBarFrameBottom;
			toolbar.Frame = new RectangleF(0, toolbarY, View.Frame.Width, toolbar.Frame.Height);

			double trueBottom = toolbar.Hidden ? toolbarY : toolbar.Frame.Bottom;
			var modelSize = _queuedSize.IsZero ? Element.Bounds.Size : _queuedSize;
			PageController.ContainerArea =
				new Rect(0, toolbar.Hidden ? 0 : toolbar.Frame.Height, modelSize.Width, modelSize.Height - trueBottom);

			if (!_queuedSize.IsZero)
			{
				Element.Layout(new Rect(Element.X, Element.Y, _queuedSize.Width, _queuedSize.Height));
				_queuedSize = Size.Zero;
			}

			_loaded = true;

			foreach (var view in View.Subviews)
			{
				if (view == NavigationBar || view == _secondaryToolbar)
					continue;
				view.Frame = View.Bounds;
			}
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			UpdateTranslucent();

			_secondaryToolbar = new SecondaryToolbar { Frame = new RectangleF(0, 0, 320, 44) };
			View.Add(_secondaryToolbar);
			_secondaryToolbar.Hidden = true;

			FindParentFlyoutPage();

			var navPage = NavPage;
			INavigationPageController navPageController = NavPage;
			if (navPage.CurrentPage == null)
			{
				throw new InvalidOperationException(
					"NavigationPage must have a root Page before being used. Either call PushAsync with a valid Page, or pass a Page to the constructor before usage.");
			}

			navPageController.PushRequested += OnPushRequested;
			navPageController.PopRequested += OnPopRequested;
			navPageController.PopToRootRequested += OnPopToRootRequested;
			navPageController.RemovePageRequested += OnRemovedPageRequested;
			navPageController.InsertPageBeforeRequested += OnInsertPageBeforeRequested;

			UpdateBarBackground();
			UpdateBarTextColor();
			UpdateHideNavigationBarSeparator();
			UpdateUseLargeTitles();

			if (OperatingSystem.IsIOSVersionAtLeast(11))
				SetNeedsUpdateOfHomeIndicatorAutoHidden();

			// If there is already stuff on the stack we need to push it
			NavPageController.Pages.ForEach(async p => await PushPageAsync(p, false));

			_tracker = new VisualElementTracker(this);

			Element.PropertyChanged += HandlePropertyChanged;

			UpdateToolBarVisible();
			UpdateBackgroundColor();
			Current = navPage.CurrentPage;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				MessagingCenter.Unsubscribe<IVisualElementRenderer>(this, UpdateToolbarButtons);

				foreach (var childViewController in ViewControllers)
					childViewController.Dispose();

				if (_tracker != null)
					_tracker.Dispose();

				_secondaryToolbar.RemoveFromSuperview();
				_secondaryToolbar.Dispose();
				_secondaryToolbar = null;

				_parentFlyoutPage = null;
				Current = null; // unhooks events

				var navPage = NavPage;
				INavigationPageController navPageController = NavPage;
				navPage.PropertyChanged -= HandlePropertyChanged;

				navPageController.PushRequested -= OnPushRequested;
				navPageController.PopRequested -= OnPopRequested;
				navPageController.PopToRootRequested -= OnPopToRootRequested;
				navPageController.RemovePageRequested -= OnRemovedPageRequested;
				navPageController.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;
			}

			base.Dispose(disposing);

			if (disposing && _appeared)
			{
				PageController.SendDisappearing();

				_appeared = false;
			}
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			ElementChanged?.Invoke(this, e);
		}

		protected virtual async Task<bool> OnPopToRoot(Page page, bool animated)
		{
			_ignorePopCall = true;
			var renderer = Platform.GetRenderer(page);
			if (renderer == null || renderer.ViewController == null)
				return false;

			var task = GetAppearedOrDisappearedTask(page);

			PopToRootViewController(animated);

			_ignorePopCall = false;
			var success = !await task;

			UpdateToolBarVisible();
			return success;
		}

		protected virtual async Task<bool> OnPopViewAsync(Page page, bool animated)
		{
			if (_ignorePopCall)
				return true;

			var renderer = Platform.GetRenderer(page);
			if (renderer == null || renderer.ViewController == null)
				return false;

			if (page != ((ParentingViewController)TopViewController).Child)
				throw new NotSupportedException("Popped page does not appear on top of current navigation stack, please file a bug.");

			var task = GetAppearedOrDisappearedTask(page);

			UIViewController poppedViewController;
			_ignorePopCall = true;
			poppedViewController = base.PopViewController(animated);

			var actuallyRemoved = poppedViewController == null ? true : !await task;
			_ignorePopCall = false;

			poppedViewController?.Dispose();

			UpdateToolBarVisible();
			return actuallyRemoved;
		}

		protected virtual async Task<bool> OnPushAsync(Page page, bool animated)
		{
			if (page is FlyoutPage)
				System.Diagnostics.Trace.WriteLine($"Pushing a {nameof(FlyoutPage)} onto a {nameof(NavigationPage)} is not a supported UI pattern on iOS. " +
					"Please see https://developer.apple.com/documentation/uikit/uisplitviewcontroller for more details.");

			var pack = CreateViewControllerForPage(page);
			var task = GetAppearedOrDisappearedTask(page);

			PushViewController(pack, animated);

			var shown = await task;
			UpdateToolBarVisible();
			return shown;
		}

		public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
		{
#pragma warning disable CA1422 // Validate platform compatibility
			base.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422 // Validate platform compatibility			// Make sure the control adheres to changes in UI theme
			if (Forms.IsiOS13OrNewer && previousTraitCollection?.UserInterfaceStyle != TraitCollection.UserInterfaceStyle)
				UpdateBackgroundColor();
		}


		ParentingViewController CreateViewControllerForPage(Page page)
		{
			if (Platform.GetRenderer(page) == null)
				Platform.SetRenderer(page, Platform.CreateRenderer(page));

			// must pack into container so padding can work
			// otherwise the view controller is forced to 0,0
			var pack = new ParentingViewController(this) { Child = page };

			pack.UpdateTitleArea(page);

			var pageRenderer = Platform.GetRenderer(page);
			pack.View.AddSubview(pageRenderer.ViewController.View);
			pack.AddChildViewController(pageRenderer.ViewController);
			pageRenderer.ViewController.DidMoveToParentViewController(pack);

			return pack;
		}

		ParentingViewController GetParentingViewController()
		{
			if (!ViewControllers.Any())
				return null;

			return ViewControllers.Last() as ParentingViewController;
		}

		void FindParentFlyoutPage()
		{
			var page = Element as Page;
			var parentPages = page.GetParentPages();
			var flyoutDetail = parentPages.OfType<FlyoutPage>().FirstOrDefault();

			if (flyoutDetail != null && parentPages.Append((Page)Element).Contains(flyoutDetail.Detail))
				_parentFlyoutPage = flyoutDetail;
		}

		Task<bool> GetAppearedOrDisappearedTask(Page page)
		{
			var tcs = new TaskCompletionSource<bool>();

			var parentViewController = Platform.GetRenderer(page).ViewController.ParentViewController as ParentingViewController;
			if (parentViewController == null)
				throw new NotSupportedException("ParentingViewController parent could not be found. Please file a bug.");

			EventHandler appearing = null, disappearing = null;
			appearing = (s, e) =>
			{
				parentViewController.Appearing -= appearing;
				parentViewController.Disappearing -= disappearing;

				Device.BeginInvokeOnMainThread(() => { tcs.SetResult(true); });
			};

			disappearing = (s, e) =>
			{
				parentViewController.Appearing -= appearing;
				parentViewController.Disappearing -= disappearing;

				Device.BeginInvokeOnMainThread(() => { tcs.SetResult(false); });
			};

			parentViewController.Appearing += appearing;
			parentViewController.Disappearing += disappearing;

			return tcs.Task;
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == NavigationPage.BarBackgroundColorProperty.PropertyName ||
				e.PropertyName == NavigationPage.BarBackgroundProperty.PropertyName)
			{
				UpdateBarBackground();
			}
			else if (e.PropertyName == NavigationPage.BarTextColorProperty.PropertyName
				  || e.PropertyName == StatusBarTextColorModeProperty.PropertyName)
			{
				UpdateBarTextColor();
				SetStatusBarStyle();
			}
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
			{
				UpdateBackgroundColor();
			}
			else if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
			{
				Current = NavPage?.CurrentPage;
				ValidateNavbarExists(Current);
			}
			else if (e.PropertyName == IsNavigationBarTranslucentProperty.PropertyName)
			{
				UpdateTranslucent();
			}
			else if (e.PropertyName == PreferredStatusBarUpdateAnimationProperty.PropertyName)
			{
				UpdateCurrentPagePreferredStatusBarUpdateAnimation();
			}
			else if (e.PropertyName == PrefersLargeTitlesProperty.PropertyName)
			{
				UpdateUseLargeTitles();
			}
			else if (e.PropertyName == NavigationPage.BackButtonTitleProperty.PropertyName)
			{
				var pack = (ParentingViewController)TopViewController;
				pack?.UpdateTitleArea(pack.Child);
			}
			else if (e.PropertyName == HideNavigationBarSeparatorProperty.PropertyName)
			{
				UpdateHideNavigationBarSeparator();
			}
		}

		void ValidateNavbarExists(Page newCurrentPage)
		{
			//if the last time we did ViewDidLayoutSubviews we had other value for _hasNavigationBar
			//we will need to relayout. This is because Current is updated async of the layout happening
			if (_hasNavigationBar != NavigationPage.GetHasNavigationBar(newCurrentPage))
				ViewDidLayoutSubviews();
		}

		void UpdateHideNavigationBarSeparator()
		{
			bool shouldHide = NavPage.OnThisPlatform().HideNavigationBarSeparator();

			// Just setting the ShadowImage is good for iOS11
			if (_defaultNavBarShadowImage == null)
				_defaultNavBarShadowImage = NavigationBar.ShadowImage;

			if (Forms.IsiOS13OrNewer)
			{
				if (shouldHide)
				{
					NavigationBar.CompactAppearance.ShadowColor = UIColor.Clear;
					NavigationBar.StandardAppearance.ShadowColor = UIColor.Clear;
					NavigationBar.ScrollEdgeAppearance.ShadowColor = UIColor.Clear;
				}
				else
				{
					NavigationBar.CompactAppearance.ShadowColor = UIColor.FromRGBA(0, 0, 0, 76); //default ios13 shadow color
					NavigationBar.StandardAppearance.ShadowColor = UIColor.FromRGBA(0, 0, 0, 76);
					NavigationBar.ScrollEdgeAppearance.ShadowColor = UIColor.FromRGBA(0, 0, 0, 76);
				}
			}
			else
			{
				if (shouldHide)
					NavigationBar.ShadowImage = new UIImage();
				else
					NavigationBar.ShadowImage = _defaultNavBarShadowImage;
			}

			if (!Forms.IsiOS11OrNewer)
			{
				// For iOS 10 and lower, you need to set the background image.
				// If you set this for iOS11, you'll remove the background color.
				if (_defaultNavBarBackImage == null)
					_defaultNavBarBackImage = NavigationBar.GetBackgroundImage(UIBarMetrics.Default);

				if (shouldHide)
					NavigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
				else
					NavigationBar.SetBackgroundImage(_defaultNavBarBackImage, UIBarMetrics.Default);
			}
		}

		void UpdateCurrentPagePreferredStatusBarUpdateAnimation()
		{
			// Not using the extension method syntax here because for some reason it confuses the mono compiler
			// and throws a CS0121 error
			PageUIStatusBarAnimation animation = PlatformConfiguration.iOSSpecific.Page.PreferredStatusBarUpdateAnimation(((Page)Element).OnThisPlatform());
			PlatformConfiguration.iOSSpecific.Page.SetPreferredStatusBarUpdateAnimation(Current.OnThisPlatform(), animation);
		}

		[PortHandler]
		void UpdateUseLargeTitles()
		{
			if (Forms.IsiOS11OrNewer && NavPage != null)
				NavigationBar.PrefersLargeTitles = NavPage.OnThisPlatform().PrefersLargeTitles();
		}

		[PortHandler]
		void UpdateTranslucent()
		{
			NavigationBar.Translucent = NavPage.OnThisPlatform().IsNavigationBarTranslucent();
		}

		void InsertPageBefore(Page page, Page before)
		{
			if (before == null)
				throw new ArgumentNullException("before");
			if (page == null)
				throw new ArgumentNullException("page");

			var pageContainer = CreateViewControllerForPage(page);
			var target = Platform.GetRenderer(before).ViewController.ParentViewController;
			ViewControllers = ViewControllers.Insert(ViewControllers.IndexOf(target), pageContainer);
		}

		void OnInsertPageBeforeRequested(object sender, NavigationRequestedEventArgs e)
		{
			InsertPageBefore(e.Page, e.BeforePage);
		}

		void OnPopRequested(object sender, NavigationRequestedEventArgs e)
		{
			e.Task = PopViewAsync(e.Page, e.Animated);
		}

		void OnPopToRootRequested(object sender, NavigationRequestedEventArgs e)
		{
			e.Task = PopToRootAsync(e.Page, e.Animated);
		}

		void OnPushRequested(object sender, NavigationRequestedEventArgs e)
		{
			// If any text entry controls have focus, we need to end their editing session
			// so that they are not the first responder; if we don't some things (like the activity indicator
			// on pull-to-refresh) will not work correctly.
			View?.Window?.EndEditing(true);

			e.Task = PushPageAsync(e.Page, e.Animated);
		}

		void OnRemovedPageRequested(object sender, NavigationRequestedEventArgs e)
		{
			RemovePage(e.Page);
		}

		void RemovePage(Page page)
		{
			if (page == null)
				throw new ArgumentNullException("page");
			if (page == Current)
				throw new NotSupportedException(); // should never happen as NavPage protects against this

			var target = Platform.GetRenderer(page).ViewController.ParentViewController;

			// So the ViewControllers property is not very property like on iOS. Assigning to it doesn't cause it to be
			// immediately reflected into the property. The change will not be reflected until there has been sufficient time
			// to process it (it ends up on the event queue). So to resolve this issue we keep our own stack until we
			// know iOS has processed it, and make sure any updates use that.

			// In the future we may want to make RemovePageAsync and deprecate RemovePage to handle cases where Push/Pop is called
			// during a remove cycle.

			if (_removeControllers == null)
			{
				_removeControllers = ViewControllers.Remove(target);
				ViewControllers = _removeControllers;
				Device.BeginInvokeOnMainThread(() => { _removeControllers = null; });
			}
			else
			{
				_removeControllers = _removeControllers.Remove(target);
				ViewControllers = _removeControllers;
			}
			target.Dispose();
			var parentingViewController = GetParentingViewController();
			parentingViewController?.UpdateLeftBarButtonItem(page);
		}

		void RemoveViewControllers(bool animated)
		{
			var controller = TopViewController as ParentingViewController;
			if (controller == null || controller.Child == null || Platform.GetRenderer(controller.Child) == null)
				return;

			// Gesture in progress, lets not be proactive and just wait for it to finish
			var task = GetAppearedOrDisappearedTask(controller.Child);

			task.ContinueWith(t =>
			{
				// task returns true if the user lets go of the page and is not popped
				// however at this point the renderer is already off the visual stack so we just need to update the NavigationPage
				// Also worth noting this task returns on the main thread
				if (t.Result)
					return;
				// because we skip the normal pop process we need to dispose ourselves
				controller?.Dispose();
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		void UpdateBackgroundColor()
		{
			var color = Element.BackgroundColor ?? Maui.Platform.ColorExtensions.BackgroundColor.ToColor();
			View.BackgroundColor = color.ToPlatform();
		}

		void UpdateBarBackground()
		{
			var barBackgroundColor = NavPage.BarBackgroundColor;

			if (Forms.IsiOS13OrNewer)
			{
				var navigationBarAppearance = NavigationBar.StandardAppearance;

				navigationBarAppearance.ConfigureWithOpaqueBackground();

				if (barBackgroundColor == null)
				{
					navigationBarAppearance.BackgroundColor = Maui.Platform.ColorExtensions.BackgroundColor;

					var parentingViewController = GetParentingViewController();
					parentingViewController?.SetupDefaultNavigationBarAppearance();
				}
				else
					navigationBarAppearance.BackgroundColor = barBackgroundColor.ToPlatform();

				var barBackgroundBrush = NavPage.BarBackground;
				var backgroundImage = NavigationBar.GetBackgroundImage(barBackgroundBrush);
				navigationBarAppearance.BackgroundImage = backgroundImage;

				NavigationBar.CompactAppearance = navigationBarAppearance;
				NavigationBar.StandardAppearance = navigationBarAppearance;
				NavigationBar.ScrollEdgeAppearance = navigationBarAppearance;
			}
			else
			{
				// Set navigation bar background color
				NavigationBar.BarTintColor = barBackgroundColor == null
					? UINavigationBar.Appearance.BarTintColor
					: barBackgroundColor.ToPlatform();

				var barBackgroundBrush = NavPage.BarBackground;
				var backgroundImage = NavigationBar.GetBackgroundImage(barBackgroundBrush);
				NavigationBar.SetBackgroundImage(backgroundImage, UIBarMetrics.Default);
			}
		}

		void UpdateBarTextColor()
		{
			var barTextColor = NavPage.BarTextColor;

			// Determine new title text attributes via global static data
			var globalTitleTextAttributes = UINavigationBar.Appearance.TitleTextAttributes;
			var titleTextAttributes = new UIStringAttributes
			{
				ForegroundColor = barTextColor == null ? globalTitleTextAttributes?.ForegroundColor : barTextColor.ToPlatform(),
				Font = globalTitleTextAttributes?.Font
			};

			// Determine new large title text attributes via global static data
			var largeTitleTextAttributes = titleTextAttributes;
			if (Forms.IsiOS11OrNewer)
			{
				var globalLargeTitleTextAttributes = UINavigationBar.Appearance.LargeTitleTextAttributes;

				largeTitleTextAttributes = new UIStringAttributes
				{
					ForegroundColor = barTextColor == null ? globalLargeTitleTextAttributes?.ForegroundColor : barTextColor.ToPlatform(),
					Font = globalLargeTitleTextAttributes?.Font
				};
			}

			if (Forms.IsiOS13OrNewer)
			{
				NavigationBar.CompactAppearance.TitleTextAttributes = titleTextAttributes;
				NavigationBar.CompactAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;

				NavigationBar.StandardAppearance.TitleTextAttributes = titleTextAttributes;
				NavigationBar.StandardAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;

				NavigationBar.ScrollEdgeAppearance.TitleTextAttributes = titleTextAttributes;
				NavigationBar.ScrollEdgeAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;
			}
			else
			{
				NavigationBar.TitleTextAttributes = titleTextAttributes;

				if (Forms.IsiOS11OrNewer)
					NavigationBar.LargeTitleTextAttributes = largeTitleTextAttributes;
			}

			// set Tint color (i. e. Back Button arrow and Text)
			var iconColor = Current != null ? NavigationPage.GetIconColor(Current) : null;
			if (iconColor == null)
				iconColor = barTextColor;

			NavigationBar.TintColor = iconColor == null || NavPage.OnThisPlatform().GetStatusBarTextColorMode() == StatusBarTextColorMode.DoNotAdjust
				? UINavigationBar.Appearance.TintColor
				: iconColor.ToPlatform();
		}

		void SetStatusBarStyle()
		{
			var barTextColor = NavPage.BarTextColor;
			var statusBarColorMode = NavPage.OnThisPlatform().GetStatusBarTextColorMode();

#pragma warning disable CA1416, CA1422 // TODO: UIApplication.StatusBarStyle has [UnsupportedOSPlatform("ios9.0")]
			if (statusBarColorMode == StatusBarTextColorMode.DoNotAdjust || barTextColor?.GetLuminosity() <= 0.5)
			{
				// Use dark text color for status bar
				if (Forms.IsiOS13OrNewer)
				{
					UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.DarkContent;
				}
				else
				{
					UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.Default;
				}
			}
			else
			{
				// Use light text color for status bar
				UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
			}
#pragma warning restore CA1416, CA1422
		}

		void UpdateToolBarVisible()
		{
			if (_secondaryToolbar == null)
				return;
			if (TopViewController != null && TopViewController.ToolbarItems != null && TopViewController.ToolbarItems.Any())
			{
				_secondaryToolbar.Hidden = false;
				_secondaryToolbar.Items = TopViewController.ToolbarItems;
			}
			else
			{
				_secondaryToolbar.Hidden = true;
				//secondaryToolbar.Items = null;
			}

			TopViewController?.NavigationItem?.TitleView?.SizeToFit();
			TopViewController?.NavigationItem?.TitleView?.LayoutSubviews();
		}

		internal async Task UpdateFormsInnerNavigation(Page pageBeingRemoved)
		{
			if (NavPage == null)
				return;
			if (_ignorePopCall)
				return;

			_ignorePopCall = true;
			if (Element.Navigation.NavigationStack.Contains(pageBeingRemoved))
				await (NavPage as INavigationPageController)?.RemoveAsyncInner(pageBeingRemoved, false, true);
			_ignorePopCall = false;

		}

		internal static async void SetFlyoutLeftBarButton(UIViewController containerController, FlyoutPage FlyoutPage)
		{
			if (!FlyoutPage.ShouldShowToolbarButton())
			{
				containerController.NavigationItem.LeftBarButtonItem = null;
				return;
			}

			await FlyoutPage.Flyout.ApplyNativeImageAsync(Page.IconImageSourceProperty, icon =>
			{
				if (icon != null)
				{
					try
					{
						containerController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, OnItemTapped);
					}
					catch (Exception)
					{
						// Throws Exception otherwise would catch more specific exception type
					}
				}

				if (icon == null || containerController.NavigationItem.LeftBarButtonItem == null)
				{
					containerController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(FlyoutPage.Flyout.Title, UIBarButtonItemStyle.Plain, OnItemTapped);
				}

				if (FlyoutPage != null && !string.IsNullOrEmpty(FlyoutPage.AutomationId))
					SetAutomationId(containerController.NavigationItem.LeftBarButtonItem, $"btn_{FlyoutPage.AutomationId}");
#if __MOBILE__
				containerController.NavigationItem.LeftBarButtonItem.SetAccessibilityHint(FlyoutPage);
				containerController.NavigationItem.LeftBarButtonItem.SetAccessibilityLabel(FlyoutPage);
#endif
			});

			void OnItemTapped(object sender, EventArgs e)
			{
				FlyoutPage.IsPresented = !FlyoutPage.IsPresented;
			}
		}

		static void SetAccessibilityHint(UIBarButtonItem uIBarButtonItem, Element element)
		{
			if (element == null)
				return;

			if (_defaultAccessibilityHint == null)
				_defaultAccessibilityHint = uIBarButtonItem.AccessibilityHint;

			uIBarButtonItem.AccessibilityHint = (string)element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityHint;
		}

		static void SetAccessibilityLabel(UIBarButtonItem uIBarButtonItem, Element element)
		{
			if (element == null)
				return;

			if (_defaultAccessibilityLabel == null)
				_defaultAccessibilityLabel = uIBarButtonItem.AccessibilityLabel;

			uIBarButtonItem.AccessibilityLabel = (string)element.GetValue(AutomationProperties.NameProperty) ?? _defaultAccessibilityLabel;
		}

		static void SetIsAccessibilityElement(UIBarButtonItem uIBarButtonItem, Element element)
		{
			if (element == null)
				return;

			if (!_defaultIsAccessibilityElement.HasValue)
				_defaultIsAccessibilityElement = uIBarButtonItem.IsAccessibilityElement;

			uIBarButtonItem.IsAccessibilityElement = (bool)((bool?)element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) ?? _defaultIsAccessibilityElement);
		}

		static void SetAccessibilityElementsHidden(UIBarButtonItem uIBarButtonItem, Element element)
		{
			if (element == null)
				return;

			if (!_defaultAccessibilityElementsHidden.HasValue)
				_defaultAccessibilityElementsHidden = uIBarButtonItem.AccessibilityElementsHidden;

			uIBarButtonItem.AccessibilityElementsHidden = (bool)((bool?)element.GetValue(AutomationProperties.ExcludedWithChildrenProperty) ?? _defaultAccessibilityElementsHidden);
		}

		static void SetAutomationId(UIBarButtonItem uIBarButtonItem, string id)
		{
			uIBarButtonItem.AccessibilityIdentifier = id;
		}

		static string _defaultAccessibilityLabel;
		static string _defaultAccessibilityHint;
		static bool? _defaultIsAccessibilityElement;
		static bool? _defaultAccessibilityElementsHidden;

		internal void ValidateInsets()
		{
			nfloat navBottom = NavigationBar.Frame.Bottom;

			if (_navigationBottom != navBottom && Current != null)
				ViewDidLayoutSubviews();
		}

		class SecondaryToolbar : UIToolbar
		{
			readonly List<UIView> _lines = new List<UIView>();

			public SecondaryToolbar()
			{
				TintColor = UIColor.White;
			}

			public override UIBarButtonItem[] Items
			{
				get { return base.Items; }
				set
				{
					base.Items = value;
					SetupLines();
				}
			}

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				if (Items == null || Items.Length == 0)
					return;
				LayoutToolbarItems(Bounds.Width, Bounds.Height, 0);
			}

			void LayoutToolbarItems(nfloat toolbarWidth, nfloat toolbarHeight, nfloat padding)
			{
				var x = padding;
				var y = 0;
				var itemH = toolbarHeight;
				var itemW = toolbarWidth / Items.Length;

				foreach (var item in Items)
				{
					var frame = new RectangleF(x, y, itemW, itemH);
					if (frame == item.CustomView.Frame)
						continue;
					item.CustomView.Frame = frame;
					x += itemW + padding;
				}

				x = itemW + padding * 1.5f;
				y = (int)Bounds.GetMidY();
				foreach (var l in _lines)
				{
					l.Center = new PointF(x, y);
					x += itemW + padding;
				}
			}

			void SetupLines()
			{
				_lines.ForEach(l => l.RemoveFromSuperview());
				_lines.Clear();
				if (Items == null)
					return;
				for (var i = 1; i < Items.Length; i++)
				{
					var l = new UIView(new RectangleF(0, 0, 1, 24)) { BackgroundColor = new UIColor(0, 0, 0, 0.2f) };
					AddSubview(l);
					_lines.Add(l);
				}
			}
		}

		class ParentingViewController : UIViewController
		{
			readonly WeakReference<NavigationRenderer> _navigation;

			Page _child;
			bool _disposed;
			ToolbarTracker _tracker = new ToolbarTracker();

			public ParentingViewController(NavigationRenderer navigation)
			{
#pragma warning disable CA1416, CA1422 // TODO: 'UIViewController.AutomaticallyAdjustsScrollViewInsets' is unsupported on: 'ios' 11.0 and later
				AutomaticallyAdjustsScrollViewInsets = false;
#pragma warning restore CA1416, CA1422

				_navigation = new WeakReference<NavigationRenderer>(navigation);
			}

			public Page Child
			{
				get { return _child; }
				set
				{
					if (_child == value)
						return;

					if (_child != null)
						_child.PropertyChanged -= HandleChildPropertyChanged;

					_child = value;

					if (_child != null)
						_child.PropertyChanged += HandleChildPropertyChanged;

					UpdateHasBackButton();
					UpdateLargeTitles();
					UpdateIconColor();
				}
			}

			public event EventHandler Appearing;

			[UnsupportedOSPlatform("ios8.0")]
			[UnsupportedOSPlatform("tvos")]
			public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
			{
				base.DidRotate(fromInterfaceOrientation);

				View.SetNeedsLayout();
			}

			public event EventHandler Disappearing;

			public override void ViewDidAppear(bool animated)
			{
				base.ViewDidAppear(animated);

				Appearing?.Invoke(this, EventArgs.Empty);
			}

			public override void ViewDidDisappear(bool animated)
			{
				base.ViewDidDisappear(animated);

				// force a redraw for right toolbar items by resetting TintColor to prevent
				// toolbar items being grayed out when canceling swipe to a previous page
				foreach (var item in NavigationItem?.RightBarButtonItems)
				{
					if (item.Image != null)
						continue;

					var tintColor = item.TintColor;
					item.TintColor = tintColor == null ? UIColor.Clear : null;
					item.TintColor = tintColor;
				}

				Disappearing?.Invoke(this, EventArgs.Empty);
			}

			public override void ViewWillLayoutSubviews()
			{
				base.ViewWillLayoutSubviews();

				NavigationRenderer n;
				if (_navigation.TryGetTarget(out n))
					n.ValidateInsets();
			}

			public override void ViewDidLayoutSubviews()
			{
				IVisualElementRenderer childRenderer;
				if (Child != null && (childRenderer = Platform.GetRenderer(Child)) != null)
					childRenderer.NativeView.Frame = Child.Bounds.ToRectangleF();
				base.ViewDidLayoutSubviews();
			}

			public override void ViewDidLoad()
			{
				base.ViewDidLoad();

				_tracker.Target = Child;
				_tracker.AdditionalTargets = Child.GetParentPages();
				_tracker.CollectionChanged += TrackerOnCollectionChanged;

				UpdateToolbarItems();
			}

			public override void ViewWillAppear(bool animated)
			{
				SetupDefaultNavigationBarAppearance();
				UpdateNavigationBarVisibility(animated);

				NavigationRenderer n;
				var isTranslucent = false;
				if (_navigation.TryGetTarget(out n))
					isTranslucent = n.NavigationBar.Translucent;
				EdgesForExtendedLayout = isTranslucent ? UIRectEdge.All : UIRectEdge.None;

				base.ViewWillAppear(animated);
			}

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;

				if (disposing)
				{
					if (Child != null)
					{
						Child.SendDisappearing();
						Child.PropertyChanged -= HandleChildPropertyChanged;
						Child = null;
					}

					_tracker.Target = null;
					_tracker.CollectionChanged -= TrackerOnCollectionChanged;
					_tracker = null;

					if (NavigationItem.TitleView != null)
					{
						NavigationItem.TitleView.Dispose();
						NavigationItem.TitleView = null;
					}

					if (NavigationItem.RightBarButtonItems != null)
					{
						for (var i = 0; i < NavigationItem.RightBarButtonItems.Length; i++)
							NavigationItem.RightBarButtonItems[i].Dispose();
					}

					if (ToolbarItems != null)
					{
						for (var i = 0; i < ToolbarItems.Length; i++)
							ToolbarItems[i].Dispose();
					}
				}

				base.Dispose(disposing);
			}

			void HandleChildPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == NavigationPage.HasNavigationBarProperty.PropertyName)
					UpdateNavigationBarVisibility(true);
				else if (e.PropertyName == Page.TitleProperty.PropertyName)
					NavigationItem.Title = Child.Title;
				else if (e.PropertyName == NavigationPage.HasBackButtonProperty.PropertyName)
					UpdateHasBackButton();
				else if (e.PropertyName == PrefersStatusBarHiddenProperty.PropertyName)
					UpdatePrefersStatusBarHidden();
				else if (e.PropertyName == LargeTitleDisplayProperty.PropertyName)
					UpdateLargeTitles();
				else if (e.PropertyName == NavigationPage.TitleIconImageSourceProperty.PropertyName ||
					 e.PropertyName == NavigationPage.TitleViewProperty.PropertyName)
					UpdateTitleArea(Child);
				else if (e.PropertyName == NavigationPage.IconColorProperty.PropertyName)
					UpdateIconColor();
			}

			internal void SetupDefaultNavigationBarAppearance()
			{
				if (!Forms.IsiOS13OrNewer)
					return;

				if (!_navigation.TryGetTarget(out NavigationRenderer navigationRenderer))
					return;

				// We will use UINavigationBar.Appareance to infer settings that
				// were already set to navigation bar in older versions of
				// iOS.
				var navBar = navigationRenderer.NavigationBar;
				var navAppearance = navBar.StandardAppearance;

				if (navAppearance.BackgroundColor == null)
				{
					var backgroundColor = navBar.BarTintColor;
					navBar.CompactAppearance.BackgroundColor = navBar.StandardAppearance.BackgroundColor = navBar.ScrollEdgeAppearance.BackgroundColor = backgroundColor;
				}

				if (navAppearance.BackgroundImage == null)
				{
					var backgroundImage = navBar.GetBackgroundImage(UIBarMetrics.Default);
					navBar.CompactAppearance.BackgroundImage = navBar.StandardAppearance.BackgroundImage = navBar.ScrollEdgeAppearance.BackgroundImage = backgroundImage;
				}

				if (navAppearance.ShadowImage == null)
				{
					var shadowImage = navBar.ShadowImage;
					navBar.CompactAppearance.ShadowImage = navBar.StandardAppearance.ShadowImage = navBar.ScrollEdgeAppearance.ShadowImage = shadowImage;

					if (shadowImage != null && shadowImage.Size == SizeF.Empty)
						navBar.CompactAppearance.ShadowColor = navBar.StandardAppearance.ShadowColor = navBar.ScrollEdgeAppearance.ShadowColor = UIColor.Clear;
				}

				UIImage backIndicatorImage = navBar.BackIndicatorImage;
				UIImage backIndicatorTransitionMaskImage = navBar.BackIndicatorTransitionMaskImage;

				if (backIndicatorImage != null && backIndicatorImage.Size == SizeF.Empty)
					backIndicatorImage = GetEmptyBackIndicatorImage();

				if (backIndicatorTransitionMaskImage != null && backIndicatorTransitionMaskImage.Size == SizeF.Empty)
					backIndicatorTransitionMaskImage = GetEmptyBackIndicatorImage();

				navBar.CompactAppearance.SetBackIndicatorImage(backIndicatorImage, backIndicatorTransitionMaskImage);
				navBar.StandardAppearance.SetBackIndicatorImage(backIndicatorImage, backIndicatorTransitionMaskImage);
				navBar.ScrollEdgeAppearance.SetBackIndicatorImage(backIndicatorImage, backIndicatorTransitionMaskImage);
			}

			UIImage GetEmptyBackIndicatorImage()
			{
				var rect = RectangleF.Empty;
				var size = rect.Size;

				UIGraphics.BeginImageContext(size);
				var context = UIGraphics.GetCurrentContext();
				context?.SetFillColor(1, 1, 1, 0);
				context?.FillRect(rect);

				var empty = UIGraphics.GetImageFromCurrentImageContext();
				context?.Dispose();

				return empty;
			}

			internal void UpdateLeftBarButtonItem(Page pageBeingRemoved = null)
			{
				NavigationRenderer n;
				if (!_navigation.TryGetTarget(out n))
					return;

				var currentChild = this.Child;
				var firstPage = n.NavPageController.Pages.FirstOrDefault();


				if (n._parentFlyoutPage == null)
					return;

				if (firstPage != pageBeingRemoved && currentChild != firstPage && NavigationPage.GetHasBackButton(currentChild))
				{
					NavigationItem.LeftBarButtonItem = null;
					return;
				}

				SetFlyoutLeftBarButton(this, n._parentFlyoutPage);
			}


			public bool NeedsTitleViewContainer(Page page) => NavigationPage.GetTitleIconImageSource(page) != null || NavigationPage.GetTitleView(page) != null;

			internal void UpdateBackButtonTitle(Page page) => UpdateBackButtonTitle(page.Title, NavigationPage.GetBackButtonTitle(page));

			internal void UpdateBackButtonTitle(string title, string backButtonTitle)
			{
				if (!string.IsNullOrWhiteSpace(title))
					NavigationItem.Title = title;

				if (backButtonTitle != null)
					// adding a custom event handler to UIBarButtonItem for navigating back seems to be ignored.
					NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = backButtonTitle, Style = UIBarButtonItemStyle.Plain };
				else
					NavigationItem.BackBarButtonItem = null;
			}

			internal void UpdateTitleArea(Page page)
			{
				if (page == null)
					return;

				ImageSource titleIcon = NavigationPage.GetTitleIconImageSource(page);
				View titleView = NavigationPage.GetTitleView(page);
				bool needContainer = titleView != null || titleIcon != null;

				string backButtonText = NavigationPage.GetBackButtonTitle(page);
				bool isBackButtonTextSet = page.IsSet(NavigationPage.BackButtonTitleProperty);

				// on iOS 10 if the user hasn't set the back button text
				// we set it to an empty string so it's consistent with iOS 11
				if (!Forms.IsiOS11OrNewer && !isBackButtonTextSet)
					backButtonText = "";

				// First page and we have a flyout detail to contend with
				UpdateLeftBarButtonItem();
				UpdateBackButtonTitle(page.Title, backButtonText);

				//var hadTitleView = NavigationItem.TitleView != null;
				ClearTitleViewContainer();
				if (needContainer)
				{
					NavigationRenderer n;
					if (!_navigation.TryGetTarget(out n))
						return;

					Container titleViewContainer = new Container(titleView, n.NavigationBar);

					UpdateTitleImage(titleViewContainer, titleIcon);
					NavigationItem.TitleView = titleViewContainer;
				}
			}

			void UpdateIconColor()
			{
				if (_navigation.TryGetTarget(out NavigationRenderer navigationRenderer))
					navigationRenderer.UpdateBarTextColor();
			}

			async void UpdateTitleImage(Container titleViewContainer, ImageSource titleIcon)
			{
				if (titleViewContainer == null)
					return;

				if (titleIcon == null || titleIcon.IsEmpty)
				{
					titleViewContainer.Icon = null;
				}
				else
				{
					var image = await titleIcon.GetNativeImageAsync();
					try
					{
						titleViewContainer.Icon = new UIImageView(image);
					}
					catch
					{
						//UIImage ctor throws on file not found if MonoTouch.ObjCRuntime.Class.ThrowOnInitFailure is true;
					}
				}
			}

			void ClearTitleViewContainer()
			{
				if (NavigationItem.TitleView != null && NavigationItem.TitleView is Container titleViewContainer)
				{
					titleViewContainer.Dispose();
					titleViewContainer = null;
					NavigationItem.TitleView = null;
				}
			}

			void UpdatePrefersStatusBarHidden()
			{
				View.SetNeedsLayout();
				ParentViewController?.View.SetNeedsLayout();
			}

			void TrackerOnCollectionChanged(object sender, EventArgs eventArgs)
			{
				UpdateToolbarItems();
			}

			void UpdateHasBackButton()
			{
				if (Child == null || NavigationItem.HidesBackButton == !NavigationPage.GetHasBackButton(Child))
					return;

				NavigationItem.HidesBackButton = !NavigationPage.GetHasBackButton(Child);

				NavigationRenderer n;
				if (!_navigation.TryGetTarget(out n))
					return;

				if (!Forms.IsiOS11OrNewer || n._parentFlyoutPage != null)
					UpdateTitleArea(Child);
			}

			void UpdateNavigationBarVisibility(bool animated)
			{
				var current = Child;

				if (current == null || NavigationController == null)
					return;

				var hasNavBar = NavigationPage.GetHasNavigationBar(current);

				if (NavigationController.NavigationBarHidden == hasNavBar)
				{
					// prevent bottom content "jumping"
					current.IgnoresContainerArea = !hasNavBar;
					NavigationController.SetNavigationBarHidden(!hasNavBar, animated);
				}
			}

			void UpdateToolbarItems()
			{
				if (NavigationItem.RightBarButtonItems != null)
				{
					for (var i = 0; i < NavigationItem.RightBarButtonItems.Length; i++)
						NavigationItem.RightBarButtonItems[i].Dispose();
				}
				if (ToolbarItems != null)
				{
					for (var i = 0; i < ToolbarItems.Length; i++)
						ToolbarItems[i].Dispose();
				}

				List<UIBarButtonItem> primaries = null;
				List<UIBarButtonItem> secondaries = null;
				var toolbarItems = _tracker.ToolbarItems;
				foreach (var item in toolbarItems)
				{
					if (item.Order == ToolbarItemOrder.Secondary)
						(secondaries = secondaries ?? new List<UIBarButtonItem>()).Add(item.ToUIBarButtonItem(true));
					else
						(primaries = primaries ?? new List<UIBarButtonItem>()).Add(item.ToUIBarButtonItem());
				}

				if (primaries != null)
					primaries.Reverse();
				NavigationItem.SetRightBarButtonItems(primaries == null ? Array.Empty<UIBarButtonItem>() : primaries.ToArray(), false);
				ToolbarItems = secondaries == null ? Array.Empty<UIBarButtonItem>() : secondaries.ToArray();

				NavigationRenderer n;
				if (_navigation.TryGetTarget(out n))
					n.UpdateToolBarVisible();
			}

			void UpdateLargeTitles()
			{
				var page = Child;
				if (page != null && Forms.IsiOS11OrNewer)
				{
					var largeTitleDisplayMode = page.OnThisPlatform().LargeTitleDisplay();
					switch (largeTitleDisplayMode)
					{
						case LargeTitleDisplayMode.Always:
							NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Always;
							break;
						case LargeTitleDisplayMode.Automatic:
							NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
							break;
						case LargeTitleDisplayMode.Never:
							NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;
							break;
					}
				}
			}

			public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
			{
				IVisualElementRenderer childRenderer;
				if (Child != null && (childRenderer = Platform.GetRenderer(Child)) != null)
					return childRenderer.ViewController.GetSupportedInterfaceOrientations();
				return base.GetSupportedInterfaceOrientations();
			}

			public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
			{
				IVisualElementRenderer childRenderer;
				if (Child != null && (childRenderer = Platform.GetRenderer(Child)) != null)
					return childRenderer.ViewController.PreferredInterfaceOrientationForPresentation();
				return base.PreferredInterfaceOrientationForPresentation();
			}
#pragma warning disable CA1416, CA1422 // TODO: ShouldAutorotateToInterfaceOrientation(...) has [UnsupportedOSPlatform("ios6.0")]
			public override bool ShouldAutorotate()
			{
				IVisualElementRenderer childRenderer;
				if (Child != null && (childRenderer = Platform.GetRenderer(Child)) != null)
					return childRenderer.ViewController.ShouldAutorotate();
				return base.ShouldAutorotate();
			}

			public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
			{
				IVisualElementRenderer childRenderer;

				if (Child != null && (childRenderer = Platform.GetRenderer(Child)) != null)
					return childRenderer.ViewController.ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
				return base.ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
			}
#pragma warning restore CA1416, CA1422
			public override bool ShouldAutomaticallyForwardRotationMethods => true;

			public override async void DidMoveToParentViewController(UIViewController parent)
			{
				//we are being removed from the UINavigationPage
				if (parent == null)
				{
					NavigationRenderer navRenderer;
					if (_navigation.TryGetTarget(out navRenderer))
						await navRenderer.UpdateFormsInnerNavigation(Child);
				}
				base.DidMoveToParentViewController(parent);
			}
		}

		public override UIViewController ChildViewControllerForStatusBarHidden()
		{
			return Platform.GetRenderer(Current)?.ViewController ??
				(Current.Handler as IPlatformViewHandler)?.ViewController;
		}

		public override UIViewController ChildViewControllerForHomeIndicatorAutoHidden =>
			ChildViewControllerForStatusBarHidden();

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, View);
		}

		internal class FormsNavigationBar : UINavigationBar
		{
			[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
			public FormsNavigationBar() : base()
			{
			}

			[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
			public FormsNavigationBar(Foundation.NSCoder coder) : base(coder)
			{
			}

			[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
			protected FormsNavigationBar(Foundation.NSObjectFlag t) : base(t)
			{
			}

			[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
			protected internal FormsNavigationBar(NativeHandle handle) : base(handle)
			{
			}

			[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
			public FormsNavigationBar(RectangleF frame) : base(frame)
			{
			}

			public RectangleF BackButtonFrameSize { get; private set; }
			public UILabel NavBarLabel { get; private set; }

			public override void LayoutSubviews()
			{
				if (!Forms.IsiOS11OrNewer)
				{
					for (int i = 0; i < this.Subviews.Length; i++)
					{
						if (Subviews[i] is UIView view)
						{
							if (view.Class.Name == "_UINavigationBarBackIndicatorView")
							{
								if (view.Alpha == 0)
									BackButtonFrameSize = CGRect.Empty;
								else
									BackButtonFrameSize = view.Frame;

								break;
							}
							else if (view.Class.Name == "UINavigationItemButtonView")
							{
								if (view.Subviews.Length == 0)
									NavBarLabel = null;
								else if (view.Subviews[0] is UILabel titleLabel)
									NavBarLabel = titleLabel;
							}
						}
					}
				}

				base.LayoutSubviews();
			}
		}

		class Container : UIView
		{
			View _view;
			FormsNavigationBar _bar;
			IVisualElementRenderer _child;
			UIImageView _icon;
			bool _disposed;

			public Container(View view, UINavigationBar bar) : base(bar.Bounds)
			{
				if (Forms.IsiOS11OrNewer)
				{
					TranslatesAutoresizingMaskIntoConstraints = false;
				}
				else
				{
					TranslatesAutoresizingMaskIntoConstraints = true;
					AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
				}

				_bar = bar as FormsNavigationBar;
				if (view != null)
				{
					_view = view;
					_child = Platform.CreateRenderer(view);
					Platform.SetRenderer(view, _child);
					AddSubview(_child.NativeView);
				}

				ClipsToBounds = true;
			}

			public override CGSize IntrinsicContentSize => UILayoutFittingExpandedSize;

			nfloat IconHeight => _icon?.Frame.Height ?? 0;
			nfloat IconWidth => _icon?.Frame.Width ?? 0;

			// Navigation bar will not stretch past these values. Prevent content clipping.
			// iOS11 does this for us automatically, but apparently iOS10 doesn't.
			nfloat ToolbarHeight
			{
				get
				{
					if (Superview?.Bounds.Height > 0)
						return Superview.Bounds.Height;

					return (DeviceInfo.Idiom == DeviceIdiom.Phone && DeviceDisplay.MainDisplayInfo.Orientation.IsLandscape()) ? 32 : 44;
				}
			}

			public override CGRect Frame
			{
				get => base.Frame;
				set
				{
					if (Superview != null)
					{
						if (!Forms.IsiOS11OrNewer)
						{
							value.Y = Superview.Bounds.Y;

							if (_bar != null && String.IsNullOrWhiteSpace(_bar.NavBarLabel?.Text) && _bar.BackButtonFrameSize != RectangleF.Empty)
							{
								var xSpace = _bar.BackButtonFrameSize.Width + (_bar.BackButtonFrameSize.X * 2);
								value.Width = (value.X - xSpace) + value.Width;
								value.X = xSpace;
							}
						}
						;

						value.Height = ToolbarHeight;
					}

					base.Frame = value;
				}
			}

			public UIImageView Icon
			{
				set
				{
					if (_icon != null)
						_icon.RemoveFromSuperview();

					_icon = value;

					if (_icon != null)
						AddSubview(_icon);
				}
			}

			public override SizeF SizeThatFits(SizeF size)
			{
				return new SizeF(size.Width, ToolbarHeight);
			}

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				if (Frame == CGRect.Empty || Frame.Width >= 10000 || Frame.Height >= 10000)
					return;

				nfloat toolbarHeight = ToolbarHeight;

				double height = Math.Min(toolbarHeight, Bounds.Height);

				if (_icon != null)
					_icon.Frame = new RectangleF(0, 0, IconWidth, Math.Min(toolbarHeight, IconHeight));

				if (_child?.Element != null)
				{
					Rect layoutBounds = new Rect(IconWidth, 0, Bounds.Width - IconWidth, height);
					if (_child.Element.Bounds != layoutBounds)
						Layout.LayoutChildIntoBoundingRegion(_child.Element, layoutBounds);
				}
				else if (_icon != null && Superview != null)
				{
					_icon.Center = new PointF(Superview.Frame.Width / 2 - Frame.X, Superview.Frame.Height / 2);
				}
			}

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;

				if (disposing)
				{

					if (_child != null)
					{
						_child.Element?.DisposeModalAndChildRenderers();
						_child.NativeView.RemoveFromSuperview();
						_child.Dispose();
						_child = null;
					}

					_view = null;

					_icon?.Dispose();
					_icon = null;
				}

				base.Dispose(disposing);
			}
		}
	}
}
