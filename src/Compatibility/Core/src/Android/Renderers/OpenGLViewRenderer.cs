using System;
using System.ComponentModel;
using Android.Content;
using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	//public class OpenGLViewRenderer : ViewRenderer<OpenGLView, GLSurfaceView>
	internal class OpenGLViewRenderer : ViewRenderer<OpenGLView, GLSurfaceView>
	{
		bool _disposed;

		public OpenGLViewRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				_disposed = true;

				if (Element != null)
					((IOpenGlViewController)Element).DisplayRequested -= Render;
			}
			base.Dispose(disposing);
		}

		protected override GLSurfaceView CreateNativeControl()
		{
			return new GLSurfaceView(Context);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<OpenGLView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
				((IOpenGlViewController)Element).DisplayRequested -= Render;

			if (e.NewElement != null)
			{
				GLSurfaceView surfaceView = Control;
				if (surfaceView == null)
				{
					surfaceView = CreateNativeControl();
					surfaceView.SetEGLContextClientVersion(2);
					SetNativeControl(surfaceView);
				}

				((IOpenGlViewController)Element).DisplayRequested += Render;
				surfaceView.SetRenderer(new Renderer(Element));
				SetRenderMode();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == OpenGLView.HasRenderLoopProperty.PropertyName)
				SetRenderMode();
		}

		void Render(object sender, EventArgs eventArgs)
		{
			if (Element.HasRenderLoop)
				return;
			Control.RequestRender();
		}

		void SetRenderMode()
		{
			Control.RenderMode = Element.HasRenderLoop ? Rendermode.Continuously : Rendermode.WhenDirty;
		}

		class Renderer : Object, GLSurfaceView.IRenderer
		{
			readonly OpenGLView _model;
			Rectangle _rect;

			public Renderer(OpenGLView model)
			{
				_model = model;
			}

			public void OnDrawFrame(IGL10 gl)
			{
				Action<Rectangle> onDisplay = _model.OnDisplay;
				if (onDisplay == null)
					return;
				onDisplay(_rect);
			}

			public void OnSurfaceChanged(IGL10 gl, int width, int height)
			{
				_rect = new Rectangle(0.0, 0.0, width, height);
			}

			public void OnSurfaceCreated(IGL10 gl, EGLConfig config)
			{
			}
		}
	}
}