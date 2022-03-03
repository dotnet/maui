#nullable enable
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, ProgressBar>
	{
		object? _foregroundDefault;

		protected override ProgressBar CreatePlatformView() => new() { Minimum = 0, Maximum = 1 };

		protected override void ConnectHandler(ProgressBar platformView)
		{
			SetupDefaults(platformView);
		}

		void SetupDefaults(ProgressBar platformView)
		{
			_foregroundDefault = platformView.GetForegroundCache();
		}

		public static void MapProgress(IProgressBarHandler handler, IProgress progress)
		{
			handler.PlatformView?.UpdateProgress(progress);
		}

		public static void MapProgressColor(IProgressBarHandler handler, IProgress progress)
		{
			if (handler is ProgressBarHandler platformHandler)
			{
				platformHandler.PlatformView?.UpdateProgressColor(progress, platformHandler._foregroundDefault);
			}
		}
	}
}