using Microsoft.Caboodle;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    public class GeocodingViewModel : BaseViewModel
    {
        public GeocodingViewModel()
        {
            //Set UWP Map Key
            Geocoding.MapKey = "RJHqIE53Onrqons5CNOx~FrDr3XhjDTyEXEjng-CRoA~Aj69MhNManYUKxo6QcwZ0wmXBtyva0zwuHB04rFYAPf7qqGJ5cHb03RCDw1jIW8l";
            GetAddressCommand = new Command(async () =>
            {
                if (IsBusy)
                    return;

                IsBusy = true;
                try
                {
                    double.TryParse(lat, out var lt);
                    double.TryParse(lon, out var ln);

                    var a = await Geocoding.GetPlacemarksAsync(lt, ln);
                    var a1 = a?.FirstOrDefault();
                    if (a1 == null)
                    {
                        GeocodeAddress = "Unable to detect placemarks";
                    }
                    else
                    {
                        GeocodeAddress = $"{nameof(a1.AdminArea)}: {a1.AdminArea}\n" +
                                    $"{nameof(a1.CountryCode)}: {a1.CountryCode}\n" +
                                    $"{nameof(a1.CountryName)}: {a1.CountryName}\n" +
                                    $"{nameof(a1.FeatureName)}: {a1.FeatureName}\n" +
                                    $"{nameof(a1.Locality)}: {a1.Locality}\n" +
                                    $"{nameof(a1.PostalCode)}: {a1.PostalCode}\n" +
                                    $"{nameof(a1.SubAdminArea)}: {a1.SubAdminArea}\n" +
                                    $"{nameof(a1.SubLocality)}: {a1.SubLocality}\n" +
                                    $"{nameof(a1.SubThoroughfare)}: {a1.SubThoroughfare}\n" +
                                    $"{nameof(a1.Thoroughfare)}: {a1.Thoroughfare}\n";
                    }
                }
                catch (Exception)
                {
                    GeocodeAddress = "Unable to detect placemarks";
                }
                finally
                {
                    IsBusy = false;
                }
            });

            GetPositionCommand = new Command(async () =>
            {
                if (IsBusy)
                    return;

                IsBusy = true;
                try
                {

                    var a = await Geocoding.GetLocationsAsync(Address);
                    var a1 = a?.FirstOrDefault();
                    if (a1 == null)
                    {
                        GeocodePosition = "Unable to detect locations";
                    }
                    else
                    {
                        GeocodePosition = $"{nameof(a1.Latitude)}: {a1.Latitude}\n" +
                                    $"{nameof(a1.Longitude)}: {a1.Longitude}\n";
                    }
                }
                catch (Exception)
                {
                    GeocodePosition = "Unable to detect locations";
                }
                finally
                {
                    IsBusy = false;
                }
            });
        }
        public ICommand GetAddressCommand { get; }
        public ICommand GetPositionCommand { get; }
        string lat = "47.673988";
        string lon = "-122.121513";
        string address = "Microsoft Building 25 Redmond WA USA";
        string geocodeAddress;
        string geocodePosition;

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




    }
}
