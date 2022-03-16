using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	public class RenderedView
	{
		public RenderedView(double width, double height, byte[]? render, RenderType renderType)
		{
			this.Width = width;
			this.Height = height;
			this.Render = render;
			this.RenderType = renderType;
		}

		public double Width { get; }

		public double Height { get; }

		public byte[]? Render { get; }

		public RenderType RenderType { get; }
	}

	public enum RenderType
	{
		JPEG,
		PNG,
		BMP,
	}
}
