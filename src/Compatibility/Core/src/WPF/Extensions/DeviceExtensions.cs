using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
{
	internal static class DeviceExtensions
	{
		public static DeviceOrientation ToDeviceOrientation(this System.Windows.Window page)
		{
			return page.Height > page.Width ? DeviceOrientation.Portrait : DeviceOrientation.Landscape;
		}
	}
}
