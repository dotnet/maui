using Xamarin.Platform;

namespace Xamarin.Forms
{
	public partial class Slider : ISlider
	{
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
