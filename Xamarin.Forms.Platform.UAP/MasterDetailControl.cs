using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using WImageSource = Windows.UI.Xaml.Media.ImageSource;

namespace Xamarin.Forms.Platform.UWP
{
	public class MasterDetailControl : Control, IToolbarProvider
	{
		public static readonly DependencyProperty MasterProperty = DependencyProperty.Register("Master", typeof(FrameworkElement), typeof(MasterDetailControl),
			new PropertyMetadata(default(FrameworkElement)));

		public static readonly DependencyProperty MasterTitleProperty = DependencyProperty.Register("MasterTitle", typeof(string), typeof(MasterDetailControl), new PropertyMetadata(default(string)));

		public static readonly DependencyProperty DetailProperty = DependencyProperty.Register("Detail", typeof(FrameworkElement), typeof(MasterDetailControl),
			new PropertyMetadata(default(FrameworkElement)));

		public static readonly DependencyProperty IsPaneOpenProperty = DependencyProperty.Register("IsPaneOpen", typeof(bool), typeof(MasterDetailControl), new PropertyMetadata(default(bool)));

		public static readonly DependencyProperty ShouldShowSplitModeProperty = DependencyProperty.Register(nameof(ShouldShowSplitMode), typeof(bool), typeof(MasterDetailControl),
			new PropertyMetadata(default(bool), OnShouldShowSplitModeChanged));

		public static readonly DependencyProperty ShouldShowNavigationBarProperty = DependencyProperty.Register(nameof(ShouldShowNavigationBar), typeof(bool), typeof(MasterDetailControl),
		new PropertyMetadata(true, OnShouldShowSplitModeChanged));

		public static readonly DependencyProperty CollapseStyleProperty = DependencyProperty.Register(nameof(CollapseStyle), typeof(CollapseStyle), 
			typeof(MasterDetailControl), new PropertyMetadata(CollapseStyle.Full, CollapseStyleChanged));

		public static readonly DependencyProperty CollapsedPaneWidthProperty = DependencyProperty.Register(nameof(CollapsedPaneWidth), typeof(double), typeof(MasterDetailControl),
			new PropertyMetadata(48d, CollapsedPaneWidthChanged));

		public static readonly DependencyProperty DetailTitleProperty = DependencyProperty.Register("DetailTitle", typeof(string), typeof(MasterDetailControl), new PropertyMetadata(default(string)));

		public static readonly DependencyProperty DetailTitleIconProperty = DependencyProperty.Register(nameof(DetailTitleIcon), typeof(WImageSource), typeof(MasterDetailControl), new PropertyMetadata(default(WImageSource)));

		public static readonly DependencyProperty DetailTitleViewProperty = DependencyProperty.Register(nameof(DetailTitleView), typeof(View), typeof(MasterDetailControl), new PropertyMetadata(default(View), OnTitleViewPropertyChanged));

