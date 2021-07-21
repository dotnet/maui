using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class FlyoutPageControl : Control, IToolbarProvider, ITitleViewRendererController
	{
		public static readonly DependencyProperty FlyoutProperty = DependencyProperty.Register(nameof(Flyout), typeof(FrameworkElement), typeof(FlyoutPageControl),
			new PropertyMetadata(default(FrameworkElement)));

		public static readonly DependencyProperty FlyoutTitleProperty = DependencyProperty.Register(nameof(FlyoutTitle), typeof(string), typeof(FlyoutPageControl), new PropertyMetadata(default(string)));

		public static readonly DependencyProperty DetailProperty = DependencyProperty.Register("Detail", typeof(FrameworkElement), typeof(FlyoutPageControl),
			new PropertyMetadata(default(FrameworkElement)));

		public static readonly DependencyProperty IsPaneOpenProperty = DependencyProperty.Register("IsPaneOpen", typeof(bool), typeof(FlyoutPageControl), new PropertyMetadata(default(bool)));

		public static readonly DependencyProperty ShouldShowSplitModeProperty = DependencyProperty.Register(nameof(ShouldShowSplitMode), typeof(bool), typeof(FlyoutPageControl),
			new PropertyMetadata(default(bool), OnShouldShowSplitModeChanged));

		public static readonly DependencyProperty ShouldShowNavigationBarProperty = DependencyProperty.Register(nameof(ShouldShowNavigationBar), typeof(bool), typeof(FlyoutPageControl),
		new PropertyMetadata(true, OnShouldShowSplitModeChanged));

		public static readonly DependencyProperty CollapseStyleProperty = DependencyProperty.Register(nameof(CollapseStyle), typeof(CollapseStyle),
			typeof(FlyoutPageControl), new PropertyMetadata(CollapseStyle.Full, CollapseStyleChanged));

		public static readonly DependencyProperty CollapsedPaneWidthProperty = DependencyProperty.Register(nameof(CollapsedPaneWidth), typeof(double), typeof(FlyoutPageControl),
			new PropertyMetadata(48d, CollapsedPaneWidthChanged));

		public static readonly DependencyProperty DetailTitleProperty = DependencyProperty.Register("DetailTitle", typeof(string), typeof(FlyoutPageControl), new PropertyMetadata(default(string)));

		public static readonly DependencyProperty DetailTitleIconProperty = DependencyProperty.Register(nameof(DetailTitleIcon), typeof(WImageSource), typeof(FlyoutPageControl), new PropertyMetadata(default(WImageSource)));

		public static readonly DependencyProperty DetailTitleViewProperty = DependencyProperty.Register(nameof(DetailTitleView), typeof(View), typeof(FlyoutPageControl), new PropertyMetadata(default(View), OnTitleViewPropertyChanged));

		public static readonly DependencyProperty ToolbarForegroundProperty = DependencyProperty.Register("ToolbarForeground", typeof(WBrush), typeof(FlyoutPageControl),
			new PropertyMetadata(default(WBrush)));

		public static readonly DependencyProperty ToolbarBackgroundProperty = DependencyProperty.Register("ToolbarBackground", typeof(WBrush), typeof(FlyoutPageControl),
			new PropertyMetadata(default(WBrush)));

		public static readonly DependencyProperty FlyoutTitleVisibilityProperty = DependencyProperty.Register(nameof(FlyoutTitleVisibility), typeof(Visibility), typeof(FlyoutPageControl),
			new PropertyMetadata(default(Visibility)));

		public static readonly DependencyProperty DetailTitleVisibilityProperty = DependencyProperty.Register("DetailTitleVisibility", typeof(Visibility), typeof(FlyoutPageControl),
			new PropertyMetadata(default(Visibility)));

		public static readonly DependencyProperty DetailTitleViewVisibilityProperty = DependencyProperty.Register(nameof(DetailTitleViewVisibility), typeof(Visibility), typeof(FlyoutPageControl),
			new PropertyMetadata(default(Visibility)));

		public static readonly DependencyProperty FlyoutToolbarVisibilityProperty = DependencyProperty.Register(nameof(FlyoutToolbarVisibility), typeof(Visibility), typeof(FlyoutPageControl),
			new PropertyMetadata(default(Visibility)));

		public static readonly DependencyProperty ContentTogglePaneButtonVisibilityProperty = DependencyProperty.Register(nameof(ContentTogglePaneButtonVisibility), typeof(Visibility), typeof(FlyoutPageControl),
			new PropertyMetadata(default(Visibility)));

		CommandBar _commandBar;
		readonly ToolbarPlacementHelper _toolbarPlacementHelper = new ToolbarPlacementHelper();
		bool _firstLoad;

		public bool ShouldShowToolbar
		{
			get { return _toolbarPlacementHelper.ShouldShowToolBar; }
			set { _toolbarPlacementHelper.ShouldShowToolBar = value; }
		}

		TaskCompletionSource<CommandBar> _commandBarTcs;
		FrameworkElement _flyoutPresenter;
		FrameworkElement _detailPresenter;
		SplitView _split;
	    ToolbarPlacement _toolbarPlacement;
		bool _toolbarDynamicOverflowEnabled = true;
		FrameworkElement _titleViewPresenter;
		TitleViewManager _titleViewManager;
		private protected virtual string FlyoutPresenterTemplateName => "FlyoutPresenter";
		public FlyoutPageControl()
		{
			DefaultStyleKey = typeof(FlyoutPageControl);

			DetailTitleVisibility = WVisibility.Collapsed;

			CollapseStyle = CollapseStyle.Full;
		}

		public FrameworkElement Detail
		{
			get { return (FrameworkElement)GetValue(DetailProperty); }
			set { SetValue(DetailProperty, value); }
		}

		public Windows.Foundation.Size DetailSize
		{
			get
			{
				double height = ActualHeight;
				double width = ActualWidth;

				if (_commandBar != null)
					height -= _commandBar.ActualHeight;

				if (ShouldShowSplitMode && IsPaneOpen)
				{
					if (_split != null)
						width -= _split.OpenPaneLength;
					else if (_detailPresenter != null)
						width -= _flyoutPresenter.ActualWidth;
				}

				return new Windows.Foundation.Size(Math.Max(width, 0), Math.Max(height, 0));
			}
		}

		public string DetailTitle
		{
			get { return (string)GetValue(DetailTitleProperty); }
			set { SetValue(DetailTitleProperty, value); }
		}

		public WImageSource DetailTitleIcon
		{
			get { return (WImageSource)GetValue(DetailTitleIconProperty); }
			set { SetValue(DetailTitleIconProperty, value); }
		}

		public View DetailTitleView
		{
			get { return (View)GetValue(DetailTitleViewProperty); }
			set { SetValue(DetailTitleViewProperty, value); }
		}

		public WVisibility DetailTitleVisibility
		{
			get { return (WVisibility)GetValue(DetailTitleVisibilityProperty); }
			set { SetValue(DetailTitleVisibilityProperty, value); }
		}

		public Visibility DetailTitleViewVisibility
		{
			get { return (Visibility)GetValue(DetailTitleViewVisibilityProperty); }
			set { SetValue(DetailTitleViewVisibilityProperty, value); }
		}

		public bool IsPaneOpen
		{
			get { return (bool)GetValue(IsPaneOpenProperty); }
			set { SetValue(IsPaneOpenProperty, value); }
		}

		public FrameworkElement Flyout
		{
			get { return (FrameworkElement)GetValue(FlyoutProperty); }
			set { SetValue(FlyoutProperty, value); }
		}

		public Windows.Foundation.Size FlyoutSize
		{
			get
			{
				double height = ActualHeight;
				double width = 0;

				// On first load, the _commandBar will still occupy space by the time this is called.
				// Check ShouldShowToolbar to make sure the _commandBar will still be there on render.
				if ((_firstLoad || !ShouldShowSplitMode) && _commandBar != null && ShouldShowToolbar)
				{
					height -= _commandBar.ActualHeight;
					_firstLoad = false;
				}

				if (_split != null)
					width = _split.OpenPaneLength;
				else if (_flyoutPresenter != null)
					width = _flyoutPresenter.ActualWidth;

				return new Windows.Foundation.Size(Math.Max(width, 0), Math.Max(height, 0));
			}
		}

		public string FlyoutTitle
		{
			get { return (string)GetValue(FlyoutTitleProperty); }
			set { SetValue(FlyoutTitleProperty, value); }
		}

		public WVisibility FlyoutTitleVisibility
		{
			get { return (WVisibility)GetValue(FlyoutTitleVisibilityProperty); }
			set { SetValue(FlyoutTitleVisibilityProperty, value); }
		}

		public WVisibility FlyoutToolbarVisibility
		{
			get { return (WVisibility)GetValue(FlyoutToolbarVisibilityProperty); }
			set { SetValue(FlyoutToolbarVisibilityProperty, value); }
		}

		public bool ShouldShowSplitMode
		{
			get { return (bool)GetValue(ShouldShowSplitModeProperty); }
			set { SetValue(ShouldShowSplitModeProperty, value); }
		}

		public CollapseStyle CollapseStyle
		{
			get { return (CollapseStyle)GetValue(CollapseStyleProperty); }
			set { SetValue(CollapseStyleProperty, value); }
		}

	    public ToolbarPlacement ToolbarPlacement
	    {
	        get { return _toolbarPlacement; }
	        set
	        {
	            _toolbarPlacement = value;
	            _toolbarPlacementHelper.UpdateToolbarPlacement();
	        }
	    }
		
		public bool ToolbarDynamicOverflowEnabled
		{
			get { return _toolbarDynamicOverflowEnabled; }
			set
			{
				_toolbarDynamicOverflowEnabled = value;
				UpdateToolbarDynamicOverflowEnabled();
			}
		}

		public WVisibility ContentTogglePaneButtonVisibility
		{
			get { return (WVisibility)GetValue(ContentTogglePaneButtonVisibilityProperty); }
			set { SetValue(ContentTogglePaneButtonVisibilityProperty, value); }
		}

		public double CollapsedPaneWidth
		{
			get { return (double)GetValue(CollapsedPaneWidthProperty); }
			set { SetValue(CollapsedPaneWidthProperty, value); }
		}

		public WBrush ToolbarBackground
		{
			get { return (WBrush)GetValue(ToolbarBackgroundProperty); }
			set { SetValue(ToolbarBackgroundProperty, value); }
		}

		public WBrush ToolbarForeground
		{
			get { return (WBrush)GetValue(ToolbarForegroundProperty); }
			set { SetValue(ToolbarForegroundProperty, value); }
		}

		public bool ShouldShowNavigationBar
		{
			get { return (bool)GetValue(ShouldShowNavigationBarProperty); }
			set { SetValue(ShouldShowNavigationBarProperty, value); }
		}

		Task<CommandBar> IToolbarProvider.GetCommandBarAsync()
		{
			if (_commandBar != null)
				return Task.FromResult(_commandBar);

			_commandBarTcs = new TaskCompletionSource<CommandBar>();
			ApplyTemplate();

			var commandBarFromTemplate = _commandBarTcs.Task;
			_commandBarTcs = null;

			return commandBarFromTemplate;
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_split = GetTemplateChild("SplitView") as SplitView;
			if (_split == null)
				return;

			var paneToggle = GetTemplateChild("PaneTogglePane") as Microsoft.UI.Xaml.Controls.Button;
			if (paneToggle != null)
				paneToggle.Click += OnToggleClicked;

			var contentToggle = GetTemplateChild("ContentTogglePane") as Microsoft.UI.Xaml.Controls.Button;
			if (contentToggle != null)
				contentToggle.Click += OnToggleClicked;

			_flyoutPresenter = GetTemplateChild(FlyoutPresenterTemplateName) as FrameworkElement;
			_detailPresenter = GetTemplateChild("DetailPresenter") as FrameworkElement;
			_titleViewPresenter = GetTemplateChild("TitleViewPresenter") as FrameworkElement;

			_commandBar = GetTemplateChild("CommandBar") as CommandBar;
			_toolbarPlacementHelper.Initialize(_commandBar, () => ToolbarPlacement, GetTemplateChild);
			UpdateToolbarDynamicOverflowEnabled();
			
			UpdateMode(); 

			if (_commandBarTcs != null)
				_commandBarTcs.SetResult(_commandBar);

			_titleViewManager = new TitleViewManager(this);
		}

		static void OnShouldShowSplitModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			((FlyoutPageControl)dependencyObject).UpdateMode();
		}

		static void CollapseStyleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((FlyoutPageControl)dependencyObject).UpdateMode();
		}

		static void CollapsedPaneWidthChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			((FlyoutPageControl)dependencyObject).UpdateMode();
		}

		static void OnTitleViewPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			((FlyoutPageControl)dependencyObject)._titleViewManager?.OnTitleViewPropertyChanged();
		}

		void OnToggleClicked(object sender, RoutedEventArgs args)
		{
			IsPaneOpen = !IsPaneOpen;
		}

		void UpdateMode()
		{
			if (_split == null)
			{
				return;
			}

			_split.DisplayMode = ShouldShowSplitMode
				? SplitViewDisplayMode.Inline
				: CollapseStyle == CollapseStyle.Full ? SplitViewDisplayMode.Overlay : SplitViewDisplayMode.CompactOverlay;

			_split.CompactPaneLength = CollapsedPaneWidth;

			if (_split.DisplayMode == SplitViewDisplayMode.Inline)
			{
				// If we've determined that the pane will always be open, then there's no
				// reason to display the show/hide pane button in the master
				FlyoutToolbarVisibility = WVisibility.Collapsed;
			}

			// If we're in compact mode or the pane is always open,
			// we don't need to display the content pane's toggle button
			ContentTogglePaneButtonVisibility = _split.DisplayMode == SplitViewDisplayMode.Overlay
				? WVisibility.Visible
				: WVisibility.Collapsed;

			if (ContentTogglePaneButtonVisibility == WVisibility.Visible)
				DetailTitleVisibility = WVisibility.Visible;

			if (DetailTitleVisibility == WVisibility.Visible && !ShouldShowNavigationBar)
				DetailTitleVisibility = WVisibility.Collapsed;

			_firstLoad = true;
		}

		View ITitleViewRendererController.TitleView => DetailTitleView;
		FrameworkElement ITitleViewRendererController.TitleViewPresenter => _titleViewPresenter;
		Visibility ITitleViewRendererController.TitleViewVisibility
		{
			get => DetailTitleViewVisibility;
			set => DetailTitleViewVisibility = value;
		}

		CommandBar ITitleViewRendererController.CommandBar { get => _commandBar; }

        void UpdateToolbarDynamicOverflowEnabled()
        {
            if (_commandBar != null)
            {
                _commandBar.IsDynamicOverflowEnabled = ToolbarDynamicOverflowEnabled;
            }
        }

    }

	public class MasterDetailControl : FlyoutPageControl
	{
		public FrameworkElement Master
		{
			get => Flyout;
			set => Flyout = value;
		}

		public string MasterTitle
		{
			get => FlyoutTitle;
			set => FlyoutTitle = value;
		}

		public WVisibility MasterTitleVisibility
		{
			get => FlyoutTitleVisibility;
			set => FlyoutTitleVisibility = value;
		}

		public WVisibility MasterToolbarVisibility
		{
			get => FlyoutToolbarVisibility;
			set => FlyoutToolbarVisibility = value;
		}

		public Windows.Foundation.Size MasterSize => FlyoutSize;

		private protected override string FlyoutPresenterTemplateName => "MasterPresenter";

	}

}
