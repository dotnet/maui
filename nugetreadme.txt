## Xamarin.Essentials

Thank you for installing Xamarin.Essentials, be sure to read through our full documentation at:
http://aka.ms/xamarinessentials

## Setup

Ensure that you install Xamarin.Essentials into all of your projects.

For Android projects there is a small amount of setup needed to handle permissions. Please follow our short guide at:
http://aka.ms/essentials-getstarted


## Changes for 0.9.0-preview

If you are upgrading from an earlier version there are a few changes to the API that may affect your code:

* SensorSpeed.Ui is now SensorSpeed.UI
* BatteryPowerSource.Ac is now BatteryPowerSource.AC
* Change to generic EventHandlers for Accelerometer, Battery, Compass, Connectivity, Display Metrics, Magnetometer, OrientationSensor, Gyroscope, and Power.

You may have registered an event hander for one of these:

Gyroscope.ReadingChanged += OnReadingChanged;

and then implemented the handler:

void OnReadingChanged(GyroscopeChangedEventArgs e)
{
}

Instead of using a custom EventHander we now use EventHandler<T> which always pass in an object as the first parameter:

void OnReadingChanged(object sender, GyroscopeChangedEventArgs e)
{
}


See our full release notes: http://aka.ms/essentials-releasenotes


## Feedback, Issues, & Feature Requests

We would love to hear from you simply head to: http://aka.ms/essentialsfeedback
