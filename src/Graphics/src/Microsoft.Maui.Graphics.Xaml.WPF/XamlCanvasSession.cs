using System;

namespace Microsoft.Maui.Graphics.Xaml
{
    public class XamlCanvasSession : IDisposable
    {
        private XamlCanvas _canvas;

        public XamlCanvasSession(XamlCanvas canvas)
        {
            this._canvas = canvas;
            canvas.BeginDrawing();
        }

        public XamlCanvas Canvas => _canvas;

        public void Dispose()
        {
            if (_canvas != null)
            {
                _canvas.EndDrawing();
                _canvas = null;
            }
        }
    }
}