		public static readonly DependencyProperty ToolbarForegroundProperty = DependencyProperty.Register("ToolbarForeground", typeof(Brush), typeof(MasterDetailControl),
			new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty ToolbarBackgroundProperty = DependencyProperty.Register("ToolbarBackground", typeof(Brush), typeof(MasterDetailControl),
			new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty MasterTitleVisibilityProperty = DependencyProperty.Register("MasterTitleVisibility", typeof(Visibility), typeof(MasterDetailControl),
			new PropertyMetadata(default(Visibility)));

		public static readonly DependencyProperty DetailTitleVisibilityProperty = DependencyProperty.Register("DetailTitleVisibility", typeof(Visibility), typeof(MasterDetailControl),
			new PropertyMetadata(default(Visibility)));

		public static readonly DependencyProperty DetailTitleViewVisibilityProperty = DependencyProperty.Register(nameof(DetailTitleViewVisibility), typeof(Visibility), typeof(MasterDetailControl),
			new PropertyMetadata(default(Visibility)));

		public static readonly DependencyProperty MasterToolbarVisibilityProperty = DependencyProperty.Register("MasterToolbarVisibility", typeof(Visibility), typeof(MasterDetailControl),
			new PropertyMetadata(default(Visibility)));

		public static readonly DependencyProperty ContentTogglePaneButtonVisibilityProperty = DependencyProperty.Register(nameof(ContentTogglePaneButtonVisibility), typeof(Visibility), typeof(MasterDetailControl),
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
		FrameworkElement _masterPresenter;
		FrameworkElement _detailPresenter;
		SplitView _split;
	    ToolbarPlacement _toolbarPlacement;
		FrameworkElement _titleViewPresenter;

	    public MasterDetailControl()
		{
			DefaultStyleKey = typeof(MasterDetailControl);

			DetailTitleVisibility = Visibility.Collapsed;

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
						width -= _masterPresenter.ActualWidth;
				}

				return new Windows.Foundation.Size(width >= 0 ? width : 0, height);
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

		public Visibility DetailTitleVisibility
		{
			get { return (Visibility)GetValue(DetailTitleVisibilityProperty); }
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

		public FrameworkElement Master
		{
			get { return (FrameworkElement)GetValue(MasterProperty); }
			set { SetValue(MasterProperty, value); }
		}

		public Windows.Foundation.Size MasterSize
		{
			get
			{
				// Use the ActualHeight of the _masterPresenter to automatically adjust for the Master Title
				double height = _masterPresenter?.ActualHeight ?? 0;

				// If there's no content, use the height of the control to make sure the background color expands.
				if (height == 0)
					height = ActualHeight;

				double width = 0;

				// On first load, the _commandBar will still occupy space by the time this is called.
				// Check ShouldShowToolbar to make sure the _commandBar will still be there on render.
				if (_firstLoad && _commandBar != null && ShouldShowToolbar)
				{
					height -= _commandBar.ActualHeight;
					_firstLoad = false;
				}

				if (_split != null)
					width = _split.OpenPaneLength;
				else if (_masterPresenter != null)
					width = _masterPresenter.ActualWidth;

				return new Windows.Foundation.Size(width, height);
			}
		}

		public string MasterTitle
		{
			get { return (string)GetValue(MasterTitleProperty); }
			set { SetValue(MasterTitleProperty, value); }
		}

		public Visibility MasterTitleVisibility
		{
			get { return (Visibility)GetValue(MasterTitleVisibilityProperty); }
			set { SetValue(MasterTitleVisibilityProperty, value); }
		}

		public Visibility MasterToolbarVisibility
		{
			get { return (Visibility)GetValue(MasterToolbarVisibilityProperty); }
			set { SetValue(MasterToolbarVisibilityProperty, value); }
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

	    public Visibility ContentTogglePaneButtonVisibility
		{
			get { return (Visibility)GetValue(ContentTogglePaneButtonVisibilityProperty); }
			set { SetValue(ContentTogglePaneButtonVisibilityProperty, value); }
		}

		public double CollapsedPaneWidth
		{
			get { return (double)GetValue(CollapsedPaneWidthProperty); }
			set { SetValue(CollapsedPaneWidthProperty, value); }
		}

		public Brush ToolbarBackground
		{
			get { return (Brush)GetValue(ToolbarBackgroundProperty); }
			set { SetValue(ToolbarBackgroundProperty, value); }
		}

		public Brush ToolbarForeground
		{
			get { return (Brush)GetValue(ToolbarForegroundProperty); }
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

			var paneToggle = GetTemplateChild("PaneTogglePane") as Windows.UI.Xaml.Controls.Button;
			if (paneToggle != null)
				paneToggle.Click += OnToggleClicked;

			var contentToggle = GetTemplateChild("ContentTogglePane") as Windows.UI.Xaml.Controls.Button;
			if (contentToggle != null)
				contentToggle.Click += OnToggleClicked;

			_masterPresenter = GetTemplateChild("MasterPresenter") as FrameworkElement;
			_detailPresenter = GetTemplateChild("DetailPresenter") as FrameworkElement;
			_titleViewPresenter = GetTemplateChild("TitleViewPresenter") as FrameworkElement;

			_commandBar = GetTemplateChild("CommandBar") as CommandBar;
			_toolbarPlacementHelper.Initialize(_commandBar, () => ToolbarPlacement, GetTemplateChild);
			
			UpdateMode(); 

			if (_commandBarTcs != null)
				_commandBarTcs.SetResult(_commandBar);
		}

		static void OnShouldShowSplitModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			((MasterDetailControl)dependencyObject).UpdateMode();
		}

		static void CollapseStyleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((MasterDetailControl)dependencyObject).UpdateMode();
		}

		static void CollapsedPaneWidthChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			((MasterDetailControl)dependencyObject).UpdateMode();
		}

		static void OnTitleViewPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			((MasterDetailControl)dependencyObject).UpdateTitleViewPresenter();
		}

		void OnToggleClicked(object sender, RoutedEventArgs args)
		{
			IsPaneOpen = !IsPaneOpen;
		}

		void OnTitleViewPresenterLoaded(object sender, RoutedEventArgs e)
		{
			if (DetailTitleView == null || _titleViewPresenter == null || _commandBar == null)
				return;

			_titleViewPresenter.Width = _commandBar.ActualWidth;
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
				MasterToolbarVisibility = Visibility.Collapsed;
			}

			// If we're in compact mode or the pane is always open,
			// we don't need to display the content pane's toggle button
			ContentTogglePaneButtonVisibility = _split.DisplayMode == SplitViewDisplayMode.Overlay 
				? Visibility.Visible 
				: Visibility.Collapsed;
			
			if (ContentTogglePaneButtonVisibility == Visibility.Visible)
				DetailTitleVisibility = Visibility.Visible;

			if (DetailTitleVisibility == Visibility.Visible && !ShouldShowNavigationBar)
				DetailTitleVisibility = Visibility.Collapsed;

			_firstLoad = true;
		}

		void UpdateTitleViewPresenter()
		{
			if (DetailTitleView == null)
			{
				DetailTitleViewVisibility = Visibility.Collapsed;

				if (_titleViewPresenter != null)
					_titleViewPresenter.Loaded -= OnTitleViewPresenterLoaded;
			}
			else
			{
				DetailTitleViewVisibility = Visibility.Visible;

				if (_titleViewPresenter != null)
					_titleViewPresenter.Loaded += OnTitleViewPresenterLoaded;
			}
		}
	}
}
