using Tizen.UIExtensions.ElmSharp;
using EColor = ElmSharp.Color;
using EProgressBar = ElmSharp.ProgressBar;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, EProgressBar>
	{
		protected virtual EColor DefaultColor => ThemeConstants.ProgressBar.ColorClass.Default;

		protected override EProgressBar CreateNativeView()
		{
			var progressBar = new EProgressBar(NativeParent);
			progressBar.Color = DefaultColor;
			return progressBar;
		}

		void SetupDefaults(EProgressBar nativeView)
		{
			nativeView.Color = ThemeConstants.ProgressBar.ColorClass.Default;
		}

		public static void MapProgress(ProgressBarHandler handler, IProgress progress)
		{
			handler.NativeView?.UpdateProgress(progress);
		}
	}
}