namespace System.Maui.Platform
{
	public partial class SliderRenderer
	{
		public static PropertyMapper<ISlider> SliderMapper = new PropertyMapper<ISlider>(ViewRenderer.ViewMapper)
		{
			[nameof(ISlider.Minimum)] = MapPropertyMinimum,
			[nameof(ISlider.Maximum)] = MapPropertyMaximum,
			[nameof(ISlider.Value)] = MapPropertyValue,
			[nameof(ISlider.MinimumTrackColor)] = MapPropertyMinimumTrackColor,
			[nameof(ISlider.MaximumTrackColor)] = MapPropertyMaximumTrackColor,
			[nameof(ISlider.ThumbColor)] = MapPropertyThumbColor
		};

		public SliderRenderer() : base(SliderMapper)
		{

		}

		public SliderRenderer(PropertyMapper mapper) : base(mapper ?? SliderMapper)
		{

		}
	}
}
