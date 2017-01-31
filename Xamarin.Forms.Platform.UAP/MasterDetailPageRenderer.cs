using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Specifics = Xamarin.Forms.PlatformConfiguration.WindowsSpecific.MasterDetailPage;

namespace Xamarin.Forms.Platform.UWP
{
	public class MasterDetailPageRenderer : IVisualElementRenderer, IToolbarProvider, ITitleProvider, IToolBarForegroundBinder
	{
		Page _master;
		Page _detail;
		bool _showTitle;

		VisualElementTracker<Page, FrameworkElement> _tracker;

		public MasterDetailControl Control { get; private set; }

		public MasterDetailPage Element { get; private set; }

		protected VisualElementTracker<Page, FrameworkElement> Tracker
		{
			get { return _tracker; }
			set
			{
				if (_tracker == value)
					return;

				if (_tracker != null)
					_tracker.Dispose();

				_tracker = value;
			}
		}

		public void Dispose()
		{
			ClearMaster();
			ClearDetail();

			Tracker = null;
			if (Element != null)
				SetElement(null);
		}

		Brush ITitleProvider.BarBackgroundBrush
		{
			set { Control.ToolbarBackground = value; }
		}

		Brush ITitleProvider.BarForegroundBrush
		{
			set { Control.ToolbarForeground = value; }
		}
		
		IPageController PageController => Element as IPageController;

		IMasterDetailPageController MasterDetailPageController => Element as IMasterDetailPageController;

