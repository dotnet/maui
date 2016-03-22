using System;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using System.Threading;

namespace Xamarin.Forms.UITests
{
	public class BaseTestFixture
	{
		string idiomEnvVar;
		string IPEnvVar;

		public AndroidApp App { get; private set; }
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
				// Use IDE Configuration
				App = ConfigureApp
					.Android
					.Debug ()
					.ApkFile ("../../../Xamarin.Forms.ControlGallery.Android/bin/Debug/AndroidControlGallery.AndroidControlGallery-Signed.apk")
					.StartApp ();
			} else { 
				// Use CI Configuration
				App = ConfigureApp
					.Android
					.DeviceIp (Device.IP)
					.ApkFile ("../../../Xamarin.Forms.ControlGallery.Android/bin/Debug/AndroidControlGallery.AndroidControlGallery-Signed.apk")
					.StartApp ();
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
				device = new Device (DeviceType.Phone, "10.0.1.161");

				if (!string.IsNullOrEmpty (IPEnvVar)) 
					device.IP = IPEnvVar;

			} else if (idiomEnvVar == "TABLET") {

				// default tablet
				device = new Device (DeviceType.Tablet, "10.0.1.42");

				if (!string.IsNullOrEmpty (IPEnvVar)) 
					device.IP = IPEnvVar;

			} else {

				// default phone
				device = new Device (DeviceType.Phone, "10.0.1.161");

			}

			return device;
		}

	}

	public static class PlatformStrings
	{
		public static string Button = "Button";
		public static string Cell = "xamarin.forms.platform.android.ViewCellRenderer_ViewCellContainer";
		public static string HomePageTitle = "Android Controls";
		public static string Label = "TextView";
		public static string Entry = "EditText";
		public static string Placeholder = "hint";
		public static string Text = "text";
	}

	public static class PlatformValues
	{
		public static int BoxViewScreenNumber = 4;
		public static int KeyboardDismissY = 500;
		public static int OffsetForScrollView = -5;
	}
}

