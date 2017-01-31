using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Xamarin.Forms.Platform.WinRT
{
	public class MasterDetailPageRenderer : IVisualElementRenderer, ITitleProvider
	{
		MasterDetailControl _container;

		bool _disposed;
		IVisualElementRenderer _masterRenderer;
		IVisualElementRenderer _detailRenderer;
		bool _showTitle;
		VisualElementTracker<Page, PageControl> _tracker;

		public MasterDetailPage Element { get; private set; }

		protected VisualElementTracker<Page, PageControl> Tracker
		{
			get { return _tracker; }
			set
			{
				if (_tracker == value)
					return;

				if (_tracker != null)
				{
					_tracker.Dispose();
				}

				_tracker = value;
			}
		}

		bool IsPopoverFullScreen
		{
			get { return Device.Idiom == TargetIdiom.Phone; }
		}

		IPageController PageController => Element as IPageController;

		public void Dispose()
		{
			Dispose(true);
		}

		Brush ITitleProvider.BarBackgroundBrush
		{
			set { _container.ToolbarBackground = value; }
		}

		Brush ITitleProvider.BarForegroundBrush
		{
			set { _container.ToolbarForeground = value; }
		}

		IMasterDetailPageController MasterDetailPageController => Element as IMasterDetailPageController;

		bool ITitleProvider.ShowTitle
		{
			get { return _showTitle; }

			set
			{
				if (_showTitle == value)
					return;
				_showTitle = value;
				if (_showTitle)
					_container.DetailTitleVisibility = Visibility.Visible;
				else
					_container.DetailTitleVisibility = Visibility.Collapsed;
			}
		}

		string ITitleProvider.Title
		{
			get { return _container.Title; }

			set { _container.Title = value; }
		}

		public FrameworkElement ContainerElement
		{
			get { return _container; }
		}

		VisualElement IVisualElementRenderer.Element
		{
			get { return Element; }
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		UIElement IVisualElementRenderer.GetNativeElement()
		{
			return null;
		}

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(Device.Info.ScaledScreenSize.Width, Device.Info.ScaledScreenSize.Height));
		}

		public void SetElement(VisualElement element)
		{
			if (element != null && !(element is MasterDetailPage))
				throw new ArgumentException("Element must be a Page", "element");

			MasterDetailPage oldElement = Element;
			Element = (MasterDetailPage)element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (element != null)
			{
				if (_container == null)
				{
					_container = new MasterDetailControl();
					_container.UserClosedPopover += OnUserClosedPopover;
					_container.SizeChanged += OnNativeSizeChanged;

					Tracker = new BackgroundTracker<PageControl>(Control.BackgroundProperty) { Element = (Page)element, Container = _container };

					_container.Loaded += OnLoaded;
					_container.Unloaded += OnUnloaded;
				}

				element.PropertyChanged += OnElementPropertyChanged;
				UpdateBehavior();
				SetMaster(Element.Master);
				SetDetail(Element.Detail);
				UpdateIsPresented();
			}

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || _disposed)
				return;

			_disposed = true;

			PageController?.SendDisappearing();
			SetElement(null);
		}

		protected void OnElementChanged(VisualElementChangedEventArgs e)
		{
			EventHandler<VisualElementChangedEventArgs> changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Master")
				SetMaster(Element.Master);
			else if (e.PropertyName == "Detail")
				SetDetail(Element.Detail);
			else if (e.PropertyName == MasterDetailPage.IsPresentedProperty.PropertyName)
				UpdateIsPresented();
			else if (e.PropertyName == MasterDetailPage.MasterBehaviorProperty.PropertyName)
				UpdateBehavior();
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateTitle();
		}

		bool GetIsMasterAPopover()
		{
			// TODO: Support tablet being shrunk to a very small size
			return !MasterDetailPageController.ShouldShowSplitMode;
		}

		void OnLoaded(object sender, RoutedEventArgs args)
		{
			PageController?.SendAppearing();
		}

		void OnNativeSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateBounds(e.NewSize);
		}

		void OnUnloaded(object sender, RoutedEventArgs args)
		{
			PageController?.SendDisappearing();
		}

		void OnUserClosedPopover(object sender, EventArgs e)
		{
			if (Element != null)
				((IElementController)Element).SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, false);
		}

		void SetDetail(Page detailPage)
		{
			((ITitleProvider)this).ShowTitle = detailPage is NavigationPage;

			if (_detailRenderer != null)
			{
				FrameworkElement native = _detailRenderer.ContainerElement;
				_container.DetailContent = null;
				_detailRenderer = null;
			}

			if (detailPage == null)
				return;

			_detailRenderer = detailPage.GetOrCreateRenderer();
			_container.DetailContent = _detailRenderer.ContainerElement;
			UpdateTitle();
		}

		void SetMaster(Page masterPage)
		{
			if (_masterRenderer != null)
			{
				FrameworkElement native = _masterRenderer.ContainerElement;
				_container.MasterContent = null;
				_masterRenderer = null;
			}

			if (masterPage == null)
				return;

			_masterRenderer = masterPage.GetOrCreateRenderer();
			_container.MasterContent = _masterRenderer.ContainerElement;
		}

		void UpdateBehavior()
		{
			string key = GetIsMasterAPopover() ? "MasterDetailPopup" : "MasterDetailSplit";
			_container.Template = (Windows.UI.Xaml.Controls.ControlTemplate)Windows.UI.Xaml.Application.Current.Resources[key];
		}

		void UpdateBounds(bool isPresented)
		{
			UpdateBounds(new Windows.Foundation.Size(_container.ActualWidth, _container.ActualHeight), isPresented);
		}

		void UpdateBounds(Windows.Foundation.Size constraint)
		{
			UpdateBounds(constraint, Element.IsPresented);
		}

		void UpdateBounds(Windows.Foundation.Size constraint, bool isPresented)
		{
			if (constraint.Width <= 0 || constraint.Height <= 0)
				return;

			bool isPopover = GetIsMasterAPopover();
			double masterWidth = 0;
			if (isPresented || isPopover)
			{
				if (isPopover && IsPopoverFullScreen)
					masterWidth = constraint.Width;
				else
					masterWidth = constraint.Width * .3;
			}

			double detailWidth = constraint.Width;
			if (!isPopover)
				detailWidth -= masterWidth;

			MasterDetailPageController.MasterBounds = new Rectangle(0, 0, masterWidth, constraint.Height);
			MasterDetailPageController.DetailBounds = new Rectangle(0, 0, detailWidth, constraint.Height);
		}

		void UpdateIsPresented()
		{
			UpdateBehavior();

			bool isPresented = !GetIsMasterAPopover() || Element.IsPresented;
			_container.IsMasterVisible = isPresented;

			UpdateBounds(isPresented);
		}

		void UpdateTitle()
		{
			if (Element?.Detail == null)
				return;

			((ITitleProvider)this).Title = (Element.Detail as NavigationPage)?.CurrentPage?.Title ?? Element.Title ?? Element?.Title;
		}
	}
}