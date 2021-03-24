namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler
	{
		public static PropertyMapper<ISlider, SliderHandler> SliderMapper = new PropertyMapper<ISlider, SliderHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISlider.Minimum)] = MapMinimum,
			[nameof(ISlider.Maximum)] = MapMaximum,
			[nameof(ISlider.Value)] = MapValue,
			[nameof(ISlider.MinimumTrackColor)] = MapMinimumTrackColor,
			[nameof(ISlider.MaximumTrackColor)] = MapMaximumTrackColor,
			[nameof(ISlider.ThumbColor)] = MapThumbColor
		};

		public SliderHandler() : base(SliderMapper)
		{

		}

		public SliderHandler(PropertyMapper? mapper = null) : base(mapper ?? SliderMapper)
		{

		}
	}
}