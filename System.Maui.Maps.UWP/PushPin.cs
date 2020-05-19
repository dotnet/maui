using System;
using System.ComponentModel;
using global::Windows.Devices.Geolocation;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls;
using global::Windows.UI.Xaml.Controls.Maps;
using global::Windows.UI.Xaml.Input;

namespace System.Maui.Maps.UWP
{
	internal class PushPin : ContentControl
	{
		readonly Pin _pin;

		internal PushPin(Pin pin)
		{
			if (pin == null)
				throw new ArgumentNullException();

			ContentTemplate = global::Windows.UI.Xaml.Application.Current.Resources["PushPinTemplate"] as global::Windows.UI.Xaml.DataTemplate;
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
#pragma warning disable CS0618
			_pin.SendTap();
#pragma warning restore CS0618
			_pin.SendMarkerClick();
		}

		void UpdateLocation()
		{
			var anchor = new global::Windows.Foundation.Point(0.65, 1);
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