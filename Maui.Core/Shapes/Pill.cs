//using System;
//using System.Maui.Graphics;

//namespace System.Maui.Shapes {
//	public class Pill : IShape
//	{
//		public Pill(Orientation orientation)
//		{
//			Orientation = orientation;
//		}

//		public Orientation Orientation { get; }

//		public Path PathForBounds (Maui.Rectangle rect)
//		{
//			var cornerRadius = (Orientation == Orientation.Horizontal ? rect.Height : rect.Width) / 2f;
//			var path = new Path();
//			path.AppendRoundedRectangle(rect, cornerRadius);
//			return path;
//		}
//	}
//}
