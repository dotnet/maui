using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class SliderPage
	{
		ImageSource? _imageSource;

		public SliderPage()
		{
			InitializeComponent();

			UpdateInfo();
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

		void OnDynamicValueChanged(object sender, ValueChangedEventArgs args)
		{
			UpdateInfo();
		}

		void OnUpdateMinimumButtonClicked(object sender, System.EventArgs e)
		{
			DynamicSlider.Minimum = 4;
			UpdateInfo();
		}

		void OnUpdateMaximumButtonClicked(object sender, System.EventArgs e)
		{
			DynamicSlider.Maximum = 8;
			UpdateInfo();
		}

		void UpdateInfo()
		{
			DynamicInfoLabel.Text = $"Minimum: {DynamicSlider.Minimum}, Maximum: {DynamicSlider.Maximum}, Value: {DynamicSlider.Value}";
		}
	}
}
