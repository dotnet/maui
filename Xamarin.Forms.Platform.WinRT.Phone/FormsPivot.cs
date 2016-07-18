using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else
namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class FormsPivot : Pivot, IToolbarProvider
	{
		public static readonly DependencyProperty ToolbarVisibilityProperty = DependencyProperty.Register(nameof(ToolbarVisibility), typeof(Visibility), typeof(FormsPivot),
			new PropertyMetadata(Visibility.Collapsed));

		public static readonly DependencyProperty ToolbarForegroundProperty = DependencyProperty.Register(nameof(ToolbarForeground), typeof(Brush), typeof(FormsPivot), new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty ToolbarBackgroundProperty = DependencyProperty.Register(nameof(ToolbarBackground), typeof(Brush), typeof(FormsPivot), new PropertyMetadata(default(Brush)));

		CommandBar _commandBar;

		TaskCompletionSource<CommandBar> _commandBarTcs;

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

		public Visibility ToolbarVisibility
		{
			get { return (Visibility)GetValue(ToolbarVisibilityProperty); }
			set { SetValue(ToolbarVisibilityProperty, value); }
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
			TaskCompletionSource<CommandBar> tcs = _commandBarTcs;
			if (tcs != null)
			{
				tcs.SetResult(_commandBar);
			}
		}
	}
}