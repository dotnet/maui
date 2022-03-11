using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class SliderPage
	{
		public SliderPage()
		{
			InitializeComponent();
		}

		void OnValueChanged(object sender, ValueChangedEventArgs args)
		{
			Debug.WriteLine($"Slider Value: {args.NewValue}");
		}
	}
}