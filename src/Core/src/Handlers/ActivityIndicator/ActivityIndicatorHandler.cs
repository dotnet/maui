namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler
	{
		public static PropertyMapper<IActivityIndicator, ActivityIndicatorHandler> ActivityIndicatorMapper = new PropertyMapper<IActivityIndicator, ActivityIndicatorHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IActivityIndicator.Color)] = MapColor,
			[nameof(IActivityIndicator.IsRunning)] = MapIsRunning,
		};

		public ActivityIndicatorHandler() : base(ActivityIndicatorMapper)
		{

		}

		public ActivityIndicatorHandler(PropertyMapper mapper) : base(mapper ?? ActivityIndicatorMapper)
		{

		}
	}
}