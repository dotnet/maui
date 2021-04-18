using Gtk;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<ICheckBox, Spinner>
	{
		protected override Spinner CreateNativeView()
		{
			return new Spinner();
		}

		public static void MapIsRunning(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.NativeView?.UpdateIsRunning(activityIndicator);
		}

		[MissingMapper]
		public static void MapColor(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator) { }
	}
}
