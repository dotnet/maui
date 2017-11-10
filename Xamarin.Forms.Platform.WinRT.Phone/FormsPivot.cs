using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else
namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class FormsPivot : Pivot, IToolbarProvider
	{
		public static readonly DependencyProperty TitleVisibilityProperty = DependencyProperty.Register(nameof(TitleVisibility), typeof(Visibility), typeof(FormsPivot),
			new PropertyMetadata(Visibility.Collapsed));

		public static readonly DependencyProperty ToolbarForegroundProperty = DependencyProperty.Register(nameof(ToolbarForeground), typeof(Brush), typeof(FormsPivot), new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty ToolbarBackgroundProperty = DependencyProperty.Register(nameof(ToolbarBackground), typeof(Brush), typeof(FormsPivot), new PropertyMetadata(default(Brush)));

		CommandBar _commandBar;
#if WINDOWS_UWP
		readonly ToolbarPlacementHelper _toolbarPlacementHelper = new ToolbarPlacementHelper();

		public bool ShouldShowToolbar
		{
			get { return _toolbarPlacementHelper.ShouldShowToolBar; }
			set { _toolbarPlacementHelper.ShouldShowToolBar = value; }
		}
#endif
		TaskCompletionSource<CommandBar> _commandBarTcs;
	    ToolbarPlacement _toolbarPlacement;
		

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
#if WINDOWS_UWP
				_toolbarPlacementHelper. UpdateToolbarPlacement();
#endif
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

#if WINDOWS_UWP
			_toolbarPlacementHelper.Initialize(_commandBar, () => ToolbarPlacement, GetTemplateChild);
#endif

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
				Windows.UI.Xaml.Automation.AutomationProperties.SetName(pivotItem, page.Title);	
			}
		}
	}
}