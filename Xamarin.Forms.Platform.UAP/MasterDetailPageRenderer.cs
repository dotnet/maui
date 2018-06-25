using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Specifics = Xamarin.Forms.PlatformConfiguration.WindowsSpecific.MasterDetailPage;
using WImageSource = Windows.UI.Xaml.Media.ImageSource;

namespace Xamarin.Forms.Platform.UWP
{
	public class MasterDetailPageRenderer : IVisualElementRenderer, IToolbarProvider, ITitleProvider, ITitleIconProvider, ITitleViewProvider, IToolBarForegroundBinder
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

		WImageSource ITitleIconProvider.TitleIcon
		{
			get { return Control?.DetailTitleIcon; }

			set
			{
				if (Control != null)
					Control.DetailTitleIcon = value;
			}
		}

		View ITitleViewProvider.TitleView
		{
			get => Control?.DetailTitleView;
			set
			{
				if (Control != null)
					Control.DetailTitleView = value;
			}
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
				UpdateMode();
				UpdateDetail();
				UpdateMaster();
				UpdateIsPresented();

				if (!string.IsNullOrEmpty(e.NewElement.AutomationId))
					Control.SetValue(Windows.UI.Xaml.Automation.AutomationProperties.AutomationIdProperty, e.NewElement.AutomationId);

				((ITitleProvider)this).BarBackgroundBrush = (Brush)Windows.UI.Xaml.Application.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"];
				UpdateToolbarPlacement();
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == MasterDetailPage.IsPresentedProperty.PropertyName || e.PropertyName == MasterDetailPage.MasterBehaviorProperty.PropertyName)
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
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		void ClearDetail()
		{
			((ITitleProvider)this).ShowTitle = false;

			var titleView = ((ITitleViewProvider)this).TitleView;
			titleView?.ClearValue(Platform.RendererProperty);
			titleView = null;

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

			Element.SendAppearing();
			UpdateBounds();
			UpdateFlowDirection();
		}

		void OnControlUnloaded(object sender, RoutedEventArgs routedEventArgs)
		{
			Element?.SendDisappearing();
		}

		void OnDetailPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateDetailTitle();
			else if (e.PropertyName == NavigationPage.TitleIconProperty.PropertyName)
				UpdateDetailTitleIcon();
			else if (e.PropertyName == NavigationPage.TitleViewProperty.PropertyName)
				UpdateDetailTitleView();
			else if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
			{
				UpdateDetailTitle();
				UpdateDetailTitleIcon();
				UpdateDetailTitleView();
			}
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

			Element.MasterBounds = new Rectangle(0, 0, masterSize.Width, masterSize.Height);
			Element.DetailBounds = new Rectangle(0, 0, detailSize.Width, detailSize.Height);
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

				UpdateToolbarVisibilty();
			}

			Control.Detail = element;
			UpdateDetailTitle();
			UpdateDetailTitleIcon();
			UpdateDetailTitleView();
		}

		void UpdateDetailTitle()
		{
			if (_detail == null)
				return;

			Control.DetailTitle = (_detail as NavigationPage)?.CurrentPage?.Title ?? _detail.Title ?? Element?.Title;
			(this as ITitleProvider).ShowTitle = !string.IsNullOrEmpty(Control.DetailTitle);
		}

		async void UpdateDetailTitleIcon()
		{
			if (_detail == null)
				return;

			Control.DetailTitleIcon = await NavigationPage.GetTitleIcon(_detail).ToWindowsImageSource();
			Control.InvalidateMeasure();
		}

		void UpdateDetailTitleView()
		{
			if (_detail == null)
				return;

			Control.DetailTitleView = NavigationPage.GetTitleView(_detail) as View;
			Control.InvalidateMeasure();
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}

		void UpdateIsPresented()
		{
			// Ignore the IsPresented value being set to false for Split mode on desktop and allow the master
			// view to be made initially visible
			if (Device.Idiom == TargetIdiom.Desktop && Control.IsPaneOpen && Element.MasterBehavior != MasterBehavior.Popover)
				return;

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

			UpdateToolbarVisibilty();
		}

		void UpdateMode()
		{
			UpdateDetailTitle();
			UpdateDetailTitleIcon();
			UpdateDetailTitleView();
			Control.CollapseStyle = Element.OnThisPlatform().GetCollapseStyle();
			Control.CollapsedPaneWidth = Element.OnThisPlatform().CollapsedPaneWidth();
			Control.ShouldShowSplitMode = Element.ShouldShowSplitMode;
		}

		void UpdateToolbarPlacement()
		{
			Control.ToolbarPlacement = Element.OnThisPlatform().GetToolbarPlacement();
		}

		void UpdateToolbarVisibilty()
		{
			// Enforce consistency rules on toolbar
			Control.ShouldShowToolbar = _detail is NavigationPage || _master is NavigationPage;
			if(_detail is NavigationPage _detailNav)
				Control.ShouldShowNavigationBar = NavigationPage.GetHasNavigationBar(_detailNav.CurrentPage);
			
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
	}
}
