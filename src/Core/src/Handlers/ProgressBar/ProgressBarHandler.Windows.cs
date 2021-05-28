#nullable enable
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, ProgressBar>
	{
		protected override ProgressBar CreateNativeView() => new ProgressBar { Minimum = 0, Maximum = 1 };

		protected override void ConnectHandler(ProgressBar nativeView)
		{
			nativeView.ValueChanged += OnProgressBarValueChanged;
		}

		protected override void DisconnectHandler(ProgressBar nativeView)
		{
			nativeView.ValueChanged -= OnProgressBarValueChanged;
		}

		public static void MapProgress(ProgressBarHandler handler, IProgress progress)	
		{
			handler.NativeView?.UpdateProgress(progress);
		}

		void OnProgressBarValueChanged(object? sender, RangeBaseValueChangedEventArgs rangeBaseValueChangedEventArgs)
		{
			VirtualView?.InvalidateMeasure();
		}
	}
}