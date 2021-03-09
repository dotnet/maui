using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class FormsPivot : Pivot, IToolbarProvider
	{
		public static readonly DependencyProperty TitleVisibilityProperty = DependencyProperty.Register(nameof(TitleVisibility), typeof(Visibility), typeof(FormsPivot), new PropertyMetadata(Visibility.Collapsed));

		public static readonly DependencyProperty ToolbarForegroundProperty = DependencyProperty.Register(nameof(ToolbarForeground), typeof(WBrush), typeof(FormsPivot), new PropertyMetadata(default(WBrush)));

		public static readonly DependencyProperty ToolbarBackgroundProperty = DependencyProperty.Register(nameof(ToolbarBackground), typeof(WBrush), typeof(FormsPivot), new PropertyMetadata(default(WBrush)));

		CommandBar _commandBar;

		readonly ToolbarPlacementHelper _toolbarPlacementHelper = new ToolbarPlacementHelper();

		public bool ShouldShowToolbar
		{
			get { return _toolbarPlacementHelper.ShouldShowToolBar; }
			set { _toolbarPlacementHelper.ShouldShowToolBar = value; }
		}

		TaskCompletionSource<CommandBar> _commandBarTcs;
	    ToolbarPlacement _toolbarPlacement;
		bool _toolbarDynamicOverflowEnabled = true;

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

		public Visibility TitleVisibility
		{
			get { return (Visibility)GetValue(TitleVisibilityProperty); }
			set { SetValue(TitleVisibilityProperty, value); }
		}

        public ToolbarPlacement ToolbarPlacement
	    {
	        get { return _toolbarPlacement; }
	        set
	        {
	            _toolbarPlacement = value;

				_toolbarPlacementHelper. UpdateToolbarPlacement();
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

			_commandBar = GetTemplateChild("CommandBar") as CommandBar;


			_toolbarPlacementHelper.Initialize(_commandBar, () => ToolbarPlacement, GetTemplateChild);
			UpdateToolbarDynamicOverflowEnabled();

			TaskCompletionSource<CommandBar> tcs = _commandBarTcs;
			tcs?.SetResult(_commandBar);
		}

		protected override DependencyObject GetContainerForItemOverride()
		{
			var containerItem = base.GetContainerForItemOverride();

			var pivotItem = containerItem as PivotItem;

			if (pivotItem != null)
			{
				// We need to know when the data context changes so we can set the automation name to the page title
				pivotItem.DataContextChanged += SetPivotItemAutomationName;
			}

			return containerItem;
		}

		static void SetPivotItemAutomationName(FrameworkElement frameworkElement, 
			DataContextChangedEventArgs dataContextChangedEventArgs)
		{
			var pivotItem = frameworkElement as PivotItem;
			var page = dataContextChangedEventArgs.NewValue as Page;

			if (pivotItem != null && page?.Title != null)
			{
				// This way we can find tabs with automation (for testing, etc.)
				Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(pivotItem, page.Title);	
			}
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