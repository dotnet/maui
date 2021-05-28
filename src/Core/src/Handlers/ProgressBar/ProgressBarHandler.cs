#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler
	{
		public static PropertyMapper<IProgress, ProgressBarHandler> ProgressMapper = new PropertyMapper<IProgress, ProgressBarHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IProgress.Progress)] = MapProgress,
		};

		public ProgressBarHandler() : base(ProgressMapper)
		{

		}

		public ProgressBarHandler(PropertyMapper? mapper = null) : base(mapper ?? ProgressMapper)
		{

		}
	}
}