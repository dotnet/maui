namespace System.Maui.Platform
{
	public partial class ProgressBarRenderer
	{
		public static PropertyMapper<IProgress> ProgressMapper = new PropertyMapper<IProgress>(ViewRenderer.ViewMapper)
		{
			[nameof(IProgress.Progress)] = MapPropertyProgress,
			[nameof(IProgress.ProgressColor)] = MapPropertyProgressColor
		};

		public ProgressBarRenderer() : base(ProgressMapper)
		{

		}

		public ProgressBarRenderer(PropertyMapper mapper) : base(mapper ?? ProgressMapper)
		{

		}
	}
}
