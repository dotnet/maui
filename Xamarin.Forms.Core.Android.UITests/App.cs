using NUnit.Framework;

using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Configuration;

namespace Xamarin.Forms.Core.UITests
{
	internal static class RunningApp
	{
		public static AndroidApp App;

		public static void Restart ()
		{
			App = ConfigureApp
				.Android
				.Debug ()
				.ApkFile ("../../../Xamarin.Forms.ControlGallery.Android/bin/Debug/AndroidControlGallery.AndroidControlGallery-Signed.apk")
				.StartApp ();
		}
	}
}