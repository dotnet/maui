// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	static class DisplayInfoExtensions
	{
		public static Size GetScaledScreenSize(this DisplayInfo info)
		{
			if (info.Density == 0)
				return Size.Zero;

			return new Size(info.Width / info.Density, info.Height / info.Density);
		}

		public static double DisplayRound(this DisplayInfo info, double value) =>
			Math.Round(value);
	}
}