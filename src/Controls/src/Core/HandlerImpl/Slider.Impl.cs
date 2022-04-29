using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Slider.xml" path="Type[@FullName='Microsoft.Maui.Controls.Slider']/Docs" />
	public partial class Slider : ISlider
	{
		IImageSource ISlider.ThumbImageSource => ThumbImageSource;

		void ISlider.DragCompleted()
		{
			(this as ISliderController).SendDragCompleted();
		}

		void ISlider.DragStarted()
		{
			(this as ISliderController).SendDragStarted();
		}

		Color ISlider.MaximumTrackColor
		{
			get => MaximumTrackColor ??
				DefaultStyles.GetColor(this, MaximumTrackColorProperty)?.Value as Color;
		}

		Color ISlider.MinimumTrackColor
		{
			get => MinimumTrackColor ??
				DefaultStyles.GetColor(this, MinimumTrackColorProperty)?.Value as Color;
		}

		Color ISlider.ThumbColor
		{
			get => ThumbColor ??
				DefaultStyles.GetColor(this, ThumbColorProperty)?.Value as Color;
		}
	}
}
