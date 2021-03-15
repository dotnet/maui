using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics
{
    public abstract class AbstractCanvas<TState> : ICanvas, IDisposable where TState : CanvasState
    {
        private readonly Stack<TState> _stateStack = new Stack<TState>();
        private readonly Func<object, TState> _createNew;
        private readonly Func<TState, TState> _createCopy;

        private TState _currentState;
        private bool _limitStrokeScaling;
        private float _strokeLimit = 1;
        private bool _strokeDashPatternDirty;

        protected abstract float NativeStrokeSize { set; }
        protected abstract void NativeSetStrokeDashPattern(float[] pattern, float strokeSize);
        protected abstract void NativeDrawLine(float x1, float y1, float x2, float y2);
        protected abstract void NativeDrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed);
        protected abstract void NativeDrawRectangle(float x, float y, float width, float height);
        protected abstract void NativeDrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius);
        protected abstract void NativeDrawEllipse(float x, float y, float width, float height);
        protected abstract void NativeDrawPath(PathF path);
        protected abstract void NativeRotate(float degrees, float radians, float x, float y);
        protected abstract void NativeRotate(float degrees, float radians);
        protected abstract void NativeScale(float fx, float fy);
        protected abstract void NativeTranslate(float tx, float ty);
        protected abstract void NativeConcatenateTransform(AffineTransform transform);

        protected AbstractCanvas(Func<object, TState> createNew, Func<TState, TState> createCopy)
        {
            _createCopy = createCopy;
            _createNew = createNew;
            _currentState = createNew(this);
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
        public abstract string FontName { set; }
        public abstract float FontSize { set; }
        public abstract float Alpha { set; }
        public abstract bool Antialias { set; }
        public abstract BlendMode BlendMode { set; }

        protected float AssignedStrokeLimit => _strokeLimit;

        public virtual float DisplayScale { get; set; }
        
        public float RetinaScale { get; set; }

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
                NativeStrokeSize = size;
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

        private void EnsureStrokePatternSet()
        {
            if (_strokeDashPatternDirty)
            {
                NativeSetStrokeDashPattern(_currentState.StrokeDashPattern, _currentState.StrokeSize);
                _strokeDashPatternDirty = false;
            }
        }

        public abstract void ClipRectangle(float x, float y, float width, float height);

        public void DrawLine(float x1, float y1, float x2, float y2)
        {
            EnsureStrokePatternSet();
            NativeDrawLine(x1, y1, x2, y2);
        }

        public void DrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed)
        {
            EnsureStrokePatternSet();
            NativeDrawArc(x, y, width, height, startAngle, endAngle, clockwise, closed);
        }

        public abstract void FillArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise);

        public void DrawRectangle(float x, float y, float width, float height)
        {
            EnsureStrokePatternSet();
            NativeDrawRectangle(x, y, width, height);
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
            NativeDrawRoundedRectangle(x, y, width, height, cornerRadius);
        }

        public abstract void FillRoundedRectangle(float x, float y, float width, float height, float cornerRadius);

        public void DrawEllipse(float x, float y, float width, float height)
        {
            EnsureStrokePatternSet();
            NativeDrawEllipse(x, y, width, height);
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
            NativeDrawPath(path);
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

            _currentState = _createNew(this);
        }

        public abstract void SetShadow(SizeF offset, float blur, Color color);
        public abstract void SetFillPaint(Paint paint, float x1, float y1, float x2, float y2);
        public abstract void SetToSystemFont();
        public abstract void SetToBoldSystemFont();
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

            _currentState = _createNew(this);
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
            _currentState = _createCopy(previousState);
            _strokeDashPatternDirty = true;
        }

        public void Rotate(float degrees, float x, float y)
        {
            var radians = Geometry.DegreesToRadians(degrees);

            var transform = _currentState.Transform;
            transform.Translate(x, y);
            transform.Rotate(radians);
            transform.Translate(-x, -y);

            NativeRotate(degrees, radians, x, y);
        }

        public void Rotate(float degrees)
        {
            var radians = Geometry.DegreesToRadians(degrees);
            _currentState.Transform.Rotate(radians);

            NativeRotate(degrees, radians);
        }

        public void Scale(float fx, float fy)
        {
            _currentState.Scale *= fx;
            _currentState.Transform.Scale(fx, fy);

            NativeScale(fx, fy);
        }

        public void Translate(float tx, float ty)
        {
            _currentState.Transform.Translate(tx, ty);
            NativeTranslate(tx, ty);
        }

        public void ConcatenateTransform(AffineTransform transform)
        {
            _currentState.Scale *= transform.ScaleX;
            _currentState.Transform.Concatenate(transform);
            NativeConcatenateTransform(transform);
        }
    }
}