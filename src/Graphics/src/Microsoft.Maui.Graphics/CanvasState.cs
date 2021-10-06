using System;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	public class CanvasState : IDisposable
	{
		public float[] StrokeDashPattern { get; set; }
		public float StrokeSize { get; set; } = 1;
		public float Scale { get; set; } = 1;
		public Matrix3x2 Transform { get; set; }

		protected CanvasState()
		{
			Transform = Matrix3x2.Identity;
		}

		protected CanvasState(CanvasState prototype)
		{
			StrokeDashPattern = prototype.StrokeDashPattern;
			StrokeSize = prototype.StrokeSize;
			Transform = prototype.Transform;
			Scale = prototype.Scale;
		}

		public virtual void Dispose()
		{
			// Do nothing right now
		}
	}
}
