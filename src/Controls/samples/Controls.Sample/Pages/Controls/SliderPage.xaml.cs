using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class SliderPage
	{
		ImageSource _imageSource;

		public SliderPage()
		{
			InitializeComponent();
		}

		void OnValueChanged(object sender, ValueChangedEventArgs args)
		{
			Debug.WriteLine($"Slider Value: {args.NewValue}");
		}

		private void ToggleImageSource(object sender, System.EventArgs e)
		{
			if (_imageSource is null)
			{
				_imageSource = ImageSlider.ThumbImageSource;
				ImageSlider.ThumbImageSource = null;
			}
			else
			{
				ImageSlider.ThumbImageSource = _imageSource;
				_imageSource = null;
			}
		}
	}
}