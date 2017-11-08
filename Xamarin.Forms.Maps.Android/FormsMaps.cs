using System;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.OS;
using Xamarin.Forms.Maps.Android;

namespace Xamarin
{
	public static class FormsMaps
	{
		public static bool IsInitialized { get; private set; }

		[Obsolete("Context is obsolete as of version 2.5. Please use a local context instead.")]
		public static Context Context { get; private set; }

		public static void Init(Activity activity, Bundle bundle)
		{
			if (IsInitialized)
				return;
			IsInitialized = true;

#pragma warning disable 618 // Setting this to support custom code which may still depend on it
			Context = activity;
#pragma warning restore 618

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