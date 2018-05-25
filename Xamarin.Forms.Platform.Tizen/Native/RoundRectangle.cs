using System;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class RoundRectangle : Polygon
	{
		int[] _radius = new int[4];
		public RoundRectangle(EvasObject parent) : base(parent)
		{
		}

		public int X { get; set; }
		public int Y { get; set; }

		public int Width { get; set; }
		public int Height { get; set; }

		public void SetRadius(int r)
		{
			SetRadius(r, r, r, r);
		}

		public void SetRadius(int topLeft, int topRight, int bottomLeft, int bottomRight)
		{
			_radius[0] = topLeft;
			_radius[1] = topRight;
			_radius[2] = bottomLeft;
			_radius[3] = bottomRight;
		}

		public void Draw()
		{
			DrawPoints();
		}

		public void Draw(Rect bound)
		{
			X = bound.X;
			Y = bound.Y;
			Width = bound.Width;
			Height = bound.Height;
			Draw();
			// It is workaround for fix geometry issue
			// A polygon make a margin of 1 pixel at the outermost point
			Geometry = bound;
		}


		protected virtual void DrawPoints()
		{
			int[] radius = new int[4];
			int maxR = Math.Min(Width / 2, Height / 2);
			radius[0] = Math.Min(_radius[0], maxR);
			radius[1] = Math.Min(_radius[1], maxR);
			radius[2] = Math.Min(_radius[2], maxR);
			radius[3] = Math.Min(_radius[3], maxR);

			ClearPoints();
			for (int i = 0; i <= radius[0]; i++)
			{
				int x = i;
				int dx = radius[0] - x;
				int y = radius[0] - (int)Math.Sqrt((radius[0] * radius[0]) - (dx * dx));
				AddRelativePoint(x, y);
			}

			AddRelativePoint(Width - radius[1], 0);

			for (int i = Width - radius[1]; i <= Width; i++)
			{
				int x = i;
				int dx = radius[1] - (Width - x);
				int y = radius[1] - (int)Math.Sqrt((radius[1] * radius[1]) - (dx * dx));
				AddRelativePoint(x, y);
			}

			AddRelativePoint(Width, Height - radius[3]);

			for (int i = Width; i >= Width - radius[3]; i--)
			{
				int x = i;
				int dx = radius[3] - (Width - x);
				int y = Height - radius[3] + (int)Math.Sqrt((radius[3] * radius[3]) - (dx * dx));
				AddRelativePoint(x, y);
			}

			AddRelativePoint(radius[2], Height);

			for (int i = radius[2]; i >= 0; i--)
			{
				int x = i;
				int dx = radius[2] - x;
				int y = Height - radius[2] + (int)Math.Sqrt((radius[2] * radius[2]) - (dx * dx));
				AddRelativePoint(x, y);
			}
		}

		void AddRelativePoint(int x, int y)
		{
			AddPoint(X + x, Y + y);
		}
	}
}
