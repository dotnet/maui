using System;
using Android.Content.Res;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IDeviceInfoProvider
	{
		Resources Resources { get; }

		event EventHandler ConfigurationChanged;
	}
}