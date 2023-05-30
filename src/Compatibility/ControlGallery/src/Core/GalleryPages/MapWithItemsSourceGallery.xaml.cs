using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapWithItemsSourceGallery : ContentPage
	{
		static readonly Location startPosition = new Location(39.8283459, -98.5794797);

		public MapWithItemsSourceGallery()
		{
			InitializeComponent();
			BindingContext = new ViewModel();
			_map.MoveToRegion(MapSpan.FromCenterAndRadius(startPosition, Distance.FromMiles(1200)));
		}

		[Preserve(AllMembers = true)]
		class ViewModel
		{
			int _pinCreatedCount = 0;

			readonly ObservableCollection<Place> _places;

			public IEnumerable Places => _places;

			public ICommand AddPlaceCommand { get; }
			public ICommand RemovePlaceCommand { get; }
			public ICommand ClearPlacesCommand { get; }
			public ICommand UpdatePlacesCommand { get; }
			public ICommand ReplacePlaceCommand { get; }

			public ViewModel()
			{
				_places = new ObservableCollection<Place>() {
					new Place("New York, USA", "The City That Never Sleeps", new Location(40.67, -73.94)),
					new Place("Los Angeles, USA", "City of Angels", new Location(34.11, -118.41)),
					new Place("San Francisco, USA", "Bay City ", new Location(37.77, -122.45))
				};

				AddPlaceCommand = new Command(AddPlace);
				RemovePlaceCommand = new Command(RemovePlace);
				ClearPlacesCommand = new Command(() => _places.Clear());
				UpdatePlacesCommand = new Command(UpdatePlaces);
				ReplacePlaceCommand = new Command(ReplacePlace);
			}

			async void AddPlace()
			{
				await Task.Run(() =>
				{
					_places.Add(NewPlace());
				});
			}

			async void RemovePlace()
			{
				await Task.Run(() =>
				{
					if (_places.Any())
					{
						_places.Remove(_places.First());
					}
				});
			}

			async void UpdatePlaces()
			{
				await Task.Run(() =>
				{
					if (!_places.Any())
					{
						return;
					}

					double lastLatitude = _places.Last().Position.Latitude;

					foreach (Place place in Places)
					{
						place.Position = new Location(lastLatitude, place.Position.Longitude);
					}
				});
			}

			async void ReplacePlace()
			{
				await Task.Run(() =>
				{
					if (!_places.Any())
					{
						return;
					}

					_places[_places.Count - 1] = NewPlace();
				});
			}

			static class RandomPosition
			{
				static Random Random = new Random(Environment.TickCount);

				public static Location Next(Location position, double latitudeRange, double longitudeRange)
				{
					return new Location(
						latitude: position.Latitude + (Random.NextDouble() * 2 - 1) * latitudeRange,
						longitude: position.Longitude + (Random.NextDouble() * 2 - 1) * longitudeRange);
				}
			}

			Place NewPlace()
			{
				++_pinCreatedCount;

				return new Place(
					$"Pin {_pinCreatedCount}",
					$"Desc {_pinCreatedCount}",
				 RandomPosition.Next(startPosition, 8, 19));
			}
		}

		[Preserve(AllMembers = true)]
		class Place : INotifyPropertyChanged
		{
			Location _position;

			public event PropertyChangedEventHandler PropertyChanged;

			public string Address { get; }

			public string Description { get; }

			public Location Position
			{
				get => _position;
				set
				{
					if (!_position.Equals(value))
					{
						_position = value;
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
					}
				}
			}

			public Place(string address, string description, Location position)
			{
				Address = address;
				Description = description;
				Position = position;
			}
		}
	}

	class MapItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate DataTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			return DataTemplate;
		}
	}
}