using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class PickerPage
	{
		public PickerPage()
		{
			InitializeComponent();

			UpdatePickerBackground();

			this.BindingContext = this;
		}

		void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			string selectedCountry = (string)((Picker)sender).SelectedItem;
			DisplayAlert("SelectedIndexChanged", selectedCountry, "Ok");
		}

		public string[] PickerItems { get; } =
		{
			"Item 1",
			"Item 2",
			"Item 3"
		};

		public string[] MorePickerItems { get; } = Enumerable.Range(1, 20).Select(i => $"Item {i}").ToArray();

		void OnUpdateBackgroundButtonClicked(object sender, System.EventArgs e)
		{
			UpdatePickerBackground();
		}

		void OnClearBackgroundButtonClicked(object sender, System.EventArgs e)
		{
			BackgroundPicker.Background = null;
		}

		void UpdatePickerBackground()
		{
			Random rnd = new Random();
			Color startColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			Color endColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);

			BackgroundPicker.Background = new LinearGradientBrush
			{
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
 				{
 					new GradientStop { Color = startColor },
 					new GradientStop { Color = endColor, Offset = 1 }
 				}
			};
		}
	}
}