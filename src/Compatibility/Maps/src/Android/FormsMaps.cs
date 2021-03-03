using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.OS;
using Microsoft.Maui.Controls.Compatibility.Maps.Android;

namespace Microsoft.Maui.Controls
{
	public static class FormsMaps
	{
		public static bool IsInitialized { get; private set; }

		public static void Init(Activity activity, Bundle bundle)
		{
			if (IsInitialized)
				return;
			IsInitialized = true;

			MapRenderer.Bundle = bundle;

#pragma warning disable 618
			if (GooglePlayServicesUtil.IsGooglePlayServicesAvailable(activity) == ConnectionResult.Success)
#pragma warning restore 618
			{
				try
				{
					MapsInitializer.Initialize(activity);
				}
				catch (Exception e)
				{
					Console.WriteLine("Google Play Services Not Found");
					Console.WriteLine("Exception: {0}", e);
				}
			}

			new GeocoderBackend(activity).Register();
		}
	}
}