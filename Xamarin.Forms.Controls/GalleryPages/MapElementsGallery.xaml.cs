using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapElementsGallery : ContentPage
	{
		enum SelectedElementType
		{
			Polyline,
			Polygon
		}

		SelectedElementType _selectedType;

		Polyline _polyline;
		Polygon _polygon;

		Random _random = new Random();

		public MapElementsGallery()
		{
			InitializeComponent();

			Map.MoveToRegion(
				MapSpan.FromCenterAndRadius(
					new Position(39.828152, -98.569817),
					Distance.FromMiles(1681)));

			_polyline = new Polyline
			{
				Geopath =
				{
					new Position(47.641944, -122.127222),
					new Position(37.411625, -122.071327),
					new Position(35.138901, -80.922623)
				}
			};

			_polygon = new Polygon
			{
				StrokeColor = Color.FromHex("#002868"),
				FillColor = Color.FromHex("#88BF0A30"),
				Geopath =
				{
					new Position(37, -102.05),
					new Position(37, -109.05),
					new Position(41, -109.05),
					new Position(41, -102.05)
				}
			};

			Map.MapElements.Add(_polyline);
			Map.MapElements.Add(_polygon);

			ElementPicker.SelectedIndex = 0;
		}

		void MapClicked(object sender, MapClickedEventArgs e)
		{
			switch (_selectedType)
			{
				case SelectedElementType.Polyline:
					_polyline.Geopath.Add(e.Position);
					break;
				case SelectedElementType.Polygon:
					_polygon.Geopath.Add(e.Position);
					break;
			}
		}

		void PickerSelectionChanged(object sender, EventArgs e)
		{
			Enum.TryParse((string)ElementPicker.SelectedItem, out _selectedType);
		}

		void AddClicked(object sender, EventArgs e)
		{
			switch (_selectedType)
			{
				case SelectedElementType.Polyline:
					Map.MapElements.Add(_polyline = new Polyline());
					break;
				case SelectedElementType.Polygon:
					Map.MapElements.Add(_polygon = new Polygon());
					break;
			}
		}

		void RemoveClicked(object sender, EventArgs e)
		{
			switch (_selectedType)
			{
				case SelectedElementType.Polyline:
					Map.MapElements.Remove(_polyline);
					_polyline = Map.MapElements.OfType<Polyline>().LastOrDefault();

					if (_polyline == null)
						Map.MapElements.Add(_polyline = new Polyline());

					break;
				case SelectedElementType.Polygon:
					Map.MapElements.Remove(_polygon);
					_polygon = Map.MapElements.OfType<Polygon>().LastOrDefault();

					if (_polygon == null)
						Map.MapElements.Add(_polygon = new Polygon());

					break;
			}
		}

		void ChangeColorClicked(object sender, EventArgs e)
		{
			var newColor = new Color(_random.NextDouble(), _random.NextDouble(), _random.NextDouble());
			switch (_selectedType)
			{
				case SelectedElementType.Polyline:
					_polyline.StrokeColor = newColor;
					break;
				case SelectedElementType.Polygon:
					_polygon.StrokeColor = newColor;
					break;
			}
		}

		void ChangeWidthClicked(object sender, EventArgs e)
		{
			var newWidth = _random.Next(1, 50);
			switch (_selectedType)
			{
				case SelectedElementType.Polyline:
					_polyline.StrokeWidth = newWidth;
					break;
				case SelectedElementType.Polygon:
					_polygon.StrokeWidth = newWidth;
					break;
			}
		}

		void ChangeFillClicked(object sender, EventArgs e)
		{
			var newColor = new Color(_random.NextDouble(), _random.NextDouble(), _random.NextDouble(), _random.NextDouble());
			switch (_selectedType)
			{
				case SelectedElementType.Polygon:
					_polygon.FillColor = newColor;
					break;
			}
		}
	}
}