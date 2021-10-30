namespace Microsoft.Maui.Controls
{
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
	}
}
