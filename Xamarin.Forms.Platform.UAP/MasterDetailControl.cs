using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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

		public static readonly DependencyProperty ShouldShowSplitModeProperty = DependencyProperty.Register("ShouldShowSplitMode", typeof(bool), typeof(MasterDetailControl),
			new PropertyMetadata(default(bool), OnShouldShowSplitModeChanged));

		public static readonly DependencyProperty DetailTitleProperty = DependencyProperty.Register("DetailTitle", typeof(string), typeof(MasterDetailControl), new PropertyMetadata(default(string)));

		public static readonly DependencyProperty ToolbarForegroundProperty = DependencyProperty.Register("ToolbarForeground", typeof(Brush), typeof(MasterDetailControl),
			new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty ToolbarBackgroundProperty = DependencyProperty.Register("ToolbarBackground", typeof(Brush), typeof(MasterDetailControl),
			new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty MasterTitleVisibilityProperty = DependencyProperty.Register("MasterTitleVisibility", typeof(Visibility), typeof(MasterDetailControl),
			new PropertyMetadata(default(Visibility)));

		public static readonly DependencyProperty DetailTitleVisibilityProperty = DependencyProperty.Register("DetailTitleVisibility", typeof(Visibility), typeof(MasterDetailControl),
			new PropertyMetadata(default(Visibility)));

		public static readonly DependencyProperty MasterToolbarVisibilityProperty = DependencyProperty.Register("MasterToolbarVisibility", typeof(Visibility), typeof(MasterDetailControl),
			new PropertyMetadata(default(Visibility)));

		CommandBar _commandBar;

		TaskCompletionSource<CommandBar> _commandBarTcs;
		FrameworkElement _masterPresenter;
		FrameworkElement _detailPresenter;
		SplitView _split;

		public MasterDetailControl()
		{
			DefaultStyleKey = typeof(MasterDetailControl);
			MasterTitleVisibility = Visibility.Collapsed;
			DetailTitleVisibility = Visibility.Collapsed;
			if (Device.Idiom != TargetIdiom.Phone)
				MasterToolbarVisibility = Visibility.Collapsed;
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

				return new Windows.Foundation.Size(width, height);
			}
		}

		public string DetailTitle
		{
			get { return (string)GetValue(DetailTitleProperty); }
			set { SetValue(DetailTitleProperty, value); }
		}

		public Visibility DetailTitleVisibility
		{
			get { return (Visibility)GetValue(DetailTitleVisibilityProperty); }
			set { SetValue(DetailTitleVisibilityProperty, value); }
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
				double height = ActualHeight;
				double width = 0;

				if (_commandBar != null)
					height -= _commandBar.ActualHeight;

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

			_commandBar = GetTemplateChild("CommandBar") as CommandBar;

			UpdateMode();

			if (_commandBarTcs != null)
				_commandBarTcs.SetResult(_commandBar);
		}

		static void OnShouldShowSplitModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			((MasterDetailControl)dependencyObject).UpdateMode();
		}

		void OnToggleClicked(object sender, RoutedEventArgs args)
		{
			IsPaneOpen = !IsPaneOpen;
		}

		void UpdateMode()
		{
			if (_split == null)
				return;

			_split.DisplayMode = ShouldShowSplitMode ? SplitViewDisplayMode.Inline : SplitViewDisplayMode.Overlay;
		}
	}
}