using System;
using System.Maui.Graphics;

namespace System.Maui.Shapes
{
	public interface IShape
	{
		Path PathForBounds(Maui.Rectangle rect);
	}
}
