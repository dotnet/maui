using System;
using System.Runtime.InteropServices;
using ElmSharp;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public static class EvasMapExtensions
	{
		public static void Perspective3D(this EvasMap map, int px, int py, int z0, int foc)
		{
			var mapType = typeof(EvasMap);
			var propInfo = mapType.GetProperty("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var handle = (IntPtr)propInfo.GetValue(map);
			evas_map_util_3d_perspective(handle, px, py, z0, foc);
		}

		[DllImport("libevas.so.1")]
		static extern void evas_map_util_3d_perspective(IntPtr map, int px, int py, int z0, int foc);
	}
}
