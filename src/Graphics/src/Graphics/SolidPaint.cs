// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics
{
	public class SolidPaint : Paint
	{
		public SolidPaint()
		{
		}

		public SolidPaint(Color color)
		{
			Color = color;
		}

		public Color Color { get; set; }

		public override bool IsTransparent
		{
			get
			{
				return Color.Alpha < 1;
			}
		}

		public override string ToString()
		{
			return $"[{nameof(SolidPaint)}: Color={Color}]";
		}
	}
}