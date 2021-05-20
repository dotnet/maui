using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapIsRunning(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator) { }
		public static void MapColor(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator) { }
	}
}