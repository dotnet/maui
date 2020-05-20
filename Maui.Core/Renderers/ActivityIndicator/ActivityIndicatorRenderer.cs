namespace System.Maui.Platform
{
	public partial class ActivityIndicatorRenderer
	{
		public static PropertyMapper<IActivityIndicator> ActivityIndicatorMapper = new PropertyMapper<IActivityIndicator>(ViewRenderer.ViewMapper)
		{
			[nameof(IActivityIndicator.Color)] = MapPropertyColor,
			[nameof(IActivityIndicator.IsRunning)] = MapPropertyIsRunning
		};

		public ActivityIndicatorRenderer() : base(ActivityIndicatorMapper)
		{

		}

		public ActivityIndicatorRenderer(PropertyMapper mapper) : base(mapper ?? ActivityIndicatorMapper)
		{

		}
	}
}