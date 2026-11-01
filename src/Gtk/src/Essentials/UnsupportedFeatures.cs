using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// Essentials APIs that have no meaningful Linux desktop implementation. Registering them (instead of leaving the default)
/// makes every call fail with a clear <see cref="FeatureNotSupportedException"/> rather than a "not implemented in the
/// reference assembly" error, and lets <c>IsSupported</c> report <see langword="false"/>.
/// </summary>
static class Unsupported
{
	internal static FeatureNotSupportedException Exception(string feature) =>
		new($"{feature} is not supported on Linux/GTK.");
}

sealed class UnsupportedAccelerometer : IAccelerometer
{
	public event EventHandler<AccelerometerChangedEventArgs>? ReadingChanged { add { } remove { } }

	public event EventHandler? ShakeDetected { add { } remove { } }

	public bool IsSupported => false;

	public bool IsMonitoring => false;

	public void Start(SensorSpeed sensorSpeed) => throw Unsupported.Exception("Accelerometer");

	public void Stop()
	{
	}
}

sealed class UnsupportedBarometer : IBarometer
{
	public event EventHandler<BarometerChangedEventArgs>? ReadingChanged { add { } remove { } }

	public bool IsSupported => false;

	public bool IsMonitoring => false;

	public void Start(SensorSpeed sensorSpeed) => throw Unsupported.Exception("Barometer");

	public void Stop()
	{
	}
}

sealed class UnsupportedCompass : ICompass
{
	public event EventHandler<CompassChangedEventArgs> ReadingChanged { add { } remove { } }

	public bool IsSupported => false;

	public bool IsMonitoring => false;

	public void Start(SensorSpeed sensorSpeed) => throw Unsupported.Exception("Compass");

	public void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter) => throw Unsupported.Exception("Compass");

	public void Stop()
	{
	}
}

sealed class UnsupportedGyroscope : IGyroscope
{
	public event EventHandler<GyroscopeChangedEventArgs> ReadingChanged { add { } remove { } }

	public bool IsSupported => false;

	public bool IsMonitoring => false;

	public void Start(SensorSpeed sensorSpeed) => throw Unsupported.Exception("Gyroscope");

	public void Stop()
	{
	}
}

sealed class UnsupportedMagnetometer : IMagnetometer
{
	public event EventHandler<MagnetometerChangedEventArgs> ReadingChanged { add { } remove { } }

	public bool IsSupported => false;

	public bool IsMonitoring => false;

	public void Start(SensorSpeed sensorSpeed) => throw Unsupported.Exception("Magnetometer");

	public void Stop()
	{
	}
}

sealed class UnsupportedOrientationSensor : IOrientationSensor
{
	public event EventHandler<OrientationSensorChangedEventArgs> ReadingChanged { add { } remove { } }

	public bool IsSupported => false;

	public bool IsMonitoring => false;

	public void Start(SensorSpeed sensorSpeed) => throw Unsupported.Exception("OrientationSensor");

	public void Stop()
	{
	}
}

sealed class UnsupportedContacts : IContacts
{
	public Task<Contact?> PickContactAsync() => Task.FromException<Contact?>(Unsupported.Exception("Contacts"));

	public Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default) =>
		Task.FromException<IEnumerable<Contact>>(Unsupported.Exception("Contacts"));
}

sealed class UnsupportedFlashlight : IFlashlight
{
	public Task<bool> IsSupportedAsync() => Task.FromResult(false);

	public Task TurnOnAsync() => Task.FromException(Unsupported.Exception("Flashlight"));

	public Task TurnOffAsync() => Task.FromException(Unsupported.Exception("Flashlight"));
}

sealed class UnsupportedVibration : IVibration
{
	public bool IsSupported => false;

	public void Vibrate() => throw Unsupported.Exception("Vibration");

	public void Vibrate(TimeSpan duration) => throw Unsupported.Exception("Vibration");

	public void Cancel() => throw Unsupported.Exception("Vibration");
}

sealed class UnsupportedHapticFeedback : IHapticFeedback
{
	public bool IsSupported => false;

	public void Perform(HapticFeedbackType type) => throw Unsupported.Exception("HapticFeedback");
}

sealed class UnsupportedSms : ISms
{
	public bool IsComposeSupported => false;

	public Task ComposeAsync(SmsMessage? message) => Task.FromException(Unsupported.Exception("Sms"));
}

sealed class UnsupportedPhoneDialer : IPhoneDialer
{
	public bool IsSupported => false;

	public void Open(string number) => throw Unsupported.Exception("PhoneDialer");
}

