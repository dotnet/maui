using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class PickerPage
	{
		int _horizontalOptionsIndex;
		int _verticalOptionsIndex;

		public PickerPage()
		{
			InitializeComponent();

			UpdatePickerBackground();
			UpdatePickerBindingContext();

			BindingContext = this;

			UpdatePickerBackground();

			Loaded += (s, e) =>
			{
				DynamicItemsPicker.Items.Add("Item 1");
				DynamicItemsPicker.Items.Add("Item 2");
				DynamicItemsPicker.Items.Add("Item 3");
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			IsOpenPicker.Opened += IsOpenPickerOpened;
			IsOpenPicker.Closed += IsOpenPickerClosed;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			IsOpenPicker.Opened -= IsOpenPickerOpened;
			IsOpenPicker.Closed -= IsOpenPickerClosed;
		}

		void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			string selectedCountry = (string)((Picker)sender).SelectedItem;
			DisplayAlertAsync("SelectedIndexChanged", selectedCountry, "Ok");
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

		void OnUpdateHorizontalOptionsClicked(object sender, EventArgs e)
		{
			AlignmentPicker.HorizontalOptions = GetLayoutOptions(_horizontalOptionsIndex);
			UpdateIndex(ref _horizontalOptionsIndex);
		}

		void OnUpdateVerticalOptionsClicked(object sender, EventArgs e)
		{
			AlignmentPicker.VerticalOptions = GetLayoutOptions(_verticalOptionsIndex);
			UpdateIndex(ref _verticalOptionsIndex);
		}

		void UpdateIndex(ref int index)
		{
			index++;
			if (index == 4)
				index = 0;
		}

		LayoutOptions GetLayoutOptions(int index)
		{
			switch (index)
			{
				default:
				case 0:
					return LayoutOptions.Start;
				case 1:
					return LayoutOptions.Center;
				case 2:
					return LayoutOptions.End;
				case 3:
					return LayoutOptions.Fill;
			}
		}

		void OnOpenClicked(object sender, EventArgs e)
		{
			IsOpenPicker.IsOpen = true;
		}

		void OnCloseClicked(object sender, EventArgs e)
		{
			IsOpenPicker.IsOpen = false;
		}

		void IsOpenPickerOpened(object? sender, PickerOpenedEventArgs e)
		{
			Console.WriteLine("IsOpenPicker Opened");
		}

		void IsOpenPickerClosed(object? sender, PickerClosedEventArgs e)
		{
			Console.WriteLine("IsOpenPicker Closed");
		}
	}

	public class PickerData
	{
		public string? Name { get; set; }

		public ObservableCollection<PickerData>? PickerItems { get; set; }
	}
}