namespace Microsoft.Maui.Handlers;

internal partial class MaterialProgressBarHandler : ViewHandler<IProgress, MaterialProgressBar>
{
	public static PropertyMapper<IProgress, MaterialProgressBarHandler> Mapper =
		new(ViewMapper)
		{
			[nameof(IProgress.Progress)] = MapProgress,
			[nameof(IProgress.ProgressColor)] = MapProgressColor,
		};

	public static CommandMapper<IProgress, MaterialProgressBarHandler> CommandMapper =
		new(ViewCommandMapper);

	public MaterialProgressBarHandler() : base(Mapper, CommandMapper)
	{
	}

	protected override MaterialProgressBar CreatePlatformView()
	{
		return new MaterialProgressBar(Context)
		{
			Indeterminate = false,
			Max = ProgressBarExtensions.Maximum
		};
	}

	protected override void ConnectHandler(MaterialProgressBar platformView)
	{
		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(MaterialProgressBar platformView)
	{
		base.DisconnectHandler(platformView);
	}

	public static void MapProgress(MaterialProgressBarHandler handler, IProgress progress)
	{
		handler.PlatformView?.UpdateProgress(progress);
	}

	public static void MapProgressColor(MaterialProgressBarHandler handler, IProgress progress)
	{
		handler.PlatformView?.UpdateProgressColor(progress);
	}
}