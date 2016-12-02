using System.Threading.Tasks;
using Windows.UI.Xaml;
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
	}
}