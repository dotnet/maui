//using System;
//using Microsoft.Maui.Graphics;
//using iOSSize = CoreGraphics.CGSize;

//namespace Microsoft.Maui.Essentials
//{
//	public static class SizeExtensions
//	{
//		public static Size ToMauiSize(this iOSSize size)
//		{
//			if (size.Width > int.MaxValue)
//				throw new ArgumentOutOfRangeException(nameof(size.Width));

//			if (size.Height > int.MaxValue)
//				throw new ArgumentOutOfRangeException(nameof(size.Height));

//			return new Size(size.Width, size.Height);
//		}

//		public static SizeF ToMauiSizeF(this iOSSize size) =>
//			new SizeF((float)size.Width, (float)size.Height);

//		public static iOSSize ToPlatformSize(this Size size) =>
//			new iOSSize((nfloat)size.Width, (nfloat)size.Height);

//		public static iOSSize ToPlatformSize(this SizeF size) =>
//			new iOSSize((nfloat)size.Width, (nfloat)size.Height);
//	}
//}
