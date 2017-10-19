using System;
using System.Collections.Specialized;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.macOSSpecific;

namespace Xamarin.Forms.Platform.MacOS
{
	public class TabbedPageRenderer : NSTabViewController, IVisualElementRenderer, IEffectControlProvider
	{
		const float DefaultImageSizeSegmentedButton = 19;
		const int TabHolderHeight = 30;

		bool _disposed;
		bool _updatingControllers;
		bool _barBackgroundColorWasSet;
		bool _barTextColorWasSet;
		bool _defaultBarTextColorSet;
		bool _defaultBarColorSet;
		VisualElementTracker _tracker;
		bool _loaded;
		Size _queuedSize;


		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public NSView NativeView => View;

		public void SetElement(VisualElement element)
		{
			var oldElement = Element;
			Element = element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
				var tabbedPage = oldElement as TabbedPage;
				if (tabbedPage != null) tabbedPage.PagesChanged -= OnPagesChanged;
			}

			if (element != null)
			{
				if (_tracker == null)
				{
					_tracker = new VisualElementTracker(this);
					_tracker.NativeControlUpdated += (sender, e) => UpdateNativeWidget();
				}
			}

			RaiseElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			ConfigureTabView();

			OnPagesChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

			Tabbed.PropertyChanged += OnElementPropertyChanged;
			Tabbed.PagesChanged += OnPagesChanged;

			UpdateBarBackgroundColor();

