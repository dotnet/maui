using System;
using Microsoft.Maui.Graphics;
using WPoint = Windows.Foundation.Point;

namespace Microsoft.Maui.Controls.Platform
{
	public static class PointExtensions
	{
		[Obsolete("ToWindows is obsolete. Please use ToNative instead")]
		public static WPoint ToWindows(this Point point) => point.ToNative();
	}
}