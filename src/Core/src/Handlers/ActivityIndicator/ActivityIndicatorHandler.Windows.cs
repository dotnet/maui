#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, MauiActivityIndicator>
	{
		object? _foregroundDefault;

		protected override MauiActivityIndicator CreateNativeView() => new MauiActivityIndicator
		{
			IsIndeterminate = true,
			Style = UI.Xaml.Application.Current.Resources["MauiActivityIndicatorStyle"] as UI.Xaml.Style
		};

		protected override void SetupDefaults(MauiActivityIndicator nativeView)
		{
			_foregroundDefault = nativeView.GetForegroundCache();

			base.SetupDefaults(nativeView);
		}

		public static void MapIsRunning(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.NativeView?.UpdateIsRunning(activityIndicator);
		}

		public static void MapColor(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.NativeView?.UpdateColor(activityIndicator, handler._foregroundDefault);
		}
	}
}