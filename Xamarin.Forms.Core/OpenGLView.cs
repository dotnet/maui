using System;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_OpenGLViewRenderer))]
	public sealed class OpenGLView : View, IOpenGlViewController
	{
		#region Statics

		public static readonly BindableProperty HasRenderLoopProperty = BindableProperty.Create("HasRenderLoop", typeof(bool), typeof(OpenGLView), default(bool));

		#endregion

		public bool HasRenderLoop
		{
			get { return (bool)GetValue(HasRenderLoopProperty); }
			set { SetValue(HasRenderLoopProperty, value); }
		}

		public Action<Rectangle> OnDisplay { get; set; }

		event EventHandler IOpenGlViewController.DisplayRequested
		{
			add { DisplayRequested += value; }
			remove { DisplayRequested -= value; }
		}

		public void Display()
		{
			EventHandler handler = DisplayRequested;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		event EventHandler DisplayRequested;
	}
}