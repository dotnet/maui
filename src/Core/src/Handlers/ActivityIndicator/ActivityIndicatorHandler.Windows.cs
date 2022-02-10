#nullable enable

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, MauiActivityIndicator>
	{
		protected override MauiActivityIndicator CreateNativeView() => new MauiActivityIndicator
		{
			IsIndeterminate = true,
			Style = UI.Xaml.Application.Current.Resources["MauiActivityIndicatorStyle"] as UI.Xaml.Style
		};

		public static void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.NativeView?.UpdateIsRunning(activityIndicator);
		}

		public static void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.NativeView?.UpdateColor(activityIndicator);
		}
	}
}