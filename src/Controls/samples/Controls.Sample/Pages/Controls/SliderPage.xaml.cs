using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class SliderPage
	{
		public SliderPage()
		{
			InitializeComponent();

			UpdateInfo();
		}

		void OnValueChanged(object sender, ValueChangedEventArgs args)
		{
			Debug.WriteLine($"Slider Value: {args.NewValue}");
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