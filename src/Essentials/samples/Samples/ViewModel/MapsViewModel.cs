using System;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;

namespace Samples.ViewModel
{
	public class MapsViewModel : BaseViewModel
	{
		string name = "Microsoft Building 25";
		string longitude = (-122.130603).ToString();
		string latitude = 47.645160.ToString();
		string locality = "Redmond";
		string adminArea = "WA";
		string thoroughfare = "Microsoft Building 25";
		string country = "United States";
		string zipCode = "98052";
		int navigationMode;

		public string Name
		{
			get => name;
			set => SetProperty(ref name, value);
		}

		public string Longitude
		{
			get => longitude;
			set => SetProperty(ref longitude, value);
		}

		public string Latitude
		{
			get => latitude;
			set => SetProperty(ref latitude, value);
		}

		public string Locality
		{
			get => locality;
			set => SetProperty(ref locality, value);
		}

		public string AdminArea
		{
			get => adminArea;
			set => SetProperty(ref adminArea, value);
		}

		public string Thoroughfare
		{
			get => thoroughfare;
			set => SetProperty(ref thoroughfare, value);
		}

		public string Country
		{
			get => country;
			set => SetProperty(ref country, value);
		}

		public string ZipCode
		{
			get => zipCode;
			set => SetProperty(ref zipCode, value);
		}

		public string[] NavigationModes { get; } =
			Enum.GetNames(typeof(NavigationMode));

		public int NavigationMode
		{
			get => navigationMode;
			set => SetProperty(ref navigationMode, value);
		}

		public ICommand MapsCommand { get; }

		public ICommand LaunchPlacemarkCommand { get; }

		public MapsViewModel()
		{
			MapsCommand = new Command(OpenLocation);
			LaunchPlacemarkCommand = new Command(OpenPlacemark);
		}

		async void OpenLocation()
		{
			var canOpen = await Map.TryOpenAsync(
				double.Parse(Latitude),
				double.Parse(Longitude),
				new MapLaunchOptions
				{
					Name = Name,
					NavigationMode = (NavigationMode)NavigationMode
				});

			if (!canOpen)
			{
				await DisplayAlertAsync("Unable to open map, possibly due to the fact that there is no default maps app installed.");
			}
		}

		async void OpenPlacemark()
		{
			var placemark = new Placemark
			{
				Locality = Locality,
				AdminArea = AdminArea,
				CountryName = Country,
				Thoroughfare = Thoroughfare,
				PostalCode = ZipCode
			};

			var canOpen = await Map.TryOpenAsync(placemark, new MapLaunchOptions
			{
				Name = Name,
				NavigationMode = (NavigationMode)NavigationMode
			});

			if (!canOpen)
			{
				await DisplayAlertAsync("Unable to open map, possibly due to the fact that there is no default maps app installed.");
			}
		}
	}
}
