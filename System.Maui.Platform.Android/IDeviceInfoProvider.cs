using System;
using Android.Content.Res;

namespace System.Maui.Platform.Android
{
	public interface IDeviceInfoProvider
	{
		global::Android.Content.Res.Resources Resources { get; }

		event EventHandler ConfigurationChanged;
	}
}