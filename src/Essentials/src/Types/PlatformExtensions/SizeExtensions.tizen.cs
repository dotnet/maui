using System.Drawing;
using ESize = ElmSharp.Size;

namespace Microsoft.Maui.Essentials
{
	public static class SizeExtensions
	{
		public static Size ToSystemSize(this ESize size) =>
			new Size(size.Width, size.Height);

		public static SizeF ToSystemSizeF(this ESize size) =>
			new SizeF(size.Width, size.Height);

		public static ESize ToPlatformSize(this Size size) =>
			new ESize(size.Width, size.Height);

		public static ESize ToPlatformSize(this SizeF size) =>
			ToPlatformSizeF(size);

		public static ESize ToPlatformSizeF(this SizeF size) =>
			new ESize((int)size.Width, (int)size.Height);
	}
}
