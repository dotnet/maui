namespace System.Graphics
{
    public abstract class AbstractPattern : IPattern
    {
        public float Width { get; }
        public float Height { get; }
        public float StepX { get; }
        public float StepY { get; }

        protected AbstractPattern(float width, float height, float stepX, float stepY)
        {
            Width = width;
            Height = height;
            StepX = stepX;
            StepY = stepY;
        }

        protected AbstractPattern(float width, float height) : this(width, height, width, height)
        {
        }

        protected AbstractPattern(float stepSize) : this(stepSize, stepSize)
        {
        }

        public abstract void Draw(ICanvas canvas);
    }
}