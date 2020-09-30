using System;
using Android.Content.Res;

namespace Xamarin.Forms.Platform.Android
{
	public interface IDeviceInfoProvider
	{
		Resources Resources { get; }

		event EventHandler ConfigurationChanged;
	}
}