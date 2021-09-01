namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler
	{
		public static IPropertyMapper<IActivityIndicator, ActivityIndicatorHandler> ActivityIndicatorMapper = new PropertyMapper<IActivityIndicator, ActivityIndicatorHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IActivityIndicator.Color)] = MapColor,
			[nameof(IActivityIndicator.IsRunning)] = MapIsRunning,
#if __ANDROID__
			// Android does not have the concept of IsRunning, so we are leveraging the Visibility
			[nameof(IActivityIndicator.Visibility)] = MapIsRunning,
#endif
		};

		public ActivityIndicatorHandler() : base(ActivityIndicatorMapper)
		{

		}

		public ActivityIndicatorHandler(IPropertyMapper mapper) : base(mapper ?? ActivityIndicatorMapper)
		{

		}
	}
}