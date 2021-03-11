using System.IO;
using System.Threading;

namespace Microsoft.Maui.Graphics
{
    public abstract class PdfExportContext : IDisposable
    {
        private readonly float _defaultWidth;
        private readonly float _defaultHeight;

        private float _currentPageWidth;
        private float _currentPageHeight;
        private int _pageCount;

        protected PdfExportContext(
            float defaultWidth = -1,
            float defaultHeight = -1)
        {
            if (defaultWidth <= 0 || defaultHeight <= 0)
            {
                if ("en-US".Equals(Thread.CurrentThread.CurrentCulture.Name))
                {
                    // Letter
                    defaultWidth = 612;
                    defaultHeight = 792;
                }
                else
                {
                    // A4
                    defaultWidth = 595;
                    defaultHeight = 842;
                }
            }

            _defaultWidth = defaultWidth;
            _defaultHeight = defaultHeight;
        }

        public float DefaultWidth => _defaultWidth;

        public float DefaultHeight => _defaultHeight;

        public int PageCount => _pageCount;

        public void AddPage(float width = -1, float height = -1)
        {
            if (width <= 0 || height <= 0)
            {
                _currentPageWidth = _defaultWidth;
                _currentPageHeight = _defaultHeight;
            }
            else
            {
                _currentPageWidth = width;
                _currentPageHeight = height;
            }

            AddPageImpl(_currentPageWidth, _currentPageHeight);
            _pageCount++;
        }

        public virtual void Dispose()
        {
        }

        protected abstract void AddPageImpl(float width, float height);

        public abstract ICanvas Canvas { get; }

        public abstract void WriteToStream(Stream stream);
    }
}