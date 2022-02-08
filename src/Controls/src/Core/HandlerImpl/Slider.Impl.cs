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
	}
}
