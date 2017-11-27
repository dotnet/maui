using System;
using System.ComponentModel;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;

namespace Xamarin.Forms.Maps.UWP
{
	internal class PushPin : ContentControl
	{
		readonly Pin _pin;

		internal PushPin(Pin pin)
		{
			if (pin == null)
				throw new ArgumentNullException();

			ContentTemplate = Windows.UI.Xaml.Application.Current.Resources["PushPinTemplate"] as Windows.UI.Xaml.DataTemplate;
			DataContext = Content = _pin = pin;

			UpdateLocation();

			Loaded += PushPinLoaded;
			Unloaded += PushPinUnloaded;
			Tapped += PushPinTapped;
		}

		void PushPinLoaded(object sender, RoutedEventArgs e)
		{
			_pin.PropertyChanged += PinPropertyChanged;
		}

		void PushPinUnloaded(object sender, RoutedEventArgs e)
		{
			_pin.PropertyChanged -= PinPropertyChanged;
			Tapped -= PushPinTapped;
		}

		void PinPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Pin.PositionProperty.PropertyName)
				UpdateLocation();
		}

		void PushPinTapped(object sender, TappedRoutedEventArgs e)
		{
			_pin.SendTap();
		}

		void UpdateLocation()
		{
			var anchor = new Windows.Foundation.Point(0.65, 1);
			var location = new Geopoint(new BasicGeoposition
			{
				Latitude = _pin.Position.Latitude,
				Longitude = _pin.Position.Longitude
			});
			MapControl.SetLocation(this, location);
			MapControl.SetNormalizedAnchorPoint(this, anchor);
		}
	}
}