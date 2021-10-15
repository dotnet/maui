using System;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	public class CanvasState : IDisposable
	{
		public float[] StrokeDashPattern { get; set; }
		public float StrokeSize { get; set; } = 1;

		private Matrix3x2 _transform = Matrix3x2.Identity;
		private float _scale = 1;
		private float _scaleX = 1;
		private float _scaleY = 1;

		public Matrix3x2 Transform
		{
			get => this._transform;
			set
			{
				if (this._transform == value) return;

				this._transform = value;
				value.DeconstructScales(out _scale, out _scaleX, out _scaleY);
				TransformChanged();
			}
		}
		public float Scale => this._scale;
		public float ScaleX => this._scaleX;
		public float ScaleY => this._scaleY;

		protected CanvasState()
		{
			
		}

		protected CanvasState(CanvasState prototype)
		{
			StrokeDashPattern = prototype.StrokeDashPattern;
			StrokeSize = prototype.StrokeSize;

			this._transform = prototype._transform;
			this._scale = prototype._scale;
			this._scaleX = prototype._scaleX;
			this._scaleY = prototype._scaleY;
		}

		protected virtual void TransformChanged()
		{
			// let derived classes handle the transform change if needed.
		}

		protected static float GetLengthScale(Matrix3x2 matrix) => matrix.GetLengthScale();

		public virtual void Dispose()
		{
			// Do nothing right now
		}
	}
}
