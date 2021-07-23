using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace Samples.ViewModel
{
	public class MapsViewModel : BaseViewModel
	{
		string name = "Microsoft Building 25";

		public string Name
		{
			get => name;
			set => SetProperty(ref name, value);
		}

		string longitude = (-122.130603).ToString();

		public string Longitude
		{
			get => longitude;
			set => SetProperty(ref longitude, value);
		}

		string latitude = 47.645160.ToString();

		public string Latitude
		{
			get => latitude;
			set => SetProperty(ref latitude, value);
		}

		string locality = "Redmond";

		public string Locality
		{
			get => locality;
			set => SetProperty(ref locality, value);
		}

		string adminArea = "WA";

		public string AdminArea
		{
			get => adminArea;
			set => SetProperty(ref adminArea, value);
		}

		string thoroughfare = "Microsoft Building 25";

		public string Thoroughfare
		{
			get => thoroughfare;
			set => SetProperty(ref thoroughfare, value);
		}

		string country = "United States";

		public string Country
		{
			get => country;
			set => SetProperty(ref country, value);
		}

		string zipCode = "98052";

		public string ZipCode
		{
			get => zipCode;
			set => SetProperty(ref zipCode, value);
		}

		public string[] NavigationModes { get; } =
		   Enum.GetNames(typeof(NavigationMode));

		int navigationMode;

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
			await Map.OpenAsync(double.Parse(Latitude), double.Parse(Longitude), new MapLaunchOptions
			{
				Name = Name,
				NavigationMode = (NavigationMode)NavigationMode
			});
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
			await Map.OpenAsync(placemark, new MapLaunchOptions
			{
				Name = Name,
				NavigationMode = (NavigationMode)NavigationMode
			});
		}
	}
}
