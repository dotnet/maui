#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using static Microsoft.Maui.Controls.Compatibility.Platform.iOS.AccessibilityExtensions;
using static Microsoft.Maui.Controls.Compatibility.Platform.iOS.ToolbarItemExtensions;
using static Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage;
using static Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page;
using PageUIStatusBarAnimation = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIStatusBarAnimation;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class NavigationRenderer : UINavigationController, INavigationViewHandler, IPlatformViewHandler
	{
		internal const string UpdateToolbarButtons = "Xamarin.UpdateToolbarButtons";
		bool _appeared;
		bool _ignorePopCall;
		FlyoutPage _parentFlyoutPage;
		UIViewController[] _removeControllers;
		UIToolbar _secondaryToolbar;
		bool _hasNavigationBar;
		UIImage _defaultNavBarShadowImage;
		UIImage _defaultNavBarBackImage;
		bool _disposed;
		IMauiContext _mauiContext;
		IMauiContext MauiContext => _mauiContext;
		public static IPropertyMapper<NavigationPage, NavigationRenderer> Mapper = new PropertyMapper<NavigationPage, NavigationRenderer>(ViewHandler.ViewMapper)
		{
			[PlatformConfiguration.iOSSpecific.NavigationPage.PrefersLargeTitlesProperty.PropertyName] = NavigationPage.MapPrefersLargeTitles,
			[PlatformConfiguration.iOSSpecific.NavigationPage.IsNavigationBarTranslucentProperty.PropertyName] = NavigationPage.MapIsNavigationBarTranslucent,
		};

		public static CommandMapper<NavigationPage, NavigationRenderer> CommandMapper = new CommandMapper<NavigationPage, NavigationRenderer>(ViewHandler.ViewCommandMapper);
		ViewHandlerDelegator<NavigationPage> _viewHandlerWrapper;
		bool _navigating = false;
		WeakReference<VisualElement> _element;
		WeakReference<Page> _current;
		bool _uiRequestedPop; // User tapped the back button or swiped to navigate back
		MauiNavigationDelegate NavigationDelegate => Delegate as MauiNavigationDelegate;

		[Internals.Preserve(Conditional = true)]
		public NavigationRenderer() : base(typeof(MauiControlsNavigationBar), null)
		{
			_viewHandlerWrapper = new ViewHandlerDelegator<NavigationPage>(Mapper, CommandMapper, this);

			Delegate = new MauiNavigationDelegate(this);
		}

		Page Current
		{
			get => _current?.GetTargetOrDefault();
			set => _current = value is null ? null : new(value);
		}

		IPageController PageController => Element as IPageController;

		NavigationPage NavPage => Element as NavigationPage;
		INavigationPageController NavPageController => NavPage;

		public VisualElement Element { get => _viewHandlerWrapper.Element ?? _element?.GetTargetOrDefault(); }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

#pragma warning disable CS0618 // Type or member is obsolete
		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return VisualElementRenderer<NavigationPage>.GetDesiredSize(this, widthConstraint, heightConstraint,
				new Size(0, 0));
		}
