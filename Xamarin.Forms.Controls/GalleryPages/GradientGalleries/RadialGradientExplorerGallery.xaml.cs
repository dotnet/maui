using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Forms.Controls.GalleryPages.GradientGalleries
{
	public partial class RadialGradientExplorerGallery : ContentPage
	{
		const uint AnimationSpeed = 200;

		Point _center;
		int _offsets;
		GradientStopCollection _gradientStops;
		Layout _layout;

		public RadialGradientExplorerGallery()
		{
			InitializeComponent();
			BindingContext = this;

			_gradientStops = new GradientStopCollection();
			BindableLayout.SetItemsSource(GradientsLayout, _gradientStops);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			GradientColorPicker.ColorSelected += GradientColorPickerColorSelected;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			GradientColorPicker.ColorSelected -= GradientColorPickerColorSelected;
		}

		void OnBackgroundColorChanged(object sender, TextChangedEventArgs e)
		{
			var backgroundColor = GetColorFromString(e.NewTextValue);

			if (backgroundColor != Color.Default)
			{
				GradientView.BackgroundColor = backgroundColor;
				BackgroundColorEntry.BackgroundColor = backgroundColor;
				BackgroundColorFrame.BackgroundColor = backgroundColor;
			}
		}

		void OnBorderColorChanged(object sender, TextChangedEventArgs e)
		{
			var borderColor = GetColorFromString(e.NewTextValue);

			if (borderColor != Color.Default)
			{
				GradientView.BorderColor = borderColor;
				BorderColorEntry.BackgroundColor = borderColor;
				BorderColorFrame.BackgroundColor = borderColor;
			}
		}

		void OnColorPickerTapped(object sender, EventArgs e)
		{
			GradientColorPicker.FadeTo(1, AnimationSpeed, Easing.SinInOut);
			GradientColorPicker.TranslateTo(0, 0, AnimationSpeed, Easing.SinInOut);

			if (((Frame)sender).Parent is Layout<View> layout)
				_layout = layout;
		}

		void OnNewGradientAdded(object sender, EventArgs e)
		{
			_offsets++;
			_gradientStops.Add(new GradientStop());
			UpdateOffsets(_gradientStops, _offsets);
		}

		void OnNewGradientRemoved(object sender, EventArgs e)
		{
			if (_gradientStops.Count <= 0)
				return;

			_offsets--;
			_gradientStops.Remove(_gradientStops.Last());
			UpdateOffsets(_gradientStops, _offsets);
			UpdateBackground();
		}

		void OnGradientChanged(object sender, TextChangedEventArgs e)
		{
			if (!(sender is BindableObject bindable) || !(bindable.BindingContext is GradientStop gradientStop))
				return;

			gradientStop.Color = GetColorFromString(e.NewTextValue);
			UpdateBackground();
		}

		void OnCenterChanged(object sender, TextChangedEventArgs e)
		{
			_center = GetPointFromString(e.NewTextValue);
			UpdateBackground();
		}

		void GradientColorPickerColorSelected(object sender, ColorSource e)
		{
			GradientColorPicker.FadeTo(0, 0, Easing.SinInOut);
			GradientColorPicker.TranslateTo(0, 1000, 0, Easing.SinInOut);

			var selectedColor = GradientColorPicker.SelectedColorSource;

			if (selectedColor == null)
				return;

			if (!(_layout.Children.FirstOrDefault() is Entry entry))
				return;

			var red = (int)(selectedColor.Color.R * 255);
			var green = (int)(selectedColor.Color.G * 255);
			var blue = (int)(selectedColor.Color.B * 255);

			entry.Text = $"#{red:X2}{green:X2}{blue:X2}";
		}

		void OnCornerRadiusChanged(object sender, ValueChangedEventArgs e)
		{
			GradientView.CornerRadius = (float)e.NewValue;
		}

		void OnShadowCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			GradientView.HasShadow = e.Value;
		}

		void OnVisualCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
				GradientView.Visual = VisualMarker.Material;
			else
				GradientView.Visual = VisualMarker.Default;

			var gradientView = GradientView;
			var parentView = (Grid)GradientView.Parent;
			parentView.Children.Remove(gradientView);
			Device.BeginInvokeOnMainThread(() => parentView.Children.Add(gradientView));
		}

		void UpdateOffsets(IEnumerable<GradientStop> gradientStops, int offsets)
		{
			var offset = 0f;
			var delta = 1f / (offsets - 1);

			foreach (var gradientStop in gradientStops)
			{
				gradientStop.Offset = offset;
				offset += delta;
			}
		}

		void UpdateBackground()
		{
			if (_center == null || _gradientStops == null)
				return;

			var radialGradient = new RadialGradientBrush
			{
				Center = _center,
				Radius = 1d,
				GradientStops = _gradientStops
			};

			if (radialGradient.IsEmpty)
				return;

			GradientView.Background = radialGradient;
		}

		Point GetPointFromString(string value)
		{
			if (string.IsNullOrEmpty(value))
				return new Point();

			try
			{
				var parts = value.Split(',');

				return new Point(Convert.ToDouble(parts[0]), Convert.ToDouble(parts[1]));
			}
			catch (Exception)
			{
				return new Point();
			}
		}

		Color GetColorFromString(string value)
		{
			if (string.IsNullOrEmpty(value))
				return Color.Default;

			try
			{
				return Color.FromHex(value[0].Equals('#') ? value : $"#{value}");
			}
			catch (Exception)
			{
				return Color.Default;
			}
		}
	}
}