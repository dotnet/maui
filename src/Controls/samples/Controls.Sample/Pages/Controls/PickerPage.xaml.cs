using System;
using System.Collections.ObjectModel;
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
			UpdatePickerBindingContext();

			this.BindingContext = this;

			Loaded += (s, e) =>
			{
				DynamicItemsPicker.Items.Add("Item 1");
				DynamicItemsPicker.Items.Add("Item 2");
				DynamicItemsPicker.Items.Add("Item 3");
			};
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

		void OnClearItemsButtonClicked(object sender, EventArgs e)
		{
			DynamicItemsPicker.Items.Clear();
		}

		void OnAddItemsButtonClicked(object sender, EventArgs e)
		{
			DynamicItemsPicker.Items.Add("New Item 1");
			DynamicItemsPicker.Items.Add("New Item 2");
			DynamicItemsPicker.Items.Add("New Item 3");
		}

		void OnReplaceItemsButtonClicked(object sender, EventArgs e)
		{
			DynamicItemsPicker.Items.Clear();

			DynamicItemsPicker.Items.Add("Item One");
			DynamicItemsPicker.Items.Add("Item Two");
			DynamicItemsPicker.Items.Add("Item Three");
		}

		void OnSetBindingContextClicked(object sender, EventArgs e)
		{
			UpdatePickerBindingContext();
		}

		void OnRemoveBindingContextClicked(object sender, EventArgs e)
		{
			BindingContextLayout.BindingContext = null;
		}

		void UpdatePickerBindingContext()
		{
			var random = new Random();

			BindingContextLayout.BindingContext = new PickerData
			{
				Name = $"Item {random.Next(0, 100)}",
				PickerItems = new ObservableCollection<PickerData>
				{
					new PickerData { Name = $"Item {random.Next(0, 100)}" },
					new PickerData { Name = $"Item {random.Next(0, 100)}" },
				}
			};
		}
	}

	public class PickerData
	{
		public string Name { get; set; }

		public ObservableCollection<PickerData> PickerItems { get; set; }
	}
}