			UpdateBarTextColor();

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);
		}

		Page Page => Element as Page;

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				platformEffect.SetContainer(View);
		}

		public void SetElementSize(Size size)
		{
			if (_loaded)
				Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
			else
				_queuedSize = size;
		}

		public NSViewController ViewController => this;

		public override void ViewWillLayout()
		{
			base.ViewWillLayout();

			if (Element == null)
				return;

			if (!Element.Bounds.IsEmpty)
				View.Frame = new System.Drawing.RectangleF((float)Element.X, (float)Element.Y, (float)Element.Width, (float)Element.Height);

			var topOffset = TabHolderHeight;
			var tabStyle = Tabbed.OnThisPlatform().GetTabsStyle();
			if (tabStyle == TabsStyle.Hidden || tabStyle == TabsStyle.OnNavigation)
				topOffset = 0;

			var frame = View.Frame;
			Page.ContainerArea = new Rectangle(0, 0, frame.Width, frame.Height - topOffset);

			if (!_queuedSize.IsZero)
			{
				Element.Layout(new Rectangle(Element.X, Element.Y, _queuedSize.Width, _queuedSize.Height));
				_queuedSize = Size.Zero;
			}

			_loaded = true;
		}


		public override nint SelectedTabViewItemIndex
		{
			get { return base.SelectedTabViewItemIndex; }
			set
			{
				base.SelectedTabViewItemIndex = value;
				if (!_updatingControllers)
					UpdateCurrentPage();
			}
		}

		public override void ViewDidAppear()
		{
			Page.SendAppearing();
			base.ViewDidAppear();
		}

		public override void ViewDidDisappear()
		{
			base.ViewDidDisappear();
			Page.SendDisappearing();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				Page.SendDisappearing();
				Tabbed.PropertyChanged -= OnElementPropertyChanged;
				Tabbed.PagesChanged -= OnPagesChanged;

				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker = null;
				}
			}

			base.Dispose(disposing);
		}

		protected virtual void ConfigureTabView()
		{
			View.WantsLayer = true;
			TabView.WantsLayer = true;
			TabView.DrawsBackground = false;
			var tabStyle = Tabbed.OnThisPlatform().GetTabsStyle();
			switch (tabStyle)
			{
				case TabsStyle.OnNavigation:
				case TabsStyle.Hidden:
					TabStyle = NSTabViewControllerTabStyle.Unspecified;
					break;
				case TabsStyle.Icons:
					TabStyle = NSTabViewControllerTabStyle.Toolbar;
					break;
				case TabsStyle.OnBottom:
					TabStyle = NSTabViewControllerTabStyle.SegmentedControlOnBottom;
					break;
				default:
					TabStyle = NSTabViewControllerTabStyle.SegmentedControlOnTop;
					break;
			}

			TabView.TabViewType = NSTabViewType.NSNoTabsNoBorder;
		}

		void RaiseElementChanged(VisualElementChangedEventArgs e)
		{
			OnElementChanged(e);
			ElementChanged?.Invoke(this, e);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
		}

		protected virtual NSTabViewItem GetTabViewItem(Page page, IVisualElementRenderer pageRenderer)
		{
			var tvi = new NSTabViewItem { ViewController = pageRenderer.ViewController, Label = page.Title ?? "" };
			if (!string.IsNullOrEmpty (page.Icon)) {
				var image = GetTabViewItemIcon (page.Icon);
				if (image != null)
					tvi.Image = image;
			}
			return tvi;
		}

		protected virtual NSImage GetTabViewItemIcon(string imageName)
		{
			var image = NSImage.ImageNamed (imageName);
			if(image == null)
				image = new NSImage (imageName);

			if (image == null)
				return null;

			bool shouldResize = TabStyle == NSTabViewControllerTabStyle.SegmentedControlOnTop ||
								TabStyle == NSTabViewControllerTabStyle.SegmentedControlOnBottom;
			if (shouldResize)
				image = image.ResizeTo(new CGSize(DefaultImageSizeSegmentedButton, DefaultImageSizeSegmentedButton));
			return image;
		}

		protected virtual void UpdateNativeWidget()
		{
			TabView.Layout();
		}

		protected TabbedPage Tabbed => (TabbedPage)Element;

		void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
			{
				var page = (Page)sender;
				var index = TabbedPage.GetIndex(page);
				TabViewItems[index].Label = page.Title;
			}
			else if (e.PropertyName == Page.IconProperty.PropertyName)
			{
				var page = (Page)sender;

				var index = TabbedPage.GetIndex(page);
				TabViewItems[index].Label = page.Title;

				if (!string.IsNullOrEmpty(page.Icon))
				{
					TabViewItems[index].Image = new NSImage(page.Icon);
				}
				else if (TabViewItems[index].Image != null)
				{
					TabViewItems[index].Image = new NSImage();
				}
			}
		}

		void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			e.Apply((o, i, c) => SetupPage((Page)o, i), (o, i) => TeardownPage((Page)o), Reset);

			SetControllers();

			UpdateChildrenOrderIndex();

			SetSelectedTabViewItem();
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(TabbedPage.CurrentPage))
			{
				var current = Tabbed.CurrentPage;
				if (current == null)
					return;

				SetSelectedTabViewItem();
			}
			else if (e.PropertyName == TabbedPage.BarBackgroundColorProperty.PropertyName)
				UpdateBarBackgroundColor();
			else if (e.PropertyName == TabbedPage.BarTextColorProperty.PropertyName)
				UpdateBarTextColor();
		}

		void Reset()
		{
			var i = 0;
			foreach (var page in Tabbed.Children)
				SetupPage(page, i++);
		}

		void SetControllers()
		{
			_updatingControllers = true;
			for (var i = 0; i < Element.LogicalChildren.Count; i++)
			{
				var child = Element.LogicalChildren[i];
				var page = child as Page;
				if (page == null)
					continue;

				var pageRenderer = Platform.GetRenderer(page);
				if (pageRenderer != null)
				{
					pageRenderer.ViewController.Identifier = i.ToString();

					NSTabViewItem newTvi = GetTabViewItem(page, pageRenderer);

					AddTabViewItem(newTvi);
				}
			}
			_updatingControllers = false;
		}

		void SetupPage(Page page, int index)
		{
			var renderer = Platform.GetRenderer(page);
			if (renderer == null)
			{
				renderer = Platform.CreateRenderer(page);
				Platform.SetRenderer(page, renderer);
			}

			renderer.ViewController.Identifier = index.ToString();

			page.PropertyChanged += OnPagePropertyChanged;
		}

		void TeardownPage(Page page)
		{
			page.PropertyChanged -= OnPagePropertyChanged;

			Platform.SetRenderer(page, null);
		}

		void SetSelectedTabViewItem()
		{
			if (Tabbed.CurrentPage == null)
				return;
			var selectedIndex = TabbedPage.GetIndex(Tabbed.CurrentPage);
			SelectedTabViewItemIndex = selectedIndex;
		}

		void UpdateChildrenOrderIndex()
		{
			for (var i = 0; i < TabViewItems.Length; i++)
			{
				int originalIndex;
				if (int.TryParse(TabViewItems[i].ViewController.Identifier, out originalIndex))
				{
					var page = Page.InternalChildren[originalIndex];
					TabbedPage.SetIndex(page as Page, i);
				}
			}
		}

		void UpdateCurrentPage()
		{
			var count = Page.InternalChildren.Count;
			Tabbed.CurrentPage = SelectedTabViewItemIndex >= 0 && SelectedTabViewItemIndex < count
				? Tabbed.GetPageByIndex((int)SelectedTabViewItemIndex)
				: null;
		}

		//TODO: Implement UpdateBarBackgroundColor
		void UpdateBarBackgroundColor()
		{
			if (Tabbed == null || TabView == null)
				return;

			var barBackgroundColor = Tabbed.BarBackgroundColor;
			var isDefaultColor = barBackgroundColor.IsDefault;

			if (isDefaultColor && !_barBackgroundColorWasSet)
				return;

			if (!_defaultBarColorSet)
			{
				//_defaultBarColor = TabView.color;
				_defaultBarColorSet = true;
			}

			if (!isDefaultColor)
				_barBackgroundColorWasSet = true;
		}

		//TODO: Implement UpdateBarTextColor
		void UpdateBarTextColor()
		{
			if (Tabbed == null || TabView == null)
				return;

			var barTextColor = Tabbed.BarTextColor;
			var isDefaultColor = barTextColor.IsDefault;

			if (isDefaultColor && !_barTextColorWasSet)
				return;

			if (!_defaultBarTextColorSet)
			{
				//	_defaultBarTextColor = TabBar.TintColor;
				_defaultBarTextColorSet = true;
			}

			if (!isDefaultColor)
				_barTextColorWasSet = true;
		}
	}
}