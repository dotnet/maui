namespace System.Graphics.CoreGraphics
{
    public class NativeCanvasState : CanvasState
    {
        private bool _shadowed;

        public NativeCanvasState() : base()
        {
        }

        public NativeCanvasState(NativeCanvasState prototype) : base(prototype)
        {
            _shadowed = prototype._shadowed;
        }

        public bool Shadowed
        {
            get => _shadowed;
            set => _shadowed = value;
        }
    }
}