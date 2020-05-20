using WProgressBar = System.Windows.Controls.ProgressBar;

namespace System.Maui.Platform
{
	public partial class ProgressBarRenderer : AbstractViewRenderer<IProgress, WProgressBar>
	{
		protected override WProgressBar CreateView() => new WProgressBar { Minimum = 0, Maximum = 1 };

		public static void MapPropertyProgress(IViewRenderer renderer, IProgress progressBar) => (renderer as ProgressBarRenderer)?.UpdateProgress();
		public static void MapPropertyProgressColor(IViewRenderer renderer, IProgress progressBar) => (renderer as ProgressBarRenderer)?.UpdateProgressColor();

		public virtual void UpdateProgressColor()
		{
			TypedNativeView.UpdateDependencyColor(WProgressBar.ForegroundProperty, VirtualView.ProgressColor.IsDefault ? Color.DeepSkyBlue : VirtualView.ProgressColor);
		}

		public virtual void UpdateProgress()
		{
			TypedNativeView.Value = VirtualView.Progress;
		}
	}
}