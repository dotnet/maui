using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class FlyoutPageRenderer : IVisualElementRenderer, IToolbarProvider, ITitleProvider, ITitleIconProvider, ITitleViewProvider, IToolBarForegroundBinder
	{
		Page _master;
		Page _detail;
		bool _showTitle;

		string _defaultAutomationPropertiesName;
		AccessibilityView? _defaultAutomationPropertiesAccessibilityView;
		string _defaultAutomationPropertiesHelpText;
		UIElement _defaultAutomationPropertiesLabeledBy;

		VisualElementTracker<Page, FrameworkElement> _tracker;
		IFlyoutPageController FlyoutPageController => Element;

		public FlyoutPageControl Control { get; private set; }

		public FlyoutPage Element { get; private set; }

		protected VisualElementTracker<Page, FrameworkElement> Tracker
		{
			get { return _tracker; }
			set
			{
				if (_tracker == value)
					return;

				_tracker?.Dispose();

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

		WBrush ITitleProvider.BarBackgroundBrush
		{
			set { Control.ToolbarBackground = value; }
		}

		WBrush ITitleProvider.BarForegroundBrush
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
				Control.DetailTitleVisibility = _showTitle ? WVisibility.Visible : WVisibility.Collapsed;
			}
		}

		string ITitleProvider.Title
		{
			get { return Element?.Title; }

			set
			{
				Control?.DetailTitle = value;
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
				Control?.DetailTitleIcon = value;
			}
		}

		View ITitleViewProvider.TitleView
		{
			get => Control?.DetailTitleView;
			set
			{
				Control?.DetailTitleView = value;
			}
		}

#pragma warning disable 0067 // Revisit: Can't remove; required by interface
		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
#pragma warning restore

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var size = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
			return new SizeRequest(size);
		}

		UIElement IVisualElementRenderer.GetNativeElement()
		{
			return Control;
		}

		public void SetElement(VisualElement element)
		{
			FlyoutPage old = Element;
			Element = (FlyoutPage)element;

			if (element != old)
				OnElementChanged(new ElementChangedEventArgs<FlyoutPage>(old, Element));
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<FlyoutPage> e)
		{
			if (e.OldElement != null)
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					Control = new FlyoutPageControl();
					Control.Loaded += OnControlLoaded;
					Control.Unloaded += OnControlUnloaded;
					Control.SizeChanged += OnNativeSizeChanged;

					Control.RegisterPropertyChangedCallback(FlyoutPageControl.IsPaneOpenProperty, OnIsPaneOpenChanged);

					Tracker = new VisualElementTracker<Page, FrameworkElement> { Container = Control, Element = Element };
				}

				e.NewElement.PropertyChanged += OnElementPropertyChanged;
				UpdateMode();
				UpdateDetail();
				UpdateMaster();
				UpdateIsPresented();

				if (!string.IsNullOrEmpty(e.NewElement.AutomationId))
					Control.SetValue(Microsoft.UI.Xaml.Automation.AutomationProperties.AutomationIdProperty, e.NewElement.AutomationId);

				((ITitleProvider)this).BarBackgroundBrush = (WBrush)Microsoft.UI.Xaml.Application.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"];
				UpdateToolbarPlacement();
				UpdateToolbarDynamicOverflowEnabled();

				_defaultAutomationPropertiesName = Control.SetAutomationPropertiesName(Element, _defaultAutomationPropertiesName);
				_defaultAutomationPropertiesHelpText = Control.SetAutomationPropertiesHelpText(Element, _defaultAutomationPropertiesHelpText);
				_defaultAutomationPropertiesLabeledBy = Control.SetAutomationPropertiesLabeledBy(Element, Element.Handler?.MauiContext ?? Forms.MauiContext, _defaultAutomationPropertiesLabeledBy);
				_defaultAutomationPropertiesAccessibilityView = Control.SetAutomationPropertiesAccessibilityView(Element, _defaultAutomationPropertiesAccessibilityView);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == FlyoutPage.IsPresentedProperty.PropertyName || e.PropertyName == FlyoutPage.FlyoutLayoutBehaviorProperty.PropertyName)
				UpdateIsPresented();
			else if (e.PropertyName == "Master")
				UpdateMaster();
			else if (e.PropertyName == "Detail")
				UpdateDetail();
			else if (e.PropertyName == nameof(FlyoutPageControl.ShouldShowSplitMode))
				UpdateMode();
			else if (e.PropertyName == PlatformConfiguration.WindowsSpecific.Page.ToolbarPlacementProperty.PropertyName)
				UpdateToolbarPlacement();
			else if (e.PropertyName == PlatformConfiguration.WindowsSpecific.Page.ToolbarDynamicOverflowEnabledProperty.PropertyName)
				UpdateToolbarDynamicOverflowEnabled();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
			else if (e.PropertyName == AutomationProperties.NameProperty.PropertyName)
				_defaultAutomationPropertiesName = Control.SetAutomationPropertiesName(Element, _defaultAutomationPropertiesName);
			else if (e.PropertyName == AutomationProperties.HelpTextProperty.PropertyName)
				_defaultAutomationPropertiesHelpText = Control.SetAutomationPropertiesHelpText(Element, _defaultAutomationPropertiesHelpText);
			else if (e.PropertyName == AutomationProperties.LabeledByProperty.PropertyName)
				_defaultAutomationPropertiesLabeledBy = Control.SetAutomationPropertiesLabeledBy(Element, Element.Handler?.MauiContext ?? Forms.MauiContext, _defaultAutomationPropertiesLabeledBy);
			else if (e.PropertyName == AutomationProperties.IsInAccessibleTreeProperty.PropertyName)
				_defaultAutomationPropertiesAccessibilityView = Control.SetAutomationPropertiesAccessibilityView(Element, _defaultAutomationPropertiesAccessibilityView);
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
			else if (e.PropertyName == NavigationPage.TitleIconImageSourceProperty.PropertyName)
				UpdateDetailTitleIcon();
			else if (e.PropertyName == NavigationPage.TitleViewProperty.PropertyName)
				UpdateDetailTitleView();
			else if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
			{
				UpdateDetailTitle();
				UpdateDetailTitleIcon();
			}
		}

		void OnIsPaneOpenChanged(DependencyObject sender, DependencyProperty dp)
		{
			((IElementController)Element).SetValueFromRenderer(FlyoutPage.IsPresentedProperty, Control.IsPaneOpen);
			UpdateBounds();
		}

		void OnMasterPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
				Control.FlyoutTitle = _master?.Title;
		}

		void OnNativeSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateBounds();
		}

		void UpdateBounds()
		{
			global::Windows.Foundation.Size masterSize = Control.FlyoutSize;
			global::Windows.Foundation.Size detailSize = Control.DetailSize;

			FlyoutPageController.FlyoutBounds = new Rect(0, 0, masterSize.Width, masterSize.Height);
			FlyoutPageController.DetailBounds = new Rect(0, 0, detailSize.Width, detailSize.Height);
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

				UpdateToolbarVisibility();
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

			Control.DetailTitle = GetCurrentPage().Title ?? Element?.Title;
			(this as ITitleProvider).ShowTitle = !string.IsNullOrEmpty(Control.DetailTitle) || Element.FlyoutLayoutBehavior == FlyoutLayoutBehavior.Popover;
		}

		async void UpdateDetailTitleIcon()
		{
			if (_detail == null)
				return;

			Control.DetailTitleIcon = await NavigationPage.GetTitleIconImageSource(GetCurrentPage()).ToWindowsImageSourceAsync();
			Control.InvalidateMeasure();
		}

		void UpdateDetailTitleView()
		{
			if (_detail == null)
				return;

			Control.DetailTitleView = NavigationPage.GetTitleView(GetCurrentPage()) as View;
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
			if (DeviceInfo.Idiom == DeviceIdiom.Desktop && Control.IsPaneOpen && Element.FlyoutLayoutBehavior != FlyoutLayoutBehavior.Popover)
				return;

			Control.IsPaneOpen = Element.IsPresented;
		}

		void UpdateMaster()
		{
			ClearMaster();

			FrameworkElement element = null;
			_master = Element.Flyout;
			if (_master != null)
			{
				_master.PropertyChanged += OnMasterPropertyChanged;

				IVisualElementRenderer renderer = _master.GetOrCreateRenderer();
				element = renderer.ContainerElement;
			}

			Control.Flyout = element;
			Control.FlyoutTitle = _master?.Title;

			UpdateToolbarVisibility();
		}

		void UpdateMode()
		{
			UpdateDetailTitle();
			UpdateDetailTitleIcon();
			UpdateDetailTitleView();
			Control.CollapseStyle = Element.OnThisPlatform().GetCollapseStyle();
			Control.CollapsedPaneWidth = Element.OnThisPlatform().CollapsedPaneWidth();
			Control.ShouldShowSplitMode = FlyoutPageController.ShouldShowSplitMode;
		}

		void UpdateToolbarPlacement()
		{
			Control.ToolbarPlacement = Element.OnThisPlatform().GetToolbarPlacement();
		}

		void UpdateToolbarDynamicOverflowEnabled()
		{
			Control.ToolbarDynamicOverflowEnabled = Element.OnThisPlatform().GetToolbarDynamicOverflowEnabled();
		}

		void UpdateToolbarVisibility()
		{
			// Enforce consistency rules on toolbar
			Control.ShouldShowToolbar = _detail is NavigationPage || _master is NavigationPage;
			if (_detail is NavigationPage _detailNav)
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
			element.SetBinding(Microsoft.UI.Xaml.Controls.Control.ForegroundProperty,
				new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("Control.ToolbarForeground"), Source = this, RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent } });
		}

		Page GetCurrentPage()
		{
			if (_detail is NavigationPage page)
				return page.CurrentPage;

			return _detail;
		}
	}
}
