using Gtk;

namespace Microsoft.Maui.Handlers
{
	// https://developer.gnome.org/gtk3/stable/GtkProgressBar.html
	public partial class ProgressBarHandler : ViewHandler<IProgress, ProgressBar>
	{
		protected override ProgressBar CreateNativeView()
		{
			return new ProgressBar();
		}

		public static void MapProgress(ProgressBarHandler handler, IProgress progress)
		{
			handler.NativeView?.UpdateProgress(progress);
		}
	}
}