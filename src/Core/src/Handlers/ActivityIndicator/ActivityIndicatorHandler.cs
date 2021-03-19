namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler
	{
		public static PropertyMapper<IActivityIndicator, ActivityIndicatorHandler> ActivityIndicatorMapper = new PropertyMapper<IActivityIndicator, ActivityIndicatorHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IActivityIndicator.IsRunning)] = MapIsRunning,
			[nameof(IActivityIndicator.Color)] = MapColor
		};

		public static void MapIsRunning(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.TypedNativeView?.UpdateIsRunning(activityIndicator);
		}

		public static void MapColor(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.TypedNativeView?.UpdateColor(activityIndicator);
		}

		public ActivityIndicatorHandler() : base(ActivityIndicatorMapper)
		{

		}

		public ActivityIndicatorHandler(PropertyMapper mapper) : base(mapper ?? ActivityIndicatorMapper)
		{

		}
	}
}