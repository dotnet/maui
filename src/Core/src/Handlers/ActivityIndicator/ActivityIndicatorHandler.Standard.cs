using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static partial void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator) { }
		public static partial void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator) { }
	}
}