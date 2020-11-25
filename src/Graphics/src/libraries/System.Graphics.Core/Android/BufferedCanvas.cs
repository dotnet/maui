using Android.Graphics;

namespace System.Graphics.Android
{
    /**
      *  Any rendering that utilizes a buffer from this class should synchronize rendering on the instance of this class
      *  that is being used.
      */
    public class BufferedCanvas : IDisposable
    {
        private readonly object _canvasLock = new object();

        private volatile Bitmap _bgBuffer; // all drawing is done on this buffer.
        private volatile Bitmap _fgBuffer;
        private readonly Canvas _canvas = new Canvas();

        /**
           * Call this method once drawing on a Canvas retrieved by {@link #getCanvas()} to mark
           * the buffer as fully rendered.  Failure to call this method will result in nothing being drawn.
           */
        public void Swap()
        {
            lock (_canvasLock)
            {
                var tmp = _bgBuffer;
                _bgBuffer = _fgBuffer;
                _fgBuffer = tmp;
            }
        }

        public void Resize(int w, int h)
        {
            lock (_canvasLock)
            {
                if (w <= 0 || h <= 0)
                {
                    _bgBuffer = null;
                    _fgBuffer = null;
                }
                else
                {
                    _bgBuffer?.Dispose();
                    _fgBuffer?.Dispose();

                    _bgBuffer = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);
                    _fgBuffer = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);
                }
            }
        }

        /**
           * Get a Canvas for drawing.  Actual drawing should be synchronized on the instance
           * of BufferedCanvas being used.
           * @return The Canvas instance to draw onto.  Returns null if drawing buffers have not
           *         been initialized a la {@link #resize(int, int)}.
           */
        public Canvas Canvas
        {
            get
            {
                lock (_canvasLock)
                {
                    if (_bgBuffer != null)
                    {
                        _canvas.SetBitmap(_bgBuffer);
                        return _canvas;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        /**
           * @return The most recent fully rendered Bitmsp
           */
        public Bitmap Bitmap => _fgBuffer;

        public void Dispose()
        {
            if (_bgBuffer != null)
            {
                _bgBuffer.Dispose();
                _bgBuffer = null;
            }

            if (_fgBuffer != null)
            {
                _fgBuffer.Dispose();
                _fgBuffer = null;
            }
        }
    }
}