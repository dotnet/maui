using Gtk;
using OpenTK.GLWidget;
using OpenTK.Graphics;
using System;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class OpenGLView : EventBox
    {
        private GLWidget _glWidget;

        private Action<Rectangle> _action;
        private bool _hasLoop;
        protected uint _timerId;

        public OpenGLView()
        {
            if (!GtkOpenGL.IsInitialized)
                throw new InvalidOperationException("call GtkOpenGL.Init() before use OpenGLView");

            GraphicsMode graphicsMode = GraphicsMode.Default;

            _glWidget = new GLWidget(GraphicsMode.Default);

            _glWidget.SingleBuffer = true;
            _glWidget.ColorBPP = graphicsMode.ColorFormat.BitsPerPixel;
            _glWidget.AccumulatorBPP = graphicsMode.AccumulatorFormat.BitsPerPixel;
            _glWidget.DepthBPP = graphicsMode.Depth;
            _glWidget.Samples = graphicsMode.Samples;
            _glWidget.StencilBPP = graphicsMode.Stencil;
            _glWidget.Stereo = graphicsMode.Stereo;
            _glWidget.GlVersionMajor = 2;
            _glWidget.GlVersionMinor = 1;
            _glWidget.CanFocus = true;
            _glWidget.GraphicsContextFlags = GraphicsContextFlags.Default;

            _glWidget.Initialized += new EventHandler(OnGLWidgetInitialized);
            _glWidget.RenderFrame += new EventHandler(OnGLWidgetRenderer);
            _glWidget.Destroyed += new EventHandler(OnGLWidgetDestroy);

            _glWidget.AddEvents((int)Gdk.EventMask.AllEventsMask);

            Add(_glWidget);

            _glWidget.ShowAll();
        }

        public Action<Rectangle> OnDisplay
        {
            get { return _action; }
            set { _action = value; }
        }

        public bool HasRenderLoop
        {
            get { return _hasLoop; }
            set { _hasLoop = value; }
        }

        protected void OnGLWidgetInitialized(object sender, EventArgs e)
        {
            _timerId = GLib.Timeout.Add(16, new GLib.TimeoutHandler(Render));
        }

        protected void OnGLWidgetRenderer(object sender, EventArgs e)
        {
            _timerId = GLib.Timeout.Add(16, new GLib.TimeoutHandler(Render));
        }

        protected void OnGLWidgetDestroy(object sender, EventArgs e)
        {
            GLib.Source.Remove(_timerId);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_glWidget != null)
            {
                _glWidget.Dispose();
            }
        }

        private bool Render()
        {
            if (!HasRenderLoop)
            {
                return false;
            }
            else
            {
                OnDisplay(new Rectangle(0, 0, WidthRequest, HeightRequest));

                return true;
            }
        }
    }
}