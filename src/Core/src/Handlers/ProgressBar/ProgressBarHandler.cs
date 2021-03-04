namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler
	{
		public static PropertyMapper<IProgress, ProgressBarHandler> ProgressMapper = new PropertyMapper<IProgress, ProgressBarHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IProgress.Progress)] = MapProgress,
			[nameof(IProgress.ProgressColor)] = MapProgressColor
		};

		public static void MapProgress(ProgressBarHandler handler, IProgress progress)
		{
			handler.TypedNativeView?.UpdateProgress(progress);
		}

		public static void MapProgressColor(ProgressBarHandler handler, IProgress progress)
		{
			handler.TypedNativeView?.UpdateProgressColor(progress);
		}

		public ProgressBarHandler() : base(ProgressMapper)
		{

		}

		public ProgressBarHandler(PropertyMapper mapper) : base(mapper ?? ProgressMapper)
		{

		}
	}
}