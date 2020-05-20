using UIKit;

namespace System.Maui.Platform
{
	public partial class ProgressBarRenderer : AbstractViewRenderer<IProgress, UIProgressView>
	{
		protected override UIProgressView CreateView()
		{
			return new UIProgressView(UIProgressViewStyle.Default);
		}

		public static void MapPropertyProgress(IViewRenderer renderer, IProgress progressBar)
		{
			if (!(renderer.NativeView is UIProgressView uIProgressView))
				return;

			uIProgressView.Progress = (float)progressBar.Progress;
		}

		public static void MapPropertyProgressColor(IViewRenderer renderer, IProgress progressBar)
		{
			if (!(renderer.NativeView is UIProgressView uIProgressView))
				return;

			uIProgressView.ProgressTintColor = progressBar.ProgressColor == Color.Default ? null : progressBar.ProgressColor.ToNativeColor();
		}
	}
}