#pragma warning restore CS0618 // Type or member is obsolete

		public UIView NativeView
		{
			get { return View; }
		}

		public void SetElement(VisualElement element)
		{
			(this as IElementHandler).SetVirtualView(element);
			_element = element is null ? null : new(element);
		}

		public UIViewController ViewController
		{
			get { return this; }
		}

		//TODO: this was deprecated in iOS8.0 and is not called in 9.0+
		[System.Runtime.Versioning.UnsupportedOSPlatform("ios8.0")]
		[System.Runtime.Versioning.UnsupportedOSPlatform("tvos")]
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
			CompletePendingNavigation(false);

			base.ViewDidDisappear(animated);

			if (!_appeared || Element == null)
				return;

			_appeared = false;
			PageController.SendDisappearing();
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();

			if (Current is not Page current)
				return;

			UpdateToolBarVisible();

			var navBarFrameBottom = Math.Min(NavigationBar.Frame.Bottom, 140);
			var toolbar = _secondaryToolbar;

			//save the state of the Current page we are calculating, this will fire before Current is updated
			_hasNavigationBar = NavigationPage.GetHasNavigationBar(current);

			// Use 0 if the NavBar is hidden or will be hidden
			var toolbarY = NavigationBarHidden || NavigationBar.Translucent || !_hasNavigationBar ? 0 : navBarFrameBottom;
			toolbar.Frame = new RectangleF(0, toolbarY, View.Frame.Width, toolbar.Frame.Height);

			// TODO .NET 10
			// This is required in order to set the "Frame" property on NavigationPage
			// It'd be good to see if we can optimize this a bit better.
			(Element as IView).Arrange(View.Bounds.ToRectangle());
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
				Delegate = null;
				foreach (var childViewController in ViewControllers)
					childViewController.Dispose();

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
			_ = page.ToPlatform(MauiContext);
			var renderer = (IPlatformViewHandler)page.Handler;
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

			_ = page.ToPlatform(MauiContext);
			var renderer = (IPlatformViewHandler)page.Handler;
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

			if (poppedViewController is ParentingViewController pvc)
				pvc.Disconnect(false);
			else
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
#pragma warning restore CA1422 // Validate platform compatibility
			// Make sure the control adheres to changes in UI theme
			if (OperatingSystem.IsIOSVersionAtLeast(13) && previousTraitCollection?.UserInterfaceStyle != TraitCollection.UserInterfaceStyle)
				UpdateBackgroundColor();
		}


		ParentingViewController CreateViewControllerForPage(Page page)
		{
			_ = page.ToPlatform(MauiContext);

			// must pack into container so padding can work
			// otherwise the view controller is forced to 0,0
			var pack = new ParentingViewController(this) { Child = page };

			pack.UpdateTitleArea(page);

			var pageRenderer = (IPlatformViewHandler)page.Handler;
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

		TaskCompletionSource<bool> _pendingNavigationRequest;
		ActionDisposable _removeLifecycleEvents;

		void CompletePendingNavigation(bool success)
		{
			if (_pendingNavigationRequest is null)
				return;

			_removeLifecycleEvents?.Dispose();
			_removeLifecycleEvents = null;

			var pendingNavigationRequest = _pendingNavigationRequest;
			_pendingNavigationRequest = null;

			BeginInvokeOnMainThread(() =>
			{
				pendingNavigationRequest?.TrySetResult(success);
				pendingNavigationRequest = null;
			});
		}

		Task<bool> GetAppearedOrDisappearedTask(Page page)
		{
			CompletePendingNavigation(false);

			_pendingNavigationRequest = new TaskCompletionSource<bool>();

			_ = page.ToPlatform(MauiContext);
			var renderer = (IPlatformViewHandler)page.Handler;
			var parentViewController = renderer.ViewController.ParentViewController as ParentingViewController;
			if (parentViewController == null)
				throw new NotSupportedException("ParentingViewController parent could not be found. Please file a bug.");

			EventHandler appearing = null, disappearing = null;
			appearing = (s, e) =>
			{
				CompletePendingNavigation(true);
			};

			disappearing = (s, e) =>
			{
				CompletePendingNavigation(false);
			};

			if (NavigationDelegate is not null)
				NavigationDelegate.WaitingForNavigationToFinish = true;

			_removeLifecycleEvents = new ActionDisposable(() =>
			{
				// This ensures that we don't cause multiple calls to CompletePendingNavigation.
				// Depending on circumstances (covered by modal page) CompletePendingNavigation 
				// might get called from the Delegate vs the DidAppear/DidDisappear methods
				// on the ParentingViewController.
				parentViewController.Appearing -= appearing;
				parentViewController.Disappearing -= disappearing;
				if (NavigationDelegate is not null)
					NavigationDelegate.WaitingForNavigationToFinish = false;
			});

			parentViewController.Appearing += appearing;
			parentViewController.Disappearing += disappearing;

			return _pendingNavigationRequest.Task;
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
				var current = Current = NavPage?.CurrentPage;
				ValidateNavbarExists(current);
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
			else if (e.PropertyName == NavigationPage.BackButtonTitleProperty.PropertyName || e.PropertyName == NavigationPage.TitleProperty.PropertyName)
			{
				var pack = (ParentingViewController)TopViewController;
				pack?.UpdateTitleArea(pack.Child);
			}
			else if (e.PropertyName == HideNavigationBarSeparatorProperty.PropertyName)
			{
				UpdateHideNavigationBarSeparator();
			}
			else if (e.PropertyName == PrefersHomeIndicatorAutoHiddenProperty.PropertyName)
			{
				UpdateHomeIndicatorAutoHidden();
			}
			else if (e.PropertyName == PrefersStatusBarHiddenProperty.PropertyName)
			{
				UpdateStatusBarHidden();
			}
		}

		void ValidateNavbarExists(Page newCurrentPage)
		{
			//if the last time we did ViewDidLayoutSubviews we had other value for _hasNavigationBar
			//we will need to relayout. This is because Current is updated async of the layout happening
			if (_hasNavigationBar != NavigationPage.GetHasNavigationBar(newCurrentPage))
				View.InvalidateMeasure(Element);
		}

		void UpdateHomeIndicatorAutoHidden()
		{
			if (Element == null)
				return;

			SetNeedsUpdateOfHomeIndicatorAutoHidden();
		}

		void UpdateStatusBarHidden()
		{
			if (Element == null)
				return;

			SetNeedsStatusBarAppearanceUpdate();
		}

		void UpdateHideNavigationBarSeparator()
		{
			bool shouldHide = NavPage.OnThisPlatform().HideNavigationBarSeparator();

			// Just setting the ShadowImage is good for iOS11
			if (_defaultNavBarShadowImage == null)
				_defaultNavBarShadowImage = NavigationBar.ShadowImage;

			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
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

			if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)))
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

		void UpdateUseLargeTitles()
		{
			_viewHandlerWrapper.UpdateProperty(PrefersLargeTitlesProperty.PropertyName);
		}

		void UpdateTranslucent()
		{
			_viewHandlerWrapper.UpdateProperty(IsNavigationBarTranslucentProperty.PropertyName);
		}

		void InsertPageBefore(Page page, Page before)
		{
			if (before.Handler is not IPlatformViewHandler nvh)
				throw new ArgumentNullException(nameof(before));
			if (page == null)
				throw new ArgumentNullException(nameof(page));

			var pageContainer = CreateViewControllerForPage(page);
			var target = nvh.ViewController.ParentViewController;
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
			if (page?.Handler is not IPlatformViewHandler nvh)
				throw new ArgumentNullException(nameof(page));
			if (page == Current)
				throw new NotSupportedException(); // should never happen as NavPage protects against this

			var target = nvh.ViewController.ParentViewController;

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
				BeginInvokeOnMainThread(() => { _removeControllers = null; });
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
			if (controller?.Child is not Page child || child.Handler == null)
				return;

			// Gesture in progress, lets not be proactive and just wait for it to finish
			var task = GetAppearedOrDisappearedTask(child);

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
			var color = Element.BackgroundColor == null ? Maui.Platform.ColorExtensions.BackgroundColor : Element.BackgroundColor.ToPlatform();
			View.BackgroundColor = color;
		}

		void UpdateBarBackground()
		{
			var barBackgroundColor = NavPage.BarBackgroundColor;
			var barBackgroundBrush = NavPage.BarBackground;

			// if the brush has a solid color, treat it as a Color so we can compute the alpha value
			if (NavPage.BarBackground is SolidColorBrush scb)
			{
				barBackgroundColor = scb.Color;
				barBackgroundBrush = null;
			}

			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
			{
				var navigationBarAppearance = NavigationBar.StandardAppearance;
				if (barBackgroundColor is null)
				{
					navigationBarAppearance.ConfigureWithOpaqueBackground();
					navigationBarAppearance.BackgroundColor = Maui.Platform.ColorExtensions.BackgroundColor;

					var parentingViewController = GetParentingViewController();
					parentingViewController?.SetupDefaultNavigationBarAppearance();
				}
				else
				{
					if (barBackgroundColor?.Alpha < 1f)
						navigationBarAppearance.ConfigureWithTransparentBackground();
					else
						navigationBarAppearance.ConfigureWithOpaqueBackground();

					navigationBarAppearance.BackgroundColor = barBackgroundColor.ToPlatform();
				}

				if (barBackgroundBrush is not null)
				{
					var backgroundImage = NavigationBar.GetBackgroundImage(barBackgroundBrush);

					navigationBarAppearance.BackgroundImage = backgroundImage;
				}

				NavigationBar.CompactAppearance = navigationBarAppearance;
				NavigationBar.StandardAppearance = navigationBarAppearance;
				NavigationBar.ScrollEdgeAppearance = navigationBarAppearance;
			}
			else
			{
				if (barBackgroundColor?.Alpha == 0f)
				{
					NavigationBar.SetTransparentNavigationBar();
				}
				else
				{
					// Set navigation bar background color
					NavigationBar.BarTintColor = barBackgroundColor == null
						? UINavigationBar.Appearance.BarTintColor
						: barBackgroundColor.ToPlatform();

					var backgroundImage = NavigationBar.GetBackgroundImage(barBackgroundBrush);
					NavigationBar.SetBackgroundImage(backgroundImage, UIBarMetrics.Default);
				}
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
			if (OperatingSystem.IsIOSVersionAtLeast(11))
			{
				var globalLargeTitleTextAttributes = UINavigationBar.Appearance.LargeTitleTextAttributes;

				largeTitleTextAttributes = new UIStringAttributes
				{
					ForegroundColor = barTextColor == null ? globalLargeTitleTextAttributes?.ForegroundColor : barTextColor.ToPlatform(),
					Font = globalLargeTitleTextAttributes?.Font
				};
			}

			if (OperatingSystem.IsIOSVersionAtLeast(13))
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

				if (OperatingSystem.IsIOSVersionAtLeast(11))
					NavigationBar.LargeTitleTextAttributes = largeTitleTextAttributes;
			}

			// set Tint color (i. e. Back Button arrow and Text)
			var iconColor = Current is Page current ? NavigationPage.GetIconColor(current) : null;
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

#pragma warning disable CA1416, CA1422 // TODO:   'UIApplication.StatusBarStyle' is unsupported on: 'ios' 9.0 and later
			if (statusBarColorMode == StatusBarTextColorMode.DoNotAdjust || barTextColor?.GetLuminosity() <= 0.5)
			{
				// Use dark text color for status bar
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
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

			bool currentHidden = _secondaryToolbar.Hidden;
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

			if (currentHidden != _secondaryToolbar.Hidden)
			{
				if (Current is Page current && current.Handler is not null)
					current.ToPlatform().InvalidateMeasure(current);

				if (VisibleViewController is ParentingViewController pvc)
					pvc.UpdateFrames();
			}

			TopViewController?.NavigationItem?.TitleView?.SizeToFit();
			TopViewController?.NavigationItem?.TitleView?.LayoutSubviews();
		}

		async Task UpdateFormsInnerNavigation(Page pageBeingRemoved)
		{
			if (NavPage == null)
				return;
			if (_ignorePopCall)
				return;

			_ignorePopCall = true;
			if (Element.Navigation.NavigationStack.Contains(pageBeingRemoved))
			{
				await (NavPage as INavigationPageController)?.RemoveAsyncInner(pageBeingRemoved, false, true);
				if (_uiRequestedPop)
				{
					NavPage?.SendNavigatedFromHandler(pageBeingRemoved, NavigationType.Pop);
				}
			}

			_ignorePopCall = false;
			_uiRequestedPop = false;
		}

		[Export("navigationBar:shouldPopItem:")]
		[Internals.Preserve(Conditional = true)]
		internal bool ShouldPopItem(UINavigationBar _, UINavigationItem __)
		{
			_uiRequestedPop = true;
			return true;
		}

		internal static void SetFlyoutLeftBarButton(UIViewController containerController, FlyoutPage FlyoutPage)
		{
			if (!FlyoutPage.ShouldShowToolbarButton())
			{
				containerController.NavigationItem.LeftBarButtonItem = null;
				return;
			}


			FlyoutPage.Flyout.IconImageSource.LoadImage(FlyoutPage.FindMauiContext(), result =>
			{
				var icon = result?.Value;
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

				containerController.NavigationItem.LeftBarButtonItem.SetAccessibilityHint(FlyoutPage);
				containerController.NavigationItem.LeftBarButtonItem.SetAccessibilityLabel(FlyoutPage);
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

#pragma warning disable CS0618 // Type or member is obsolete
			uIBarButtonItem.AccessibilityHint = (string)element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityHint;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		static void SetAccessibilityLabel(UIBarButtonItem uIBarButtonItem, Element element)
		{
			if (element == null)
				return;

			if (_defaultAccessibilityLabel == null)
				_defaultAccessibilityLabel = uIBarButtonItem.AccessibilityLabel;

#pragma warning disable CS0618 // Type or member is obsolete
			uIBarButtonItem.AccessibilityLabel = (string)element.GetValue(AutomationProperties.NameProperty) ?? _defaultAccessibilityLabel;
#pragma warning restore CS0618 // Type or member is obsolete
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

		class MauiNavigationDelegate : UINavigationControllerDelegate
		{
			bool _finishedWithInitialNavigation;
			readonly WeakReference<NavigationRenderer> _navigation;

			public bool WaitingForNavigationToFinish { get; internal set; }

			public MauiNavigationDelegate(NavigationRenderer navigationRenderer)
			{
				_navigation = new WeakReference<NavigationRenderer>(navigationRenderer);
			}

			public override void DidShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
			{
				if (_navigation.TryGetTarget(out NavigationRenderer r))
				{
					r._navigating = false;
					if (r.VisibleViewController is ParentingViewController pvc)
					{
						pvc.UpdateFrames();
					}

					if (r.Element is NavigationPage np && !_finishedWithInitialNavigation)
					{
						_finishedWithInitialNavigation = true;
						np.SendNavigatedFromHandler(null, NavigationType.Push);
					}

					if (WaitingForNavigationToFinish)
						r.CompletePendingNavigation(true);
				}
			}

			public override void WillShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
			{
				if (_navigation.TryGetTarget(out NavigationRenderer r))
				{
					r._navigating = true;
				}
			}
		}

		class ParentingViewController : UIViewController
		{
			readonly WeakReference<NavigationRenderer> _navigation;

			WeakReference<Page> _child;
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
				get => _child?.GetTargetOrDefault();
				set
				{
					var child = Child;
					if (child == value)
						return;

					if (child is not null)
					{
						child.PropertyChanged -= HandleChildPropertyChanged;
					}

					if (value is not null)
					{
						_child = new(value);
						value.PropertyChanged += HandleChildPropertyChanged;
					}
					else
					{
						_child = null;
					}

					UpdateHasBackButton();
					UpdateLargeTitles();
					UpdateIconColor();
				}
			}

			public event EventHandler Appearing;

			[System.Runtime.Versioning.UnsupportedOSPlatform("ios8.0")]
			[System.Runtime.Versioning.UnsupportedOSPlatform("tvos")]
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

			public void UpdateFrames()
			{
				NavigationRenderer n;

				// We only want to update the frame after the navigation has settled
				// The frame bounces a bit during navigation and it messes up 
				// the animation changing the frame during navigation
				if (_navigation.TryGetTarget(out n) &&
					ChildViewControllers.Length > 0 &&
					!n._disposed &&
					!n._navigating
					)
				{
					var vc = ChildViewControllers[^1];

					if (vc is null)
						return;

					var newAdditionalSafeArea = vc.AdditionalSafeAreaInsets;
					var offset = n._secondaryToolbar.Hidden ? 0 : n._secondaryToolbar.Frame.Height;

					if (newAdditionalSafeArea.Top != offset)
					{
						newAdditionalSafeArea.Top = offset;
						vc.AdditionalSafeAreaInsets = newAdditionalSafeArea;
					}
				}
			}

			public override void ViewWillLayoutSubviews()
			{
				base.ViewWillLayoutSubviews();

				var childView = (Child?.Handler as IPlatformViewHandler)?.ViewController?.View;

				if (childView is not null)
				{
					childView.Frame = View.Bounds;
				}
			}

			public override void ViewDidLayoutSubviews()
			{
				base.ViewDidLayoutSubviews();
				UpdateFrames();
			}

			public override void ViewDidLoad()
			{
				base.ViewDidLoad();

				_tracker.Target = Child;
				_tracker.AdditionalTargets = Child.GetParentPages();

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

			public override void WillMoveToParentViewController(UIViewController parent)
			{
				base.WillMoveToParentViewController(parent);

				if (parent is null)
				{
					_tracker.CollectionChanged -= TrackerOnCollectionChanged;
				}
				else
				{
					_tracker.CollectionChanged += TrackerOnCollectionChanged;
				}
			}

			internal void Disconnect(bool dispose)
			{
				if (Child is Page child)
				{
					child.SendDisappearing();
					child.PropertyChanged -= HandleChildPropertyChanged;
					Child = null;
				}

				if (_tracker is not null)
				{
					_tracker.Target = null;
					_tracker.CollectionChanged -= TrackerOnCollectionChanged;
					_tracker = null;
				}

				if (NavigationItem.TitleView is not null)
				{
					if (dispose)
						NavigationItem.TitleView.Dispose();

					NavigationItem.TitleView = null;
				}

				if (NavigationItem.RightBarButtonItems is not null && dispose)
				{
					for (var i = 0; i < NavigationItem.RightBarButtonItems.Length; i++)
						NavigationItem.RightBarButtonItems[i].Dispose();
				}

				if (ToolbarItems is not null && dispose)
				{
					for (var i = 0; i < ToolbarItems.Length; i++)
						ToolbarItems[i].Dispose();
				}

				for (int i = View.Subviews.Length - 1; i >= 0; i--)
				{
					View.Subviews[i].RemoveFromSuperview();
				}


				for (int i = ChildViewControllers.Length - 1; i >= 0; i--)
				{
					var childViewController = ChildViewControllers[i];
					childViewController.View.RemoveFromSuperview();
					childViewController.RemoveFromParentViewController();
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
					Disconnect(true);
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
				if (!(OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13)))
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

			public override void ViewWillTransitionToSize(SizeF toSize, IUIViewControllerTransitionCoordinator coordinator)
			{
				base.ViewWillTransitionToSize(toSize, coordinator);

				if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
					UpdateLeftBarButtonItem();
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
				if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)) && !isBackButtonTextSet)
					backButtonText = "";

				_navigation.TryGetTarget(out NavigationRenderer n);

				// First page and we have a flyout detail to contend with
				UpdateLeftBarButtonItem();
				UpdateBackButtonTitle(page.Title ?? n?.NavPage.Title, backButtonText);

				//var hadTitleView = NavigationItem.TitleView != null;
				ClearTitleViewContainer();
				if (needContainer)
				{
					if (n is null)
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

			void UpdateTitleImage(Container titleViewContainer, ImageSource titleIcon)
			{
				if (titleViewContainer == null)
					return;

				if (titleIcon == null || titleIcon.IsEmpty)
				{
					titleViewContainer.Icon = null;
				}
				else
				{
					if (_navigation.TryGetTarget(out NavigationRenderer n) && n.MauiContext is IMauiContext mc)
					{
						titleIcon.LoadImage(mc, result =>
						{
							var image = result?.Value;
							try
							{
								titleViewContainer.Icon = new UIImageView(image);
							}
							catch
							{
								//UIImage ctor throws on file not found if MonoTouch.ObjCRuntime.Class.ThrowOnInitFailure is true;
							}
						});
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
				SetNeedsStatusBarAppearanceUpdate();
			}

			void TrackerOnCollectionChanged(object sender, EventArgs eventArgs)
			{
				UpdateToolbarItems();
			}

			void UpdateHasBackButton()
			{
				if (Child is not Page child || NavigationItem.HidesBackButton == !NavigationPage.GetHasBackButton(child))
					return;

				NavigationItem.HidesBackButton = !NavigationPage.GetHasBackButton(child);

				NavigationRenderer n;
				if (!_navigation.TryGetTarget(out n))
					return;

				if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)) || n._parentFlyoutPage != null)
					UpdateTitleArea(child);
			}

			void UpdateNavigationBarVisibility(bool animated)
			{
				var current = Child;

				if (current == null || NavigationController == null)
					return;

				var hasNavBar = NavigationPage.GetHasNavigationBar(current);
				if (!_navigation.TryGetTarget(out NavigationRenderer navigationRenderer))
				{
					navigationRenderer._hasNavigationBar = hasNavBar;
				}

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
				if (page != null && OperatingSystem.IsIOSVersionAtLeast(11))
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
				if (Child?.Handler is IPlatformViewHandler ivh)
					return ivh.ViewController.GetSupportedInterfaceOrientations();
				return base.GetSupportedInterfaceOrientations();
			}

			public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
			{
				if (Child?.Handler is IPlatformViewHandler ivh)
					return ivh.ViewController.PreferredInterfaceOrientationForPresentation();
				return base.PreferredInterfaceOrientationForPresentation();
			}
#pragma warning disable CA1422 // Validate platform compatibility
			public override bool ShouldAutorotate()
			{
				if (Child?.Handler is IPlatformViewHandler ivh)

					return ivh.ViewController.ShouldAutorotate();
				return base.ShouldAutorotate();
			}
#pragma warning restore CA1422 // Validate platform compatibility

			[System.Runtime.Versioning.UnsupportedOSPlatform("ios6.0")]
			[System.Runtime.Versioning.UnsupportedOSPlatform("tvos")]
			public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
			{
				if (Child?.Handler is IPlatformViewHandler ivh)
					return ivh.ViewController.ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
				return base.ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
			}

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
			return (Current.Handler as IPlatformViewHandler)?.ViewController;
		}

		public override UIViewController ChildViewControllerForHomeIndicatorAutoHidden =>
			ChildViewControllerForStatusBarHidden();

		bool IViewHandler.HasContainer { get => false; set { } }

		object IViewHandler.ContainerView => null;

		IView IViewHandler.VirtualView => Element;

		object IElementHandler.PlatformView => NativeView;

		Maui.IElement IElementHandler.VirtualView => Element;

		IMauiContext IElementHandler.MauiContext => _mauiContext;

		UIView IPlatformViewHandler.PlatformView => NativeView;

		UIView IPlatformViewHandler.ContainerView => null;

		UIViewController IPlatformViewHandler.ViewController => this;

		IStackNavigationView INavigationViewHandler.VirtualView => NavPage;

		UIView INavigationViewHandler.PlatformView => NativeView;

		Size IViewHandler.GetDesiredSize(double widthConstraint, double heightConstraint) =>
			_viewHandlerWrapper.GetDesiredSize(widthConstraint, heightConstraint);

		void IViewHandler.PlatformArrange(Rect rect) =>
			_viewHandlerWrapper.PlatformArrange(rect);

		void IElementHandler.SetMauiContext(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		void IElementHandler.SetVirtualView(Maui.IElement view)
		{
			_viewHandlerWrapper.SetVirtualView(view, ElementChanged, false);
			_element = view is VisualElement v ? new(v) : null;

			void ElementChanged(ElementChangedEventArgs<NavigationPage> e)
			{
				OnElementChanged(new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
			}
		}

		void IElementHandler.UpdateValue(string property)
		{
			_viewHandlerWrapper.UpdateProperty(property);
		}

		void IElementHandler.Invoke(string command, object args)
		{
			_viewHandlerWrapper.Invoke(command, args);
		}

		void IElementHandler.DisconnectHandler()
		{
			_viewHandlerWrapper.DisconnectHandler();
		}

		internal class MauiControlsNavigationBar : UINavigationBar
		{
			[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
			public MauiControlsNavigationBar() : base()
			{
			}

			[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
			public MauiControlsNavigationBar(Foundation.NSCoder coder) : base(coder)
			{
			}

			[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
			protected MauiControlsNavigationBar(Foundation.NSObjectFlag t) : base(t)
			{
			}

			[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
			protected internal MauiControlsNavigationBar(IntPtr handle) : base(handle)
			{
			}

			[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
			public MauiControlsNavigationBar(RectangleF frame) : base(frame)
			{
			}

			protected internal MauiControlsNavigationBar(NativeHandle handle) : base(handle)
			{
			}

			public RectangleF BackButtonFrameSize { get; private set; }
			public UILabel NavBarLabel { get; private set; }

			public override void LayoutSubviews()
			{
				if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)))
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
			MauiControlsNavigationBar _bar;
			IPlatformViewHandler _child;
			UIImageView _icon;
			bool _disposed;

			public Container(View view, UINavigationBar bar) : base(bar.Bounds)
			{
				if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11))
				{
					TranslatesAutoresizingMaskIntoConstraints = false;
				}
				else
				{
					TranslatesAutoresizingMaskIntoConstraints = true;
					AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
				}

				_bar = bar as MauiControlsNavigationBar;
				if (view != null)
				{
					_view = view;

					if (_view.Parent is null)
					{
						_view.ParentSet += OnTitleViewParentSet;
					}
					else
					{
						SetupTitleView();
					}
				}

				ClipsToBounds = true;
			}

			void OnTitleViewParentSet(object sender, EventArgs e)
			{
				if (sender is View view)
					view.ParentSet -= OnTitleViewParentSet;

				SetupTitleView();
			}

			void SetupTitleView()
			{
				var mauiContext = _view.FindMauiContext();
				if (_view is not null && mauiContext is not null)
				{
					var platformView = _view.ToPlatform(mauiContext);
					_child = (IPlatformViewHandler)_view.Handler;
					AddSubview(platformView);
				}

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
						if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)))
						{
							value.Y = Superview.Bounds.Y;

							if (_bar != null && String.IsNullOrWhiteSpace(_bar.NavBarLabel?.Text) && _bar.BackButtonFrameSize != RectangleF.Empty)
							{
								var xSpace = _bar.BackButtonFrameSize.Width + (_bar.BackButtonFrameSize.X * 2);
								value.Width = (value.X - xSpace) + value.Width;
								value.X = xSpace;
							}
						};

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

				if (_child?.VirtualView != null)
				{
					Rect layoutBounds = new Rect(IconWidth, 0, Bounds.Width - IconWidth, height);

					_child.PlatformArrangeHandler(layoutBounds);
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

					if (_child?.IsConnected() == true)
					{
						(_child.ContainerView ?? _child.PlatformView).RemoveFromSuperview();
						_child.DisconnectHandler();
						_child = null;
					}

					if (_view is not null)
					{
						_view.ParentSet -= OnTitleViewParentSet;
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
