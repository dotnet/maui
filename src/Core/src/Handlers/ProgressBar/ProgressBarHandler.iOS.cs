using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, UIProgressView>
	{
		protected override UIProgressView CreatePlatformView()
		{
			return new UIProgressView(UIProgressViewStyle.Default);
		}

		public static void MapProgress(ProgressBarHandler handler, IProgress progress)
		{
			handler.PlatformView?.UpdateProgress(progress);
		}

		public static void MapProgressColor(ProgressBarHandler handler, IProgress progress)
		{
			handler.PlatformView?.UpdateProgressColor(progress);
		}
	}
}