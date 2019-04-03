using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using CoreAnimation;
using Foundation;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.macOSSpecific;

namespace Xamarin.Forms.Platform.MacOS
{
	public class NavigationPageRenderer : NSViewController, IVisualElementRenderer, IEffectControlProvider
	{
		bool _disposed;
		bool _appeared;
		EventTracker _events;
		VisualElementTracker _tracker;
		Stack<NavigationChildPageWrapper> _currentStack = new Stack<NavigationChildPageWrapper>();

		NavigationPage NavigationPage => Element as NavigationPage;

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				platformEffect.SetContainer(View);
		}

		public NavigationPageRenderer() : this(IntPtr.Zero)
		{
		}

		public NavigationPageRenderer(IntPtr handle)
		{
			View = new NSView { WantsLayer = true };
		}

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public NSViewController ViewController => this;

		public NSView NativeView => View;

		public void SetElement(VisualElement element)
		{
			var oldElement = Element;
			Element = element;

			Init();

			RaiseElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);
		}

		public void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public Task<bool> PopToRootAsync(Page page, bool animated = true)
		{
			return OnPopToRoot(page, animated);
		}

		public Task<bool> PopViewAsync(Page page, bool animated = true)
		{
			return OnPop(page, animated);
		}

		public Task<bool> PushPageAsync(Page page, bool animated = true)
		{
			return OnPush(page, animated);
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				if (Element != null)
				{
					if (NavigationPage != null)
					{
						NavigationPage.PushRequested -= OnPushRequested;
						NavigationPage.PopRequested -= OnPopRequested;
						NavigationPage.PopToRootRequested -= OnPopToRootRequested;
						NavigationPage.RemovePageRequested -= OnRemovedPageRequested;
						NavigationPage.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;
						NavigationPage.Popped -= OnPopped;
						NavigationPage.PoppedToRoot -= OnPoppedToRoot;

						NavigationPage.SendDisappearing();
					}
					((Element as IPageContainer<Page>)?.CurrentPage as Page)?.SendDisappearing();
					Element.PropertyChanged -= OnElementPropertyChanged;
					Platform.SetRenderer(Element, null);
					Element = null;
				}

				_tracker?.Dispose();
				_tracker = null;

				_events?.Dispose();
				_events = null;

				if(_currentStack != null)
				{
					foreach (var childPageWrapper in _currentStack)
					{
						childPageWrapper.Dispose();
					}
					_currentStack.Clear();
					_currentStack = null;
				}

				Platform.NativeToolbarTracker.Navigation = null;
				_disposed = true;

			}
			base.Dispose(disposing);
		}

		public override void ViewDidDisappear()
		{
			base.ViewDidDisappear();
			if (!_appeared)
				return;
			Platform.NativeToolbarTracker.TryHide(Element as NavigationPage);
			_appeared = false;
			NavigationPage?.SendDisappearing();
		}

		public override void ViewDidAppear()
		{
			base.ViewDidAppear();
			Platform.NativeToolbarTracker.Navigation = (NavigationPage)Element;
			if (_appeared)
				return;

			_appeared = true;
			NavigationPage?.SendAppearing();
		}

		void RaiseElementChanged(VisualElementChangedEventArgs e)
		{
			if (e.OldElement != null)
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;

			if (e.NewElement != null)
				e.NewElement.PropertyChanged += OnElementPropertyChanged;

			OnElementChanged(e);
			ElementChanged?.Invoke(this, e);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
		}

		protected virtual void ConfigurePageRenderer()
		{
			View.WantsLayer = true;
		}

		protected virtual Task<bool> OnPopToRoot(Page page, bool animated)
		{
			var renderer = Platform.GetRenderer(page);
			if (renderer == null || renderer.ViewController == null)
				return Task.FromResult(false);

			Platform.NativeToolbarTracker.UpdateToolBar();
			return Task.FromResult(true);
		}

		protected virtual async Task<bool> OnPop(Page page, bool animated)
		{
			var removed = await PopPageAsync(page, animated);
			Platform.NativeToolbarTracker.UpdateToolBar();
			return removed;
		}

		protected virtual async Task<bool> OnPush(Page page, bool animated)
		{
			var shown = await AddPage(page, animated);
			Platform.NativeToolbarTracker.UpdateToolBar();
			return shown;
		}

		void Init()
		{
			ConfigurePageRenderer();

			if (NavigationPage.CurrentPage == null)
				throw new InvalidOperationException(
					"NavigationPage must have a root Page before being used. Either call PushAsync with a valid Page, or pass a Page to the constructor before usage.");

			Platform.NativeToolbarTracker.Navigation = NavigationPage;

			NavigationPage.PushRequested += OnPushRequested;
			NavigationPage.PopRequested += OnPopRequested;
			NavigationPage.PopToRootRequested += OnPopToRootRequested;
			NavigationPage.RemovePageRequested += OnRemovedPageRequested;
			NavigationPage.InsertPageBeforeRequested += OnInsertPageBeforeRequested;
			NavigationPage.Popped += OnPopped;
			NavigationPage.PoppedToRoot += OnPoppedToRoot;

			UpdateBarBackgroundColor();
			UpdateBarTextColor();

			_events = new EventTracker(this);
			_events.LoadEvents(NativeView);
			_tracker = new VisualElementTracker(this);

			NavigationPage.Pages.ForEach(async p => await PushPageAsync(p, false));

			UpdateBackgroundColor();
		}

		IVisualElementRenderer CreateViewControllerForPage(Page page)
		{
			if (Platform.GetRenderer(page) == null)
				Platform.SetRenderer(page, Platform.CreateRenderer(page));

			var pageRenderer = Platform.GetRenderer(page);
			return pageRenderer;
		}

		void OnPopped(object sender, NavigationEventArgs e)
		{
			Platform.NativeToolbarTracker.UpdateToolBar();
		}

		void OnPoppedToRoot(object sender, NavigationEventArgs e)
		{
			Platform.NativeToolbarTracker.UpdateToolBar();
		}

		void InsertPageBefore(Page page, Page before)
		{
			if (before == null)
				throw new ArgumentNullException(nameof(before));
			if (page == null)
				throw new ArgumentNullException(nameof(page));

			var currentList = _currentStack.Reverse().ToList();
			var beforePageIndex = currentList.IndexOf(p => p.Page == before);
			var pageWrapper = new NavigationChildPageWrapper(page);
			currentList.Insert(beforePageIndex, pageWrapper);
			_currentStack = new Stack<NavigationChildPageWrapper>(currentList);

			var vc = CreateViewControllerForPage(page);
			vc.SetElementSize(new Size(View.Bounds.Width, View.Bounds.Height));
			page.Layout(new Rectangle(0, 0, View.Bounds.Width, View.Frame.Height));
			vc.NativeView.WantsLayer = true;

			var beforeRenderer = Platform.GetRenderer(before);
			var beforeControllerIndex = ChildViewControllers.IndexOf(beforeRenderer.ViewController);

			InsertChildViewController(vc.ViewController, beforeControllerIndex);
			View.AddSubview(vc.NativeView, NSWindowOrderingMode.Below, beforeRenderer.NativeView);
		}

		void OnInsertPageBeforeRequested(object sender, NavigationRequestedEventArgs e)
		{
			InsertPageBefore(e.Page, e.BeforePage);
			Platform.NativeToolbarTracker.UpdateNavigationItems(true);
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
			e.Task = PushPageAsync(e.Page, e.Animated);
		}

		void OnRemovedPageRequested(object sender, NavigationRequestedEventArgs e)
		{
			RemovePage(e.Page, true);
			Platform.NativeToolbarTracker.UpdateToolBar();
		}

		void RemovePage(Page page, bool removeFromStack)
		{
			page?.SendDisappearing();
			var target = Platform.GetRenderer(page);
			target?.NativeView?.RemoveFromSuperview();
			target?.ViewController?.RemoveFromParentViewController();
			target?.Dispose();
			if (removeFromStack)
			{
				var newSource = _currentStack.Reverse().Where(w => w.Page != page);
				_currentStack = new Stack<NavigationChildPageWrapper>(newSource);
			}
		}

		NSViewControllerTransitionOptions ToViewControllerTransitionOptions(NavigationTransitionStyle transitionStyle)
		{
			switch (transitionStyle)
			{
				case NavigationTransitionStyle.Crossfade:
					return NSViewControllerTransitionOptions.Crossfade;
				case NavigationTransitionStyle.SlideBackward:
					return NSViewControllerTransitionOptions.SlideBackward;
				case NavigationTransitionStyle.SlideDown:
					return NSViewControllerTransitionOptions.SlideDown;
				case NavigationTransitionStyle.SlideForward:
					return NSViewControllerTransitionOptions.SlideForward;
				case NavigationTransitionStyle.SlideLeft:
					return NSViewControllerTransitionOptions.SlideLeft;
				case NavigationTransitionStyle.SlideRight:
					return NSViewControllerTransitionOptions.SlideRight;
				case NavigationTransitionStyle.SlideUp:
					return NSViewControllerTransitionOptions.SlideUp;

				default:
					return NSViewControllerTransitionOptions.None;
			}
		}

		async Task<bool> PopPageAsync(Page page, bool animated)
		{
			if (page == null)
				throw new ArgumentNullException(nameof(page));

			var wrapper = _currentStack.Peek();
			if (page != wrapper.Page)
				throw new NotSupportedException("Popped page does not appear on top of current navigation stack, please file a bug.");

			_currentStack.Pop();
			page.SendDisappearing();

			var target = Platform.GetRenderer(page);
			var previousPage = _currentStack.Peek().Page;

			if (animated)
			{
				var previousPageRenderer = CreateViewControllerForPage(previousPage);
				var transitionStyle = NavigationPage.OnThisPlatform().GetNavigationTransitionPopStyle();

				return await this.HandleAsyncAnimation(target.ViewController, previousPageRenderer.ViewController,
					ToViewControllerTransitionOptions(transitionStyle), () => target.DisposeRendererAndChildren(), true);
			}

			RemovePage(page, false);
			return true;
		}

		async Task<bool> AddPage(Page page, bool animated)
		{
			if (page == null)
				throw new ArgumentNullException(nameof(page));

			Page oldPage = null;
			if (_currentStack.Count >= 1)
				oldPage = _currentStack.Peek().Page;

			_currentStack.Push(new NavigationChildPageWrapper(page));

			var vc = CreateViewControllerForPage(page);
			vc.SetElementSize(new Size(View.Bounds.Width, View.Bounds.Height));
			page.Layout(new Rectangle(0, 0, View.Bounds.Width, View.Frame.Height));

			if (_currentStack.Count == 1 || !animated)
			{
				vc.NativeView.WantsLayer = true;
				AddChildViewController(vc.ViewController);
				View.AddSubview(vc.NativeView);
				return true;
			}
			var vco = Platform.GetRenderer(oldPage);
			AddChildViewController(vc.ViewController);

            var transitionStyle = NavigationPage.OnThisPlatform().GetNavigationTransitionPushStyle();
			return await this.HandleAsyncAnimation(vco.ViewController, vc.ViewController,
				ToViewControllerTransitionOptions(transitionStyle), () => page?.SendAppearing(), true);
		}

		void UpdateBackgroundColor()
		{
			if (View.Layer == null)
				return;
			var color = Element.BackgroundColor == Color.Default ? Color.White : Element.BackgroundColor;
			View.Layer.BackgroundColor = color.ToCGColor();
		}

		void UpdateBarBackgroundColor()
		{
			Platform.NativeToolbarTracker.UpdateToolBar();
		}

		void UpdateBarTextColor()
		{
			Platform.NativeToolbarTracker.UpdateToolBar();
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_tracker == null)
				return;

			if (e.PropertyName == Xamarin.Forms.NavigationPage.BarBackgroundColorProperty.PropertyName)
				UpdateBarBackgroundColor();
			else if (e.PropertyName == Xamarin.Forms.NavigationPage.BarTextColorProperty.PropertyName)
				UpdateBarTextColor();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
		}
	}
}
