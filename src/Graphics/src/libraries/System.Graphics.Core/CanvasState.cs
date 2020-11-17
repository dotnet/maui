namespace System.Graphics
{
    public class CanvasState : IDisposable
    {
        public float[] StrokeDashPattern { get; set; }
        public float StrokeSize { get; set; } = 1;
        public float Scale { get; set; } = 1;
        public AffineTransform Transform { get; set; }

        protected CanvasState()
        {
            Transform = new AffineTransform();
        }

        protected CanvasState(CanvasState prototype)
        {
            StrokeDashPattern = prototype.StrokeDashPattern;
            StrokeSize = prototype.StrokeSize;
            Transform = new AffineTransform(prototype.Transform);
            Scale = prototype.Scale;
        }

        public virtual void Dispose()
        {
            // Do nothing right now
        }
    }
}