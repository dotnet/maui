using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, UIProgressView>
	{
		protected override UIProgressView CreateNativeView()
		{
			return new UIProgressView(UIProgressViewStyle.Default);
		}

		public static void MapProgress(ProgressBarHandler handler, IProgress progress)
		{
			handler.NativeView?.UpdateProgress(progress);
		}

		public static void MapProgressColor(ProgressBarHandler handler, IProgress progress)
		{
			handler.NativeView?.UpdateProgressColor(progress);
		}
	}
}