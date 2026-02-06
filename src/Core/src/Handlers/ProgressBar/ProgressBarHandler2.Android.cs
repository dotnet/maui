namespace Microsoft.Maui.Handlers;

// TODO: Material3 - make it public in .net 11
internal class ProgressBarHandler2 : ViewHandler<IProgress, MaterialProgressBar>
{
	public static PropertyMapper<IProgress, ProgressBarHandler2> Mapper =
		new(ViewMapper)
		{
			[nameof(IProgress.Progress)] = MapProgress,
			[nameof(IProgress.ProgressColor)] = MapProgressColor,
		};

	public static CommandMapper<IProgress, ProgressBarHandler2> CommandMapper =
		new(ViewCommandMapper);

	public ProgressBarHandler2() : base(Mapper, CommandMapper)
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

	public static void MapProgress(ProgressBarHandler2 handler, IProgress progress)
	{
		handler.PlatformView?.UpdateProgress(progress);
	}

	public static void MapProgressColor(ProgressBarHandler2 handler, IProgress progress)
	{
		handler.PlatformView?.UpdateProgressColor(progress);
	}
}