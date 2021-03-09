using System;
using System.Collections.Generic;
using ElmSharp;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native
{
	public class BorderRectangle : RoundRectangle
	{
		public BorderRectangle(EvasObject parent) : base(parent) { }

		public int BorderWidth { get; set; }

		protected override void DrawPoints()
		{
			ClearPoints();
			if (BorderWidth > 0)
			{
				IReadOnlyList<int> radius = GetRadius();
				DrawRect(radius[0], radius[1], radius[2], radius[3],
					0, 0, Width, Height);
				DrawRect(Math.Max(radius[0] - BorderWidth, 0),
					Math.Max(radius[1] - BorderWidth, 0),
					Math.Max(radius[2] - BorderWidth, 0),
					Math.Max(radius[3] - BorderWidth, 0),
					BorderWidth, BorderWidth, Width - BorderWidth * 2, Height - BorderWidth * 2);
			}
		}

		protected void DrawRect(int topLeft, int topRight, int bottomLeft, int bottomRight, int startX, int startY, int width, int height)
		{
			int[] radius = new int[4];
			int maxR = Math.Min(width / 2, height / 2);
			radius[0] = Math.Min(topLeft, maxR);
			radius[1] = Math.Min(topRight, maxR);
			radius[2] = Math.Min(bottomLeft, maxR);
			radius[3] = Math.Min(bottomRight, maxR);

			Point first = new Point(-1, -1);
			for (int i = 0; i <= radius[0]; i++)
			{
				int x = i;
				int dx = radius[0] - x;
				int y = radius[0] - (int)Math.Sqrt((radius[0] * radius[0]) - (dx * dx));
				AddRelativePoint(startX + x, startY + y);
				if (first.X < 0 && first.Y < 0)
				{
					first.X = startX + x;
					first.Y = startY + y;
				}
			}

			AddRelativePoint(startX + width - radius[1], startY);

			for (int i = width - radius[1]; i <= width; i++)
			{
				int x = i;
				int dx = radius[1] - (width - x);
				int y = radius[1] - (int)Math.Sqrt((radius[1] * radius[1]) - (dx * dx));
				AddRelativePoint(startX + x, startY + y);
			}

			AddRelativePoint(startX + width, startY + height - radius[3]);

			for (int i = width; i >= width - radius[3]; i--)
			{
				int x = i;
				int dx = radius[3] - (width - x);
				int y = height - radius[3] + (int)Math.Sqrt((radius[3] * radius[3]) - (dx * dx));
				AddRelativePoint(startX + x, startY + y);
			}

			AddRelativePoint(startX + radius[2], startY + height);

			for (int i = radius[2]; i >= 0; i--)
			{
				int x = i;
				int dx = radius[2] - x;
				int y = height - radius[2] + (int)Math.Sqrt((radius[2] * radius[2]) - (dx * dx));
				AddRelativePoint(startX + x, startY + y);
			}
			AddRelativePoint((int)first.X, (int)first.Y);
		}
	}
}
