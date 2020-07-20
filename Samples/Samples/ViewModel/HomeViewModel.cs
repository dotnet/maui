using System.Collections.Generic;
using System.Linq;
using Samples.Model;
using Samples.View;
using Xamarin.Essentials;

namespace Samples.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        bool alreadyAppeared;
        SampleItem[] samples;
        IEnumerable<SampleItem> filteredItems;
        string filterText;

        public HomeViewModel()
        {
            alreadyAppeared = false;
            samples = new SampleItem[]
            {
                new SampleItem(
                    "📏",
                    "Accelerometer",
                    typeof(AccelerometerPage),
                    "Retrieve acceleration data of the device in 3D space.",
                    new[] { "accelerometer", "sensors", "hardware", "device" }),
                new SampleItem(
                    "📏",
                    "All Sensors",
                    typeof(AllSensorsPage),
                    "Have a look at the accelerometer, barometer, compass, gyroscope, magnetometer, and orientation sensors.",
                    new[] { "accelerometer", "barometer", "compass", "gyroscope", "magnetometer", "orientation", "sensors", "hardware", "device" }),
                new SampleItem(
                    "📦",
                    "App Info",
                    typeof(AppInfoPage),
                    "Find out about the app with ease.",
                    new[] { "app", "info" }),
                new SampleItem(
                    "📏",
                    "Barometer",
                    typeof(BarometerPage),
                    "Easily detect pressure level, via the device barometer.",
                    new[] { "barometer", "hardware", "device", "sensor" }),
                new SampleItem(
                    "🔋",
                    "Battery",
                    typeof(BatteryPage),
                    "Easily detect battery level, source, and state.",
                    new[] { "battery", "hardware", "device" }),
                new SampleItem(
                    "🌐",
                    "Browser",
                    typeof(BrowserPage),
                    "Quickly and easily open a browser to a specific website.",
                    new[] { "browser", "web", "internet" }),
                new SampleItem(
                    "📋",
                    "Clipboard",
                    typeof(ClipboardPage),
                    "Quickly and easily use the clipboard.",
                    new[] { "clipboard", "copy", "paste" }),
                new SampleItem(
                    "🎨",
                    "Color Converters",
                    typeof(ColorConvertersPage),
                    "Convert and adjust colors.",
                    new[] { "color", "drawing", "style" }),
                new SampleItem(
                    "🧭",
                    "Compass",
                    typeof(CompassPage),
                    "Monitor compass for changes.",
                    new[] { "compass", "sensors", "hardware", "device" }),
                new SampleItem(
                    "📶",
                    "Connectivity",
                    typeof(ConnectivityPage),
                    "Check connectivity state and detect changes.",
                    new[] { "connectivity", "internet", "wifi" }),
                new SampleItem(
                    "📱",
                    "Device Info",
                    typeof(DeviceInfoPage),
                    "Find out about the device with ease.",
                    new[] { "hardware", "device", "info", "screen", "display", "orientation", "rotation" }),
                new SampleItem(
                    "📧",
                    "Email",
                    typeof(EmailPage),
                    "Easily send email messages.",
                    new[] { "email", "share", "communication", "message" }),
                new SampleItem(
                    "📁",
                    "File System",
                    typeof(FileSystemPage),
                    "Easily save files to app data.",
                    new[] { "files", "directory", "filesystem", "storage" }),
                new SampleItem(
                    "🔦",
                    "Flashlight",
                    typeof(FlashlightPage),
                    "A simple way to turn the flashlight on/off.",
                    new[] { "flashlight", "torch", "hardware", "flash", "device" }),
                new SampleItem(
                    "📍",
                    "Geocoding",
                    typeof(GeocodingPage),
                    "Easily geocode and reverse geocoding.",
                    new[] { "geocoding", "geolocation", "position", "address", "mapping" }),
                new SampleItem(
                    "📍",
                    "Geolocation",
                    typeof(GeolocationPage),
                    "Quickly get the current location.",
                    new[] { "geolocation", "position", "address", "mapping" }),
                new SampleItem(
                    "💤",
                    "Keep Screen On",
                    typeof(KeepScreenOnPage),
                    "Keep the device screen awake.",
                    new[] { "screen", "awake", "sleep" }),
                new SampleItem(
                    "📏",
                    "Launcher",
                    typeof(LauncherPage),
                    "Launch other apps via Uri",
                    new[] { "launcher", "app", "run" }),
                new SampleItem(
                    "📏",
                    "Gyroscope",
                    typeof(GyroscopePage),
                    "Retrieve rotation around the device's three primary axes.",
                    new[] { "gyroscope", "sensors", "hardware", "device" }),
                new SampleItem(
                    "📏",
                    "Magnetometer",
                    typeof(MagnetometerPage),
                    "Detect device's orientation relative to Earth's magnetic field.",
                    new[] { "compass", "magnetometer", "sensors", "hardware", "device" }),
                new SampleItem(
                    "🗺",
                    "Launch Maps",
                    typeof(MapsPage),
                    "Easily launch maps with coordinates.",
                    new[] { "geocoding", "geolocation", "position", "address", "mapping", "maps", "route", "navigation" }),
                new SampleItem(
                    "📏",
                    "Orientation Sensor",
                    typeof(OrientationSensorPage),
                    "Retrieve orientation of the device in 3D space.",
                    new[] { "orientation", "sensors", "hardware", "device" }),
                new SampleItem(
                    "🔒",
                    "Permissions",
                    typeof(PermissionsPage),
                    "Request various permissions.",
                    new[] { "permissions" }),
                new SampleItem(
                    "📞",
                    "Phone Dialer",
                    typeof(PhoneDialerPage),
                    "Easily open the phone dialer.",
                    new[] { "phone", "dialer", "communication", "call" }),
                new SampleItem(
                    "⚙️",
                    "Preferences",
                    typeof(PreferencesPage),
                    "Quickly and easily add persistent preferences.",
                    new[] { "settings", "preferences", "prefs", "storage" }),
                new SampleItem(
                    "🔒",
                    "Secure Storage",
                    typeof(SecureStoragePage),
                    "Securely store data.",
                    new[] { "settings", "preferences", "prefs", "security", "storage" }),
                new SampleItem(
                    "📲",
                    "Share",
                    typeof(SharePage),
                    "Send text, website uris and files to other apps.",
                    new[] { "data", "transfer", "share", "communication" }),
                new SampleItem(
                    "💬",
                    "SMS",
                    typeof(SMSPage),
                    "Easily send SMS messages.",
                    new[] { "sms", "message", "text", "communication", "share" }),
                new SampleItem(
                    "🔊",
                    "Text To Speech",
                    typeof(TextToSpeechPage),
                    "Vocalize text on the device.",
                    new[] { "text", "message", "speech", "communication" }),
                new SampleItem(
                    "🌡",
                    "Unit Converters",
                    typeof(UnitConvertersPage),
                    "Easily converter different units.",
                    new[] { "units", "converters", "calculations" }),
                new SampleItem(
                    "📳",
                    "Vibration",
                    typeof(VibrationPage),
                    "Quickly and easily make the device vibrate.",
                    new[] { "vibration", "vibrate", "hardware", "device" }),
                new SampleItem(
                    "📳",
                    "Haptic Feedback",
                    typeof(HapticFeedbackPage),
                    "Quickly and easily make the device provide haptic feedback",
                    new[] { "haptic", "feedback", "hardware", "device" }),
                new SampleItem(
                    "🔓",
                    "Web Authenticator",
                    typeof(WebAuthenticatorPage),
                    "Quickly and easily authenticate and wait for a callback.",
                    new[] { "auth", "authenticate", "authenticator", "web", "webauth" }),
            };
            filteredItems = samples;
            filterText = string.Empty;
        }

        public IEnumerable<SampleItem> FilteredItems
        {
            get => filteredItems;
            private set => SetProperty(ref filteredItems, value);
        }

        public string FilterText
        {
            get => filterText;
            set
            {
                SetProperty(ref filterText, value);
                FilteredItems = Filter(samples, value);
            }
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            if (!alreadyAppeared)
            {
                alreadyAppeared = true;

                if (VersionTracking.IsFirstLaunchEver)
                {
                    DisplayAlertAsync("Welcome to the Samples! This is the first time you are launching the app!");
                }
                else if (VersionTracking.IsFirstLaunchForCurrentVersion)
                {
                    var count = VersionTracking.VersionHistory.Count();
                    DisplayAlertAsync($"Welcome to the NEW Samples! You have tried {count} versions.");
                }
            }
        }

        static IEnumerable<SampleItem> Filter(IEnumerable<SampleItem> samples, string filterText)
        {
            if (!string.IsNullOrWhiteSpace(filterText))
            {
                var lower = filterText.ToLowerInvariant();
                samples = samples.Where(s =>
                {
                    var tags = s.Tags
                        .Union(new[] { s.Name })
                        .Select(t => t.ToLowerInvariant());
                    return tags.Any(t => t.Contains(lower));
                });
            }

            return samples.OrderBy(s => s.Name);
        }
    }
}
