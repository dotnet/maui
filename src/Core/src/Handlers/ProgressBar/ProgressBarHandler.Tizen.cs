using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, ProgressBar>
	{
		protected override ProgressBar CreatePlatformView() => new ProgressBar();

		public static void MapProgress(ProgressBarHandler handler, IProgress progress)
		{
			handler.PlatformView?.UpdateProgress(progress);
		}

		public static void MapProgressColor(ProgressBarHandler handler, IProgress progress)
		{
			handler.PlatformView?.UpdateProgressColor(progress);
		}

		public static void MapProgressColor(ProgressBarHandler handler, IProgress progress)
		{
			handler.PlatformView?.UpdateProgressColor(progress);
		}
	}
}