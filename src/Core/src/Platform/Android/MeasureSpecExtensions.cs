// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Android.Content;
using Android.Views;
using static Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public static class MeasureSpecExtensions
	{
		public static int GetSize(this int measureSpec)
		{
			const int modeMask = 0x3 << 30;
			return measureSpec & ~modeMask;
		}

		public static MeasureSpecMode GetMode(this int measureSpec)
		{
			return MeasureSpec.GetMode(measureSpec);
		}

		public static int MakeMeasureSpec(this MeasureSpecMode mode, int size)
		{
			return size + (int)mode;
		}

		public static double ToDouble(this int measureSpec, Context context)
		{
			return measureSpec.GetMode() == MeasureSpecMode.Unspecified
				? double.PositiveInfinity
				: context.FromPixels(measureSpec.GetSize());
		}
	}
}
