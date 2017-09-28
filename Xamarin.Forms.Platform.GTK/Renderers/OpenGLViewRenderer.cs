using System;
using System.ComponentModel;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class OpenGLViewRenderer : ViewRenderer<OpenGLView, Controls.OpenGLView>
    {
        private Controls.OpenGLView _openGlView;
        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;

                if (Element != null)
                    ((IOpenGlViewController)Element).DisplayRequested -= OnDisplay;
            }

            base.Dispose(disposing);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<OpenGLView> e)
        {
            if (e.OldElement != null)
                ((IOpenGlViewController)e.OldElement).DisplayRequested -= OnDisplay;

            if (e.NewElement != null)
            {
                // The Open Toolkit library is a low-level C# binding for OpenGL, OpenGL ES and OpenAL. 
                // Runs on Linux, MacOS and Windows with GTK# (and more platforms).
                _openGlView = new Controls.OpenGLView();
                SetNativeControl(_openGlView);

                ((IOpenGlViewController)e.NewElement).DisplayRequested += OnDisplay;

                SetRenderMode();
                SetupRenderAction();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == OpenGLView.HasRenderLoopProperty.PropertyName)
            {
                SetRenderMode();
                SetupRenderAction();
            }
        }

        public void OnDisplay(object sender, EventArgs eventArgs)
        {
            if (Element.HasRenderLoop)
                return;

            SetupRenderAction();
        }

        private void SetRenderMode()
        {
            Control.HasRenderLoop = Element.HasRenderLoop;
        }

        private void SetupRenderAction()
        {
            if (!Element.HasRenderLoop)
                return;

            var model = Element;
            var onDisplay = model.OnDisplay;

            if (_openGlView != null)
            {
                _openGlView.OnDisplay = onDisplay;
            }
        }
    }
}