using System;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents the state of a canvas, including transformation and stroke properties.
	/// </summary>
	public class CanvasState : IDisposable
	{
		/// <summary>
		/// Gets or sets the pattern of dashes and gaps used to stroke paths.
		/// </summary>
		public float[] StrokeDashPattern { get; set; }

		/// <summary>
		/// Gets or sets the distance into the dash pattern to start the dash.
		/// </summary>
		public float StrokeDashOffset { get; set; } = 1;

		/// <summary>
		/// Gets or sets the width of the stroke used to draw an object's outline.
		/// </summary>
		public float StrokeSize { get; set; } = 1;

		private Matrix3x2 _transform = Matrix3x2.Identity;
		private float _scale = 1;
		private float _scaleX = 1;
		private float _scaleY = 1;

		/// <summary>
		/// Gets or sets the transformation matrix for the canvas.
		/// </summary>
		public Matrix3x2 Transform
		{
			get => this._transform;
			set
			{
				if (this._transform == value)
					return;

				this._transform = value;
				value.DeconstructScales(out _scale, out _scaleX, out _scaleY);
				TransformChanged();
			}
		}

		/// <summary>
		/// Gets the uniform scale factor derived from the transformation matrix.
		/// </summary>
		public float Scale => this._scale;

		/// <summary>
		/// Gets the horizontal scale factor derived from the transformation matrix.
		/// </summary>
		public float ScaleX => this._scaleX;

		/// <summary>
		/// Gets the vertical scale factor derived from the transformation matrix.
		/// </summary>
		public float ScaleY => this._scaleY;

		/// <summary>
		/// Initializes a new instance of the <see cref="CanvasState"/> class.
		/// </summary>
		protected CanvasState()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CanvasState"/> class with properties from another canvas state.
		/// </summary>
		/// <param name="prototype">The canvas state to copy properties from.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="prototype"/> is null.</exception>
		protected CanvasState(CanvasState prototype)
		{
			StrokeDashPattern = prototype.StrokeDashPattern;
			StrokeDashOffset = prototype.StrokeDashOffset;
			StrokeSize = prototype.StrokeSize;

			this._transform = prototype._transform;
			this._scale = prototype._scale;
			this._scaleX = prototype._scaleX;
			this._scaleY = prototype._scaleY;
		}

		/// <summary>
		/// Called when the transformation matrix changes.
		/// </summary>
		protected virtual void TransformChanged()
		{
			// let derived classes handle the transform change if needed.
		}

		/// <summary>
		/// Gets the scale factor for lengths based on the provided transformation matrix.
		/// </summary>
		/// <param name="matrix">The transformation matrix.</param>
		/// <returns>A scale factor for lengths.</returns>
		protected static float GetLengthScale(Matrix3x2 matrix) => matrix.GetLengthScale();

		/// <summary>
		/// Releases resources used by the canvas state.
		/// </summary>
		public virtual void Dispose()
		{
			// Do nothing right now
		}
	}
}
