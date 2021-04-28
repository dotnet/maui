using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, ProgressBar>
	{
		protected override ProgressBar CreateNativeView() => new ProgressBar { Minimum = 0, Maximum = 1 };

		public static void MapProgress(ProgressBarHandler handler, IProgress progress)	
		{
			handler.NativeView?.UpdateProgress(progress);
		}
	}
}