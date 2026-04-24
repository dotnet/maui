using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Specifies the device form factor.</summary>
	[Obsolete("Use Microsoft.Maui.Devices.DeviceIdiom instead.")]
	public enum TargetIdiom
	{
		/// <summary>The device idiom is not supported.</summary>
		Unsupported,
		/// <summary>The device is a phone.</summary>
		Phone,
		/// <summary>The device is a tablet.</summary>
		Tablet,
		/// <summary>The device is a desktop computer.</summary>
		Desktop,
		/// <summary>The device is a television.</summary>
		TV,
		/// <summary>The device is a watch.</summary>
		Watch
	}
}
