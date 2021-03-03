using System.Drawing;
using AndroidSize = Android.Util.Size;
using AndroidSizeF = Android.Util.SizeF;

namespace Microsoft.Maui.Essentials
{
	public static class SizeExtensions
	{
		public static Size ToSystemSize(this AndroidSize size) =>
			new Size(size.Width, size.Height);

		public static SizeF ToSystemSizeF(this AndroidSizeF size) =>
			new SizeF(size.Width, size.Height);

		public static AndroidSize ToPlatformSize(this Size size) =>
			new AndroidSize(size.Width, size.Height);

		public static AndroidSizeF ToPlatformSizeF(this SizeF size) =>
			new AndroidSizeF(size.Width, size.Height);
	}
}
