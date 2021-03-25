using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public sealed class PageControl : ContentControl, IToolbarProvider, ITitleViewRendererController
	{
		public static readonly DependencyProperty TitleVisibilityProperty = DependencyProperty.Register(nameof(TitleVisibility), typeof(Visibility), typeof(PageControl), new PropertyMetadata(Visibility.Visible));

		public static readonly DependencyProperty ToolbarBackgroundProperty = DependencyProperty.Register(nameof(ToolbarBackground), typeof(WBrush), typeof(PageControl),
			new PropertyMetadata(default(WBrush)));

		public static readonly DependencyProperty BackButtonTitleProperty = DependencyProperty.Register("BackButtonTitle", typeof(string), typeof(PageControl), new PropertyMetadata(false));

		public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register("ContentMargin", typeof(Microsoft.UI.Xaml.Thickness), typeof(PageControl),
			new PropertyMetadata(default(Microsoft.UI.Xaml.Thickness)));

		public static readonly DependencyProperty TitleIconProperty = DependencyProperty.Register(nameof(TitleIcon), typeof(WImageSource), typeof(PageControl), new PropertyMetadata(default(WImageSource)));

		public static readonly DependencyProperty TitleViewProperty = DependencyProperty.Register(nameof(TitleView), typeof(View), typeof(PageControl), new PropertyMetadata(default(View), OnTitleViewPropertyChanged));

		public static readonly DependencyProperty TitleViewVisibilityProperty = DependencyProperty.Register(nameof(TitleViewVisibility), typeof(Visibility), typeof(PageControl), new PropertyMetadata(Visibility.Collapsed));

		public static readonly DependencyProperty TitleInsetProperty = DependencyProperty.Register("TitleInset", typeof(double), typeof(PageControl), new PropertyMetadata(default(double)));

		public static readonly DependencyProperty TitleBrushProperty = DependencyProperty.Register("TitleBrush", typeof(WBrush), typeof(PageControl), new PropertyMetadata(null));

		CommandBar _commandBar;
		FrameworkElement _titleViewPresenter;

        ToolbarPlacement _toolbarPlacement;
		bool _toolbarDynamicOverflowEnabled = true;
		readonly ToolbarPlacementHelper _toolbarPlacementHelper = new ToolbarPlacementHelper();

		public bool ShouldShowToolbar
		{
			get { return _toolbarPlacementHelper.ShouldShowToolBar; }
			set { _toolbarPlacementHelper.ShouldShowToolBar = value; }
		}

		public WImageSource TitleIcon
		{
			get { return (WImageSource)GetValue(TitleIconProperty); }
			set { SetValue(TitleIconProperty, value); }
		}

		public View TitleView
		{
			get { return (View)GetValue(TitleViewProperty); }
			set { SetValue(TitleViewProperty, value); }
		}

		TaskCompletionSource<CommandBar> _commandBarTcs;
		Microsoft.UI.Xaml.Controls.ContentPresenter _presenter;
		TitleViewManager _titleViewManager;

		public PageControl()
		{
			Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultPageControlStyle"] as Microsoft.UI.Xaml.Style;
		}

		public string BackButtonTitle
		{
			get { return (string)GetValue(BackButtonTitleProperty); }
			set { SetValue(BackButtonTitleProperty, value); }
		}

		public double ContentHeight
		{
			get { return _presenter != null ? _presenter.ActualHeight : 0; }
		}

		public Microsoft.UI.Xaml.Thickness ContentMargin
		{
			get { return (Microsoft.UI.Xaml.Thickness)GetValue(ContentMarginProperty); }
			set { SetValue(ContentMarginProperty, value); }
		}

		public double ContentWidth
		{
			get { return _presenter != null ? _presenter.ActualWidth : 0; }
		}

		public WBrush ToolbarBackground
		{
			get { return (WBrush)GetValue(ToolbarBackgroundProperty); }
			set { SetValue(ToolbarBackgroundProperty, value); }
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

		public Visibility TitleVisibility
		{
			get { return (Visibility)GetValue(TitleVisibilityProperty); }
			set { SetValue(TitleVisibilityProperty, value); }
		}

		public Visibility TitleViewVisibility
		{
			get { return (Visibility)GetValue(TitleViewVisibilityProperty); }
			set { SetValue(TitleViewVisibilityProperty, value); }
		}

		public WBrush TitleBrush
		{
			get { return (WBrush)GetValue(TitleBrushProperty); }
			set { SetValue(TitleBrushProperty, value); }
		}

		public double TitleInset
		{
			get { return (double)GetValue(TitleInsetProperty); }
			set { SetValue(TitleInsetProperty, value); }
		}

		FrameworkElement ITitleViewRendererController.TitleViewPresenter => _titleViewPresenter;
		CommandBar ITitleViewRendererController.CommandBar => _commandBar;

		Task<CommandBar> IToolbarProvider.GetCommandBarAsync()
		{
			if (_commandBar != null)
				return Task.FromResult(_commandBar);

			_commandBarTcs = new TaskCompletionSource<CommandBar>();
			ApplyTemplate();
			return _commandBarTcs.Task;
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_presenter = GetTemplateChild("presenter") as Microsoft.UI.Xaml.Controls.ContentPresenter;

			_titleViewPresenter = GetTemplateChild("TitleViewPresenter") as FrameworkElement;

			_commandBar = GetTemplateChild("CommandBar") as CommandBar;

			_titleViewManager = new TitleViewManager(this);

			_toolbarPlacementHelper.Initialize(_commandBar, () => ToolbarPlacement, GetTemplateChild);
			UpdateToolbarDynamicOverflowEnabled();

			TaskCompletionSource<CommandBar> tcs = _commandBarTcs;
		    tcs?.SetResult(_commandBar);
		}

		static void OnTitleViewPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)	
		{	
			((PageControl)dependencyObject)._titleViewManager?.OnTitleViewPropertyChanged();	
		}
		
		void UpdateToolbarDynamicOverflowEnabled()
		{
			if (_commandBar != null)
			{
				_commandBar.IsDynamicOverflowEnabled = ToolbarDynamicOverflowEnabled;
			}
		}
	}
}
