#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler
	{
		public static IPropertyMapper<IProgress, ProgressBarHandler> ProgressMapper = new PropertyMapper<IProgress, ProgressBarHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IProgress.Progress)] = MapProgress,
			[nameof(IProgress.ProgressColor)] = MapProgressColor
		};

		public ProgressBarHandler() : base(ProgressMapper)
		{

		}

		public ProgressBarHandler(IPropertyMapper? mapper = null) : base(mapper ?? ProgressMapper)
		{

		}
	}
}