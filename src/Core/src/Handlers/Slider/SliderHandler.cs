#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler
	{
		public static PropertyMapper<ISlider, SliderHandler> SliderMapper = new PropertyMapper<ISlider, SliderHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISlider.Maximum)] = MapMaximum,
			[nameof(ISlider.MaximumTrackColor)] = MapMaximumTrackColor,
			[nameof(ISlider.Minimum)] = MapMinimum,
			[nameof(ISlider.MinimumTrackColor)] = MapMinimumTrackColor,
			[nameof(ISlider.ThumbColor)] = MapThumbColor,
			[nameof(ISlider.Value)] = MapValue,
		};

		public SliderHandler() : base(SliderMapper)
		{

		}

		public SliderHandler(PropertyMapper? mapper = null) : base(mapper ?? SliderMapper)
		{

		}
	}
}