using System;
using System.Collections.Generic;
using System.Numerics;

using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides an abstract base implementation of the <see cref="ICanvas"/> interface.
	/// </summary>
	/// <typeparam name="TState">The type of state managed by this canvas, which must derive from <see cref="CanvasState"/>.</typeparam>
	/// <remarks>
	/// This class handles state management, coordinate transformation, and common drawing operations,
	/// while delegating platform-specific rendering to derived classes.
	/// </remarks>
	public abstract class AbstractCanvas<TState> : ICanvas, IDisposable where TState : CanvasState
	{
		private readonly ICanvasStateService<TState> _stateService;
		private readonly IStringSizeService _stringSizeService;

		private readonly Stack<TState> _stateStack = new Stack<TState>();

		private TState _currentState;
		private bool _limitStrokeScaling;
		private float _strokeLimit = 1;
		private bool _strokeDashPatternDirty;

		protected abstract float PlatformStrokeSize { set; }

		protected abstract void PlatformSetStrokeDashPattern(float[] strokePattern, float strokeDashOffset, float strokeSize);
		protected abstract void PlatformDrawLine(float x1, float y1, float x2, float y2);
		protected abstract void PlatformDrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed);
		protected abstract void PlatformDrawRectangle(float x, float y, float width, float height);
		protected abstract void PlatformDrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius);
		protected abstract void PlatformDrawEllipse(float x, float y, float width, float height);
		protected abstract void PlatformDrawPath(PathF path);
		protected abstract void PlatformRotate(float degrees, float radians, float x, float y);
		protected abstract void PlatformRotate(float degrees, float radians);
		protected abstract void PlatformScale(float fx, float fy);
		protected abstract void PlatformTranslate(float tx, float ty);
		protected abstract void PlatformConcatenateTransform(Matrix3x2 transform);

		protected AbstractCanvas(ICanvasStateService<TState> stateService, IStringSizeService stringSizeService)
		{
			_stateService = stateService;
			_stringSizeService = stringSizeService;
			_currentState = stateService.CreateNew(this);
		}

		protected TState CurrentState => _currentState;

		public virtual void Dispose()
		{
			if (_currentState != null)
			{
				_currentState.Dispose();
				_currentState = null;
			}
		}

		public bool LimitStrokeScaling
		{
			set => _limitStrokeScaling = value;
		}

		protected bool LimitStrokeScalingEnabled => _limitStrokeScaling;

		public float StrokeLimit
		{
			set => _strokeLimit = value;
		}

		public abstract Color FillColor { set; }
		public abstract Color FontColor { set; }
		public abstract IFont Font { set; }
		public abstract float FontSize { set; }
		public abstract float Alpha { set; }
		public abstract bool Antialias { set; }
		public abstract BlendMode BlendMode { set; }

		protected float AssignedStrokeLimit => _strokeLimit;

		public virtual float DisplayScale { get; set; } = 1;

		public float StrokeSize
		{
			set
			{
				var size = value;

				if (_limitStrokeScaling)
				{
					var scale = _currentState.Scale;
					var scaledStrokeSize = scale * value;
					if (scaledStrokeSize < _strokeLimit)
					{
						size = _strokeLimit / scale;
					}
				}

				_currentState.StrokeSize = size;
				PlatformStrokeSize = size;
			}
		}

		public abstract float MiterLimit { set; }
		public abstract Color StrokeColor { set; }
		public abstract LineCap StrokeLineCap { set; }
		public abstract LineJoin StrokeLineJoin { set; }

		public float[] StrokeDashPattern
		{
			set
			{
				if (!ReferenceEquals(value, _currentState.StrokeDashPattern))
				{
					_currentState.StrokeDashPattern = value;
					_strokeDashPatternDirty = true;
				}
			}
		}
		public float StrokeDashOffset
		{
			set
			{
				var dashOffset = value;

				_currentState.StrokeDashOffset = dashOffset;
			}
		}

		private void EnsureStrokePatternSet()
		{
			if (_strokeDashPatternDirty)
			{
				PlatformSetStrokeDashPattern(_currentState.StrokeDashPattern, _currentState.StrokeDashOffset, _currentState.StrokeSize);
				_strokeDashPatternDirty = false;
			}
		}

		public abstract void ClipRectangle(float x, float y, float width, float height);

		public void DrawLine(float x1, float y1, float x2, float y2)
		{
			EnsureStrokePatternSet();
			PlatformDrawLine(x1, y1, x2, y2);
		}

		public void DrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed)
		{
			EnsureStrokePatternSet();
			PlatformDrawArc(x, y, width, height, startAngle, endAngle, clockwise, closed);
		}

		public abstract void FillArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise);

		public void DrawRectangle(float x, float y, float width, float height)
		{
			EnsureStrokePatternSet();
			PlatformDrawRectangle(x, y, width, height);
		}

		public abstract void FillRectangle(float x, float y, float width, float height);

		public void DrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
		{
			var halfHeight = Math.Abs(height / 2);
			if (cornerRadius > halfHeight)
				cornerRadius = halfHeight;

			var halfWidth = Math.Abs(width / 2);
			if (cornerRadius > halfWidth)
				cornerRadius = halfWidth;

			EnsureStrokePatternSet();
			PlatformDrawRoundedRectangle(x, y, width, height, cornerRadius);
		}

		public abstract void FillRoundedRectangle(float x, float y, float width, float height, float cornerRadius);

		public void DrawEllipse(float x, float y, float width, float height)
		{
			EnsureStrokePatternSet();
			PlatformDrawEllipse(x, y, width, height);
		}

		public abstract void FillEllipse(float x, float y, float width, float height);
		public abstract void DrawString(string value, float x, float y, HorizontalAlignment horizontalAlignment);

		public abstract void DrawString(
			string value,
			float x,
			float y,
			float width,
			float height,
			HorizontalAlignment horizontalAlignment,
			VerticalAlignment verticalAlignment,
			TextFlow textFlow = TextFlow.ClipBounds,
			float lineSpacingAdjustment = 0);

		public abstract void DrawText(IAttributedText value, float x, float y, float width, float height);

		public void DrawPath(PathF path)
		{
			EnsureStrokePatternSet();
			PlatformDrawPath(path);
		}

		public abstract void FillPath(PathF path, WindingMode windingMode);
		public abstract void SubtractFromClip(float x, float y, float width, float height);
		public abstract void ClipPath(PathF path, WindingMode windingMode = WindingMode.NonZero);

		public virtual void ResetState()
		{
			while (_stateStack.Count > 0)
			{
				if (_currentState != null)
				{
					_currentState.Dispose();
					_currentState = null;
				}

				_currentState = _stateStack.Pop();
				StateRestored(_currentState);
			}

			if (_currentState != null)
			{
				_currentState.Dispose();
				_currentState = null;
			}

			_currentState = _stateService.CreateNew(this);
		}

		public abstract void SetShadow(SizeF offset, float blur, Color color);
		public abstract void SetFillPaint(Paint paint, RectF rectangle);

		public abstract void DrawImage(IImage image, float x, float y, float width, float height);

		public virtual bool RestoreState()
		{
			_strokeDashPatternDirty = true;

			if (_currentState != null)
			{
				_currentState.Dispose();
				_currentState = null;
			}

			if (_stateStack.Count > 0)
			{
				_currentState = _stateStack.Pop();
				StateRestored(_currentState);
				return true;
			}

			_currentState = _stateService.CreateNew(this);
			return false;
		}

		protected virtual void StateRestored(TState state)
		{
			// Do nothing
		}

		public virtual void SaveState()
		{
			var previousState = _currentState;
			_stateStack.Push(previousState);
			_currentState = _stateService.CreateCopy(previousState);
			_strokeDashPatternDirty = true;
		}

		public void Rotate(float degrees, float x, float y)
		{
			var radians = GeometryUtil.DegreesToRadians(degrees);

			var transform = _currentState.Transform;
			transform = Matrix3x2.CreateTranslation(x, y) * transform;
			transform = Matrix3x2.CreateRotation(radians) * transform;
			transform = Matrix3x2.CreateTranslation(-x, -y) * transform;
			_currentState.Transform = transform;

			PlatformRotate(degrees, radians, x, y);
		}

		public void Rotate(float degrees)
		{
			var radians = GeometryUtil.DegreesToRadians(degrees);

			var transform = _currentState.Transform;
			transform = Matrix3x2.CreateRotation(radians) * transform;
			_currentState.Transform = transform;

			PlatformRotate(degrees, radians);
		}

		public void Scale(float fx, float fy)
		{
			var transform = _currentState.Transform;
			transform = Matrix3x2.CreateScale(fx, fy) * transform;
			_currentState.Transform = transform;

			PlatformScale(fx, fy);
		}

		public void Translate(float tx, float ty)
		{
			var transform = _currentState.Transform;
			transform = Matrix3x2.CreateTranslation(tx, ty) * transform;
			_currentState.Transform = transform;

			PlatformTranslate(tx, ty);
		}

		public void ConcatenateTransform(Matrix3x2 transform)
		{
			_currentState.Transform = transform * _currentState.Transform;

			PlatformConcatenateTransform(transform);
		}

		public SizeF GetStringSize(string value, IFont font, float fontSize) =>
			_stringSizeService.GetStringSize(value, font, fontSize);

		public SizeF GetStringSize(string aString, IFont font, float aFontSize, HorizontalAlignment aHorizontalAlignment, VerticalAlignment aVerticalAlignment) =>
			_stringSizeService.GetStringSize(aString, font, aFontSize, aHorizontalAlignment, aVerticalAlignment);
	}
}