		bool ITitleProvider.ShowTitle
		{
			get { return _showTitle; }

			set
			{
				if (_showTitle == value)
					return;

				_showTitle = value;
				Control.DetailTitleVisibility = _showTitle ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		string ITitleProvider.Title
		{
			get { return Element?.Title; }

			set
			{
				if (Control != null)
					Control.DetailTitle = value;
			}
		}

		Task<CommandBar> IToolbarProvider.GetCommandBarAsync()
		{
			return ((IToolbarProvider)Control)?.GetCommandBarAsync();
		}

		FrameworkElement IVisualElementRenderer.ContainerElement
		{
			get { return Control; }
		}

		VisualElement IVisualElementRenderer.Element
		{
			get { return Element; }
		}

#pragma warning disable 0067 // Revisit: Can't remove; required by interface
		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
#pragma warning restore

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			Size size = Device.Info.ScaledScreenSize;
			return new SizeRequest(new Size(size.Width, size.Height));
		}

		UIElement IVisualElementRenderer.GetNativeElement()
		{
			return Control;
		}

		public void SetElement(VisualElement element)
		{
			MasterDetailPage old = Element;
			Element = (MasterDetailPage)element;

			if (element != old)
				OnElementChanged(new ElementChangedEventArgs<MasterDetailPage>(old, Element));
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<MasterDetailPage> e)
		{
			if (e.OldElement != null)
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					Control = new MasterDetailControl();
					Control.Loaded += OnControlLoaded;
					Control.Unloaded += OnControlUnloaded;
					Control.SizeChanged += OnNativeSizeChanged;

					Control.RegisterPropertyChangedCallback(MasterDetailControl.IsPaneOpenProperty, OnIsPaneOpenChanged);

					Tracker = new VisualElementTracker<Page, FrameworkElement> { Container = Control, Element = Element };
				}

				e.NewElement.PropertyChanged += OnElementPropertyChanged;
				UpdateDetail();
				UpdateMaster();
				UpdateMode();
				UpdateIsPresented();

				if (!string.IsNullOrEmpty(e.NewElement.AutomationId))
					Control.SetValue(AutomationProperties.AutomationIdProperty, e.NewElement.AutomationId);

#if WINDOWS_UWP
                UpdateToolbarPlacement();
#endif

            }
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == MasterDetailPage.IsPresentedProperty.PropertyName)
				UpdateIsPresented();
			else if (e.PropertyName == "Master")
				UpdateMaster();
			else if (e.PropertyName == "Detail")
				UpdateDetail();
			else if (e.PropertyName == nameof(MasterDetailControl.ShouldShowSplitMode)
			         || e.PropertyName == Specifics.CollapseStyleProperty.PropertyName
			         || e.PropertyName == Specifics.CollapsedPaneWidthProperty.PropertyName)
				UpdateMode();
			else if(e.PropertyName ==  PlatformConfiguration.WindowsSpecific.Page.ToolbarPlacementProperty.PropertyName)
				UpdateToolbarPlacement();
		}

		void ClearDetail()
		{
			((ITitleProvider)this).ShowTitle = false;

			if (_detail == null)
				return;

			_detail.PropertyChanged -= OnDetailPropertyChanged;

			IVisualElementRenderer renderer = Platform.GetRenderer(_detail);
			renderer?.Dispose();

			_detail.ClearValue(Platform.RendererProperty);
			_detail = null;
		}

		void ClearMaster()
		{
			if (_master == null)
				return;

			_master.PropertyChanged -= OnMasterPropertyChanged;

			IVisualElementRenderer renderer = Platform.GetRenderer(_master);
			renderer?.Dispose();

			_master.ClearValue(Platform.RendererProperty);
			_master = null;
		}

		void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			if (Element == null)
				return;

			PageController.SendAppearing();
			UpdateBounds();
		}

		void OnControlUnloaded(object sender, RoutedEventArgs routedEventArgs)
		{
			PageController?.SendDisappearing();
		}

		void OnDetailPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName || e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
				UpdateDetailTitle();
		}

		void OnIsPaneOpenChanged(DependencyObject sender, DependencyProperty dp)
		{
			((IElementController)Element).SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, Control.IsPaneOpen);
			UpdateBounds();
		}

		void OnMasterPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
				Control.MasterTitle = _master?.Title;
		}

		void OnNativeSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateBounds();
		}

		void UpdateBounds()
		{
			Windows.Foundation.Size masterSize = Control.MasterSize;
			Windows.Foundation.Size detailSize = Control.DetailSize;

			MasterDetailPageController.MasterBounds = new Rectangle(0, 0, masterSize.Width, masterSize.Height);
			MasterDetailPageController.DetailBounds = new Rectangle(0, 0, detailSize.Width, detailSize.Height);
		}

		void UpdateDetail()
		{
			ClearDetail();

			FrameworkElement element = null;

			_detail = Element.Detail;
			if (_detail != null)
			{
				_detail.PropertyChanged += OnDetailPropertyChanged;

				IVisualElementRenderer renderer = _detail.GetOrCreateRenderer();
				element = renderer.ContainerElement;

				// Enforce consistency rules on toolbar (show toolbar if Detail is Navigation Page)
				Control.ShouldShowToolbar = _detail is NavigationPage; 
			}

			Control.Detail = element;
			UpdateDetailTitle();
		}

		void UpdateDetailTitle()
		{
			if (_detail == null)
				return;

			Control.DetailTitle = (_detail as NavigationPage)?.CurrentPage?.Title ?? _detail.Title ?? Element?.Title;
			(this as ITitleProvider).ShowTitle = !string.IsNullOrEmpty(Control.DetailTitle);
		}

		void UpdateIsPresented()
		{
			Control.IsPaneOpen = Element.IsPresented;
		}

		void UpdateMaster()
		{
			ClearMaster();

			FrameworkElement element = null;
			_master = Element.Master;
			if (_master != null)
			{
				_master.PropertyChanged += OnMasterPropertyChanged;

				IVisualElementRenderer renderer = _master.GetOrCreateRenderer();
				element = renderer.ContainerElement;
			}

			Control.Master = element;
			Control.MasterTitle = _master?.Title;

			// Enforce consistency rules on toolbar (show toolbar if Master is Navigation Page)
			Control.ShouldShowToolbar = _master is NavigationPage;
		}

		void UpdateMode()
		{
			Control.CollapseStyle = Element.OnThisPlatform().GetCollapseStyle();
			Control.CollapsedPaneWidth = Element.OnThisPlatform().CollapsedPaneWidth();
			Control.ShouldShowSplitMode = MasterDetailPageController.ShouldShowSplitMode;
		}

#if WINDOWS_UWP

        void UpdateToolbarPlacement()
		{
			Control.ToolbarPlacement = Element.OnThisPlatform().GetToolbarPlacement();
		}

		public void BindForegroundColor(AppBar appBar)
		{
			SetAppBarForegroundBinding(appBar);
		}

		public void BindForegroundColor(AppBarButton button)
		{
			SetAppBarForegroundBinding(button);
		}

		void SetAppBarForegroundBinding(FrameworkElement element)
		{
			element.SetBinding(Windows.UI.Xaml.Controls.Control.ForegroundProperty,
				new Windows.UI.Xaml.Data.Binding { Path = new PropertyPath("Control.ToolbarForeground"), Source = this, RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent } });
		}
#endif
	}
}