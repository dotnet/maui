#nullable enable
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, ProgressBar>
	{
		object? _foregroundDefault;

		protected override ProgressBar CreatePlatformView() => new() { Minimum = 0, Maximum = 1 };

		protected override void ConnectHandler(ProgressBar nativeView)
		{
			SetupDefaults(nativeView);
		}

		void SetupDefaults(ProgressBar nativeView)
		{
			_foregroundDefault = nativeView.GetForegroundCache();
		}

		public static void MapProgress(ProgressBarHandler handler, IProgress progress)
		{
			handler.NativeView?.UpdateProgress(progress);
		}

		public static void MapProgressColor(ProgressBarHandler handler, IProgress progress)
		{
			handler.NativeView?.UpdateProgressColor(progress, handler._foregroundDefault);
		}
	}
}