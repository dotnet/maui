using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	public class RenderedView
	{
		public RenderedView(byte[]? render, RenderType renderType)
		{
			this.Render = render;
			this.RenderType = renderType;
		}

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
