using CoreGraphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ObjCRuntime;
using UIKit;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using static Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page;
using static Xamarin.Forms.PlatformConfiguration.iOSSpecific.NavigationPage;
using PageUIStatusBarAnimation = Xamarin.Forms.PlatformConfiguration.iOSSpecific.UIStatusBarAnimation;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Xamarin.Forms.Platform.iOS
{
	public class NavigationRenderer : UINavigationController, IVisualElementRenderer, IEffectControlProvider
	{
		internal const string UpdateToolbarButtons = "Xamarin.UpdateToolbarButtons";
		bool _appeared;
		bool _ignorePopCall;
		bool _loaded;
		MasterDetailPage _parentMasterDetailPage;
		Size _queuedSize;
		UIViewController[] _removeControllers;
		UIToolbar _secondaryToolbar;
		VisualElementTracker _tracker;
		nfloat _navigationBottom = 0;
		bool _hasNavigationBar;
		UIImage _defaultNavBarShadowImage;
		UIImage _defaultNavBarBackImage;

		public NavigationRenderer()
		{
			MessagingCenter.Subscribe<IVisualElementRenderer>(this, UpdateToolbarButtons, sender =>
			{
				if (!ViewControllers.Any())
					return;
				var parentingViewController = (ParentingViewController)ViewControllers.Last();
				UpdateLeftBarButtonItem(parentingViewController);
			});
		}

		Page Current { get; set; }

		IPageController PageController => Element as IPageController;

		NavigationPage NavPage => Element as NavigationPage;

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
				Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
			else
				_queuedSize = size;
		}

		public UIViewController ViewController
		{
			get { return this; }
		}

		//TODO: this was deprecated in iOS8.0 and is not called in 9.0+
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);

			View.SetNeedsLayout();

			var parentingViewController = (ParentingViewController)ViewControllers.Last();
			UpdateLeftBarButtonItem(parentingViewController);
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
				new Rectangle(0, toolbar.Hidden ? 0 : toolbar.Frame.Height, modelSize.Width, modelSize.Height - trueBottom);

			if (!_queuedSize.IsZero)
			{
				Element.Layout(new Rectangle(Element.X, Element.Y, _queuedSize.Width, _queuedSize.Height));
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

			FindParentMasterDetail();

			var navPage = NavPage;

			if (navPage.CurrentPage == null)
			{
				throw new InvalidOperationException(
					"NavigationPage must have a root Page before being used. Either call PushAsync with a valid Page, or pass a Page to the constructor before usage.");
			}

			navPage.PushRequested += OnPushRequested;
			navPage.PopRequested += OnPopRequested;
			navPage.PopToRootRequested += OnPopToRootRequested;
			navPage.RemovePageRequested += OnRemovedPageRequested;
			navPage.InsertPageBeforeRequested += OnInsertPageBeforeRequested;

			UpdateTint();
			UpdateBarBackgroundColor();
			UpdateBarTextColor();
			UpdateUseLargeTitles();
			UpdateHideNavigationBarSeparator();

			// If there is already stuff on the stack we need to push it
			navPage.Pages.ForEach(async p => await PushPageAsync(p, false));

			_tracker = new VisualElementTracker(this);

			Element.PropertyChanged += HandlePropertyChanged;

			UpdateToolBarVisible();
			UpdateBackgroundColor();
			Current = navPage.CurrentPage;
		}

		protected override void Dispose(bool disposing)
		{
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

				_parentMasterDetailPage = null;
				Current = null; // unhooks events

				var navPage = NavPage;
				navPage.PropertyChanged -= HandlePropertyChanged;

				navPage.PushRequested -= OnPushRequested;
				navPage.PopRequested -= OnPopRequested;
				navPage.PopToRootRequested -= OnPopToRootRequested;
				navPage.RemovePageRequested -= OnRemovedPageRequested;
				navPage.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;
			}

			base.Dispose(disposing);
			if (_appeared)
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

			var actuallyRemoved = false;

			if (page != ((ParentingViewController)TopViewController).Child)
				throw new NotSupportedException("Popped page does not appear on top of current navigation stack, please file a bug.");

			var task = GetAppearedOrDisappearedTask(page);

			UIViewController poppedViewController;
			poppedViewController = base.PopViewController(animated);

			actuallyRemoved = (poppedViewController == null) ? true : !await task;

			poppedViewController?.Dispose();

			UpdateToolBarVisible();
			return actuallyRemoved;
		}

		protected virtual async Task<bool> OnPushAsync(Page page, bool animated)
		{
			if (page is MasterDetailPage)
				System.Diagnostics.Trace.WriteLine($"Pushing a {nameof(MasterDetailPage)} onto a {nameof(NavigationPage)} is not a supported UI pattern on iOS. " +
					"Please see https://developer.apple.com/documentation/uikit/uisplitviewcontroller for more details.");

			var pack = CreateViewControllerForPage(page);
			var task = GetAppearedOrDisappearedTask(page);

			PushViewController(pack, animated);

			var shown = await task;
			UpdateToolBarVisible();
			return shown;
		}

		ParentingViewController CreateViewControllerForPage(Page page)
		{
			if (Platform.GetRenderer(page) == null)
				Platform.SetRenderer(page, Platform.CreateRenderer(page));

			// must pack into container so padding can work
			// otherwise the view controller is forced to 0,0
			var pack = new ParentingViewController(this) { Child = page };

			UpdateTitleArea(pack, page);

			var pageRenderer = Platform.GetRenderer(page);
			pack.View.AddSubview(pageRenderer.ViewController.View);
			pack.AddChildViewController(pageRenderer.ViewController);
			pageRenderer.ViewController.DidMoveToParentViewController(pack);

			return pack;
		}

		void FindParentMasterDetail()
		{
			Page page = Element as Page;

			var parentPages = page.GetParentPages();
			var masterDetail = parentPages.OfType<MasterDetailPage>().FirstOrDefault();

			if (masterDetail != null && parentPages.Append((Page)Element).Contains(masterDetail.Detail))
				_parentMasterDetailPage = masterDetail;
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
#pragma warning disable 0618 //retaining legacy call to obsolete code
			if (e.PropertyName == NavigationPage.TintProperty.PropertyName)
#pragma warning restore 0618
			{
				UpdateTint();
			}
			else if (e.PropertyName == NavigationPage.BarBackgroundColorProperty.PropertyName)
			{
				UpdateBarBackgroundColor();
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
				UpdateTitleArea(pack, pack.Child);
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
			if(_hasNavigationBar != NavigationPage.GetHasNavigationBar(newCurrentPage))
				ViewDidLayoutSubviews();
		}

		void UpdateHideNavigationBarSeparator()
		{
			bool shouldHide = NavPage.OnThisPlatform().HideNavigationBarSeparator();

			// Just setting the ShadowImage is good for iOS11
			if (_defaultNavBarShadowImage == null)
				_defaultNavBarShadowImage = NavigationBar.ShadowImage;

			if (shouldHide)
				NavigationBar.ShadowImage = new UIImage();
			else
				NavigationBar.ShadowImage = _defaultNavBarShadowImage;

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

		void UpdateUseLargeTitles()
		{
			if (Forms.IsiOS11OrNewer && NavPage != null)
				NavigationBar.PrefersLargeTitles = NavPage.OnThisPlatform().PrefersLargeTitles();
		}

		void UpdateTitleArea(ParentingViewController pack, Page page)
		{
			if (pack == null || page == null)
				return;

			if (!string.IsNullOrWhiteSpace(page.Title))
				pack.NavigationItem.Title = page.Title;

			// First page and we have a master detail to contend with
			UpdateLeftBarButtonItem(pack);

			var titleText = NavigationPage.GetBackButtonTitle(page);
			if (titleText != null)
			{
				// adding a custom event handler to UIBarButtonItem for navigating back seems to be ignored.
				pack.NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = titleText, Style = UIBarButtonItemStyle.Plain };
			}

			FileImageSource titleIcon = NavigationPage.GetTitleIcon(page);
			VisualElement titleView = NavigationPage.GetTitleView(page);

			ClearTitleViewContainer(pack);

			bool needContainer = titleView != null || titleIcon != null;

			if (needContainer)
			{
				Container titleViewContainer = new Container();

				pack.NavigationItem.TitleView = titleViewContainer;

				UpdateTitleImage(titleViewContainer, titleIcon);
				UpdateTitleView(titleViewContainer, titleView);

				// Need to call this for iOS10 to properly frame the renderer
				TopViewController?.NavigationItem?.TitleView?.SizeToFit();
				TopViewController?.NavigationItem?.TitleView?.LayoutSubviews();
			}
		}

		async void UpdateTitleImage(Container titleViewContainer, FileImageSource titleIcon)
		{
			if (titleViewContainer == null)
				return;

			if (string.IsNullOrWhiteSpace(titleIcon))
			{
				titleViewContainer.Icon = null;
			}
			else
			{
				var source = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(titleIcon);
				var image = await source.LoadImageAsync(titleIcon);

				try
				{
					titleViewContainer.Icon = new UIImageView(image) { };
				}
				catch
				{
					//UIImage ctor throws on file not found if MonoTouch.ObjCRuntime.Class.ThrowOnInitFailure is true;
				}
			}
		}

		void ClearTitleViewContainer(UIViewController pack)
		{
			if (pack == null)
				return;

			if (pack.NavigationItem.TitleView != null && pack.NavigationItem.TitleView is Container titleViewContainer)
			{
				titleViewContainer.Dispose();
				titleViewContainer = null;
				pack.NavigationItem.TitleView = null;
			}
		}

		void UpdateTitleView(Container titleViewContainer, VisualElement titleView)
		{
			if (titleViewContainer == null)
				return;

			var titleViewRenderer = titleViewContainer.Child;

			if (titleView != null)
			{
				if (titleViewRenderer != null)
				{
					var rendererType = titleViewRenderer is System.Reflection.IReflectableType reflectableType 
						? reflectableType.GetTypeInfo().AsType() 
						: titleViewRenderer.GetType();

					if (titleView != null && rendererType == Internals.Registrar.Registered.GetHandlerTypeForObject(titleView))
					{
						if (titleViewContainer != null)
							titleViewContainer.Child = null;
						titleViewRenderer.SetElement(titleView);
						return;
					}
					titleViewContainer?.DisposeChild();
				}

				titleViewRenderer = Platform.CreateRenderer(titleView);
				titleViewContainer.Child = titleViewRenderer;
			}
			else if (titleViewRenderer != null)
			{
				titleViewContainer?.DisposeChild();
			}
		}

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
			var parentingViewController = ViewControllers.Last() as ParentingViewController;
			UpdateLeftBarButtonItem(parentingViewController, page);
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
			var color = Element.BackgroundColor == Color.Default ? Color.White : Element.BackgroundColor;
			View.BackgroundColor = color.ToUIColor();
		}

		void UpdateBarBackgroundColor()
		{
			var barBackgroundColor = NavPage.BarBackgroundColor;
			// Set navigation bar background color
			NavigationBar.BarTintColor = barBackgroundColor == Color.Default
				? UINavigationBar.Appearance.BarTintColor
				: barBackgroundColor.ToUIColor();
		}

		void UpdateBarTextColor()
		{
			var barTextColor = NavPage.BarTextColor;

			var globalAttributes = UINavigationBar.Appearance.GetTitleTextAttributes();

			if (barTextColor == Color.Default)
			{
				if (NavigationBar.TitleTextAttributes != null)
				{
					var attributes = new UIStringAttributes();
					attributes.ForegroundColor = globalAttributes.TextColor;
					attributes.Font = globalAttributes.Font;
					NavigationBar.TitleTextAttributes = attributes;
				}
			}
			else
			{
				var titleAttributes = new UIStringAttributes();
				titleAttributes.Font = globalAttributes.Font;
				// TODO: the ternary if statement here will always return false because of the encapsulating if statement.
				// What was the intention?
				titleAttributes.ForegroundColor = barTextColor == Color.Default
					? titleAttributes.ForegroundColor ?? UINavigationBar.Appearance.TintColor
					: barTextColor.ToUIColor();
				NavigationBar.TitleTextAttributes = titleAttributes;
			}

			if (Forms.IsiOS11OrNewer)
			{
				var globalLargeTitleAttributes = UINavigationBar.Appearance.LargeTitleTextAttributes;
				if (globalLargeTitleAttributes == null)
					NavigationBar.LargeTitleTextAttributes = NavigationBar.TitleTextAttributes;
			}

			var statusBarColorMode = NavPage.OnThisPlatform().GetStatusBarTextColorMode();

			// set Tint color (i. e. Back Button arrow and Text)
			NavigationBar.TintColor = barTextColor == Color.Default || statusBarColorMode == StatusBarTextColorMode.DoNotAdjust
				? UINavigationBar.Appearance.TintColor
				: barTextColor.ToUIColor();
		}

		void SetStatusBarStyle()
		{
			var barTextColor = NavPage.BarTextColor;
			var statusBarColorMode = NavPage.OnThisPlatform().GetStatusBarTextColorMode();

			if (statusBarColorMode == StatusBarTextColorMode.DoNotAdjust || barTextColor.Luminosity <= 0.5)
			{
				// Use dark text color for status bar
				UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.Default;
			}
			else
			{
				// Use light text color for status bar
				UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
			}
		}

		void UpdateLeftBarButtonItem(ParentingViewController containerController, Page pageBeingRemoved = null)
		{
			if (containerController == null)
				return;
			var currentChild = containerController.Child;
			var firstPage = NavPage.Pages.FirstOrDefault();
			if ((firstPage != pageBeingRemoved && currentChild != firstPage && NavigationPage.GetHasBackButton(currentChild)) || _parentMasterDetailPage == null)
				return;

			SetMasterLeftBarButton(containerController, _parentMasterDetailPage);
		}

		void UpdateTint()
		{
#pragma warning disable 0618 //retaining legacy call to obsolete code
			var tintColor = NavPage.Tint;
#pragma warning restore 0618
			NavigationBar.BarTintColor = tintColor == Color.Default
				? UINavigationBar.Appearance.BarTintColor
				: tintColor.ToUIColor();
			if (tintColor == Color.Default)
				NavigationBar.TintColor = UINavigationBar.Appearance.TintColor;
			else
				NavigationBar.TintColor = tintColor.Luminosity > 0.5 ? UIColor.Black : UIColor.White;
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
			_ignorePopCall = true;
			if (Element.Navigation.NavigationStack.Contains(pageBeingRemoved))
				await (NavPage as INavigationPageController)?.RemoveAsyncInner(pageBeingRemoved, false, true);
			_ignorePopCall = false;

		}

		internal static async void SetMasterLeftBarButton(UIViewController containerController, MasterDetailPage masterDetailPage)
		{
			if (!masterDetailPage.ShouldShowToolbarButton())
			{
				containerController.NavigationItem.LeftBarButtonItem = null;
				return;
			}

			EventHandler handler = (o, e) => masterDetailPage.IsPresented = !masterDetailPage.IsPresented;

			bool shouldUseIcon = masterDetailPage.Master.Icon != null;
			if (shouldUseIcon)
			{
				try
				{
					var source = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(masterDetailPage.Master.Icon);
					var icon = await source.LoadImageAsync(masterDetailPage.Master.Icon);
					containerController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, handler);
				}
				catch (Exception)
				{
					// Throws Exception otherwise would catch more specific exception type
					shouldUseIcon = false;
				}
			}

			if (!shouldUseIcon)
			{
				containerController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(masterDetailPage.Master.Title, UIBarButtonItemStyle.Plain, handler);
			}
		}

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
			ToolbarTracker _tracker = new ToolbarTracker();

			public ParentingViewController(NavigationRenderer navigation)
			{
				AutomaticallyAdjustsScrollViewInsets = false;

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
				}
			}

			public event EventHandler Appearing;

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
				if (disposing)
				{
					Child.SendDisappearing();

					if (Child != null)
					{
						Child.PropertyChanged -= HandleChildPropertyChanged;
						Child = null;
					}

					_tracker.Target = null;
					_tracker.CollectionChanged -= TrackerOnCollectionChanged;
					_tracker = null;

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
				else if (e.PropertyName == NavigationPage.TitleIconProperty.PropertyName ||
					 e.PropertyName == NavigationPage.TitleViewProperty.PropertyName)
				{
					NavigationRenderer n;
					if (_navigation.TryGetTarget(out n))
						n.UpdateTitleArea(this, Child);
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
				if (Child == null)
					return;

				NavigationItem.HidesBackButton = !NavigationPage.GetHasBackButton(Child);
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
				foreach (var item in _tracker.ToolbarItems)
				{
					if (item.Order == ToolbarItemOrder.Secondary)
						(secondaries = secondaries ?? new List<UIBarButtonItem>()).Add(item.ToUIBarButtonItem(true));
					else
						(primaries = primaries ?? new List<UIBarButtonItem>()).Add(item.ToUIBarButtonItem());
				}

				if (primaries != null)
					primaries.Reverse();
				NavigationItem.SetRightBarButtonItems(primaries == null ? new UIBarButtonItem[0] : primaries.ToArray(), false);
				ToolbarItems = secondaries == null ? new UIBarButtonItem[0] : secondaries.ToArray();

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
			return (UIViewController)Platform.GetRenderer(Current);
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, View);
		}

		class Container : UIView
		{
			IVisualElementRenderer _child;
			UIImageView _icon;

			nfloat IconHeight => _icon?.Frame.Height ?? 0;

			nfloat IconWidth => _icon?.Frame.Width ?? 0;

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					DisposeChild();

					_icon?.Dispose();
					_icon = null;
				}
				base.Dispose(disposing);
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

			public IVisualElementRenderer Child
			{
				get { return _child; }
				set
				{
					if (_child != null)
						DisposeChild();

					_child = value;

					if (value != null)
						AddSubview(value.NativeView);
				}
			}

			public override SizeF SizeThatFits(SizeF size)
			{
				IVisualElementRenderer renderer = _child;

				if (renderer == null || renderer.Element == null || renderer.Element.Parent == null)
					return new SizeF(IconWidth, IconHeight);

				var result = renderer.Element.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);

				return new SizeF(result.Request.Width + IconWidth, Math.Max(IconHeight, result.Request.Height));
			}

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();

				// Navigation bar will not stretch past these values. Prevent content clipping.
				// iOS11 does this for us automatically, but apparently iOS10 doesn't.
				int toolbarHeight = 44;
				if (Device.Idiom == TargetIdiom.Phone && Device.Info.CurrentOrientation.IsLandscape())
					toolbarHeight = 32;

				double height = Math.Min(toolbarHeight, Bounds.Height);

				Frame = new RectangleF(Frame.X, Frame.Y, Bounds.Width, height);

				if (_icon != null)
					_icon.Frame = new RectangleF(0, 0, IconWidth, Math.Min(toolbarHeight, IconHeight));

				if (_child?.Element != null)
					Layout.LayoutChildIntoBoundingRegion(_child.Element, new Rectangle(IconWidth, 0, Bounds.Width - IconWidth, height));
			}

			public void DisposeChild()
			{
				if (_child == null)
					return;

				if (_child.Element.Platform is Platform platform)
					platform.DisposeModelAndChildrenRenderers(_child.Element);

				_child.NativeView.RemoveFromSuperview();
				_child.Dispose();
				_child = null;
			}
		}
	}
}
