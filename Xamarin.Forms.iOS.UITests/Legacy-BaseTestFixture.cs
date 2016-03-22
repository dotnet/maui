using System;
using Xamarin.UITest;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using System.Threading;

namespace Xamarin.Forms.UITests
{

	public class BaseTestFixture
	{
		string idiomEnvVar;
		string IPEnvVar;

		public static iOSApp App { get; private set; }
		public Device Device { get; set; }

		public BaseTestFixture ()
		{
			idiomEnvVar = Environment.GetEnvironmentVariable ("DEVICE_IDIOM");
			IPEnvVar = Environment.GetEnvironmentVariable ("DEVICE_IP");

			Console.WriteLine (string.Format ("****** Connecting to {0} with IP: {1} ********", idiomEnvVar, IPEnvVar));

			Device = SetupDevice (idiomEnvVar, IPEnvVar);
		}

		[SetUp]
		public void Setup ()
		{

			if (string.IsNullOrEmpty (idiomEnvVar) &&
				string.IsNullOrEmpty (IPEnvVar)) { 
				// IDE Configuration
				// Use your own device information
				App = ConfigureApp
					.iOS
					.Debug ()
//					.DeviceIp ("10.0.1.159") // iPod iOS 7
					// .DeviceIp ("10.0.1.163") // iPhone iOS 7
					// .DeviceIp ("10.0.3.146") // iPod iOS 6
					.InstalledApp ("com.xamarin.quickui.controlgallery")
					.StartApp();
			} else {
				// CI Configuration
				App = ConfigureApp
					.iOS
					.DeviceIp (Device.IP)
					.InstalledApp ("com.xamarin.quickui.controlgallery")
					.StartApp();
			}
				
			FixtureSetup ();
		}

		protected virtual void FixtureSetup ()
		{
			App.SetOrientationPortrait ();
			App.Screenshot ("Begin test");
		}

		Device SetupDevice (string idiomEnvVar, string IPEnvVar)
		{
			Device device;

			if (idiomEnvVar == "PHONE") {

				// default phone
				device = new Device (DeviceType.Phone, "10.0.3.253");

				if (!string.IsNullOrEmpty (IPEnvVar)) 
					device.IP = IPEnvVar;

			} else if (idiomEnvVar == "TABLET") {

				// default tablet
				device = new Device (DeviceType.Tablet, "10.0.1.159");

				if (!string.IsNullOrEmpty (IPEnvVar)) 
					device.IP = IPEnvVar;

			} else {

				// default phone
				device = new Device (DeviceType.Phone, "10.0.3.253");

			}

			return device;
		}
	}

	public static class PlatformStrings
	{
		public static string Button = "Button";
		public static string Cell = "TableViewCell";
		public static string Entry = "TextField";
		public static string HomePageTitle = "iOS Controls";
		public static string Label = "Label";
		public static string MapPin = "view:'MKPinAnnotationView'";
		public static string Placeholder = "placeholder";
		public static string Text = "text";
	}

	public static class PlatformValues
	{
		public static int BoxViewScreenNumber = 3;
		public static int KeyboardDismissY = 200;
		public static int OffsetForScrollView = 5;
	}
}

