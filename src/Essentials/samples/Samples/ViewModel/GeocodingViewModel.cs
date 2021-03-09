using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
	public class GeocodingViewModel : BaseViewModel
	{
		string lat = 47.67398.ToString();
		string lon = (-122.121513).ToString();
		string address = "Microsoft Building 25 Redmond WA";
		string geocodeAddress;
		string geocodePosition;

		public GeocodingViewModel()
		{
			GetAddressCommand = new Command(OnGetAddress);
			GetPositionCommand = new Command(OnGetPosition);
		}

		public ICommand GetAddressCommand { get; }

		public ICommand GetPositionCommand { get; }

		public string Latitude
		{
			get => lat;
			set => SetProperty(ref lat, value);
		}

		public string Longitude
		{
			get => lon;
			set => SetProperty(ref lon, value);
		}

		public string GeocodeAddress
		{
			get => geocodeAddress;
			set => SetProperty(ref geocodeAddress, value);
		}

		public string Address
		{
			get => address;
			set => SetProperty(ref address, value);
		}

		public string GeocodePosition
		{
			get => geocodePosition;
			set => SetProperty(ref geocodePosition, value);
		}

		async void OnGetPosition()
		{
			if (IsBusy)
				return;

			IsBusy = true;
			try
			{
				var locations = await Geocoding.GetLocationsAsync(Address);
				var location = locations?.FirstOrDefault();
				if (location == null)
				{
					GeocodePosition = "Unable to detect locations";
				}
				else
				{
					GeocodePosition =
						$"{nameof(location.Latitude)}: {location.Latitude}\n" +
						$"{nameof(location.Longitude)}: {location.Longitude}\n";
				}
			}
			catch (Exception ex)
			{
				GeocodePosition = $"Unable to detect locations: {ex.Message}";
			}
			finally
			{
				IsBusy = false;
			}
		}

		async void OnGetAddress()
		{
			if (IsBusy)
				return;

			IsBusy = true;
			try
			{
				double.TryParse(lat, out var lt);
				double.TryParse(lon, out var ln);

				var placemarks = await Geocoding.GetPlacemarksAsync(lt, ln);
				var placemark = placemarks?.FirstOrDefault();
				if (placemark == null)
				{
					GeocodeAddress = "Unable to detect placemarks.";
				}
				else
				{
					GeocodeAddress =
						$"{nameof(placemark.AdminArea)}: {placemark.AdminArea}\n" +
						$"{nameof(placemark.CountryCode)}: {placemark.CountryCode}\n" +
						$"{nameof(placemark.CountryName)}: {placemark.CountryName}\n" +
						$"{nameof(placemark.FeatureName)}: {placemark.FeatureName}\n" +
						$"{nameof(placemark.Locality)}: {placemark.Locality}\n" +
						$"{nameof(placemark.PostalCode)}: {placemark.PostalCode}\n" +
						$"{nameof(placemark.SubAdminArea)}: {placemark.SubAdminArea}\n" +
						$"{nameof(placemark.SubLocality)}: {placemark.SubLocality}\n" +
						$"{nameof(placemark.SubThoroughfare)}: {placemark.SubThoroughfare}\n" +
						$"{nameof(placemark.Thoroughfare)}: {placemark.Thoroughfare}\n";
				}
			}
			catch (Exception ex)
			{
				GeocodeAddress = $"Unable to detect placemarks: {ex.Message}";
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}
