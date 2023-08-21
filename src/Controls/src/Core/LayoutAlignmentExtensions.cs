// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Controls
{
	internal static class LayoutAlignmentExtensions
	{
		public static double ToDouble(this LayoutAlignment align)
		{
			switch (align)
			{
				case LayoutAlignment.Start:
					return 0;
				case LayoutAlignment.Center:
					return 0.5;
				case LayoutAlignment.End:
					return 1;
			}
			throw new ArgumentOutOfRangeException("align");
		}
	}
}