using System;
using System.ComponentModel;
using GLKit;
using OpenGLES;
using Foundation;
using CoreAnimation;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal class OpenGLViewRenderer : ViewRenderer<OpenGLView, GLKView>
	{
		CADisplayLink _displayLink;

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public OpenGLViewRenderer()
		{

		}

		public void Display(object sender, EventArgs eventArgs)
		{
			if (Element.HasRenderLoop)
				return;
			SetupRenderLoop(true);
		}

		protected override void Dispose(bool disposing)
		{
			if (_displayLink != null)
			{
				_displayLink.Invalidate();
				_displayLink.Dispose();
				_displayLink = null;

				if (Element != null)
					Element.DisplayRequested -= Display;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<OpenGLView> e)
		{
			if (e.OldElement != null)
				e.OldElement.DisplayRequested -= Display;

			if (e.NewElement != null)
			{
				var context = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
				var glkView = new GLKView(RectangleF.Empty)
				{
					Context = context,
					DrawableDepthFormat = GLKViewDrawableDepthFormat.Format24,
					Delegate = new Delegate(e.NewElement)
				};
				SetNativeControl(glkView);

				e.NewElement.DisplayRequested += Display;

				SetupRenderLoop(false);
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == OpenGLView.HasRenderLoopProperty.PropertyName)
				SetupRenderLoop(false);
		}

		void SetupRenderLoop(bool oneShot)
		{
			if (_displayLink != null)
				return;
			if (!oneShot && !Element.HasRenderLoop)
				return;

			_displayLink = CADisplayLink.Create(() =>
			{
				var control = Control;
				var model = Element;
				if (control != null)
					control.Display();
				if (control == null || model == null || !model.HasRenderLoop)
				{
					_displayLink?.Invalidate();
					_displayLink?.Dispose();
					_displayLink = null;
				}
			});
			_displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSRunLoopCommonModes);
		}

		class Delegate : GLKViewDelegate, IGLKViewDelegate
		{
			readonly OpenGLView _model;

			public Delegate(OpenGLView model)
			{
				_model = model;
			}

			public override void DrawInRect(GLKView view, RectangleF rect)
			{
				var onDisplay = _model.OnDisplay;
				if (onDisplay == null)
					return;
				onDisplay(rect.ToRectangle());
			}
		}
	}
}