using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;

namespace Microsoft.Maui.Graphics.SharpDX
{
	public class TextBrush : SolidColorBrush
	{
		public SolidColorBrush Background { get; set; }

		public TextBrush(IntPtr nativePtr) : base(nativePtr)
		{
		}

		public TextBrush(RenderTarget renderTarget, RawColor4 color) : base(renderTarget, color)
		{
		}

		public TextBrush(RenderTarget renderTarget, RawColor4 color, BrushProperties? brushProperties) : base(renderTarget, color, brushProperties)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Background?.Dispose();
				Background = null;
			}

			base.Dispose(disposing);
		}
	}
}
