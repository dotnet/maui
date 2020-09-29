#if HAVE_OPENTK
using System;
using OpenTK.Graphics.OpenGL;

namespace Xamarin.Forms.Controls
{
	public class BasicOpenGLGallery : ContentPage
	{
		private bool _initGl = false;
		private int _viewportWidth;
		private int _viewportHeight;
		private uint _mProgramHandle;
		private OpenGLView _openGLView = null;

		public BasicOpenGLGallery()
		{
			Title = "Basic OpenGLView Sample";

			var titleLabel = new Label
			{
				Text = "OpenGLView",
				FontSize = 36
			};

			_openGLView = new OpenGLView
			{
				HeightRequest = 300,
				WidthRequest = 300,
				HasRenderLoop = true
			};

			_openGLView.OnDisplay = r =>
			{
				if (!_initGl)
				{
					double width_in_pixels = 300;
					double height_in_pixels = 300;

					InitGl((int)width_in_pixels, (int)height_in_pixels);
				}

				Render();
			};

			var stack = new StackLayout
			{
				Padding = new Size(12, 12),
				Children = { titleLabel, _openGLView }
			};

			Content = stack;
		}

		void InitGl(int width, int height)
		{
			_viewportHeight = width;
			_viewportWidth = height;

			_mProgramHandle = (uint)GL.CreateProgram();
			if (_mProgramHandle == 0)
				throw new InvalidOperationException("Unable to create program");

			GL.BindAttribLocation(_mProgramHandle, 0, "vPosition");
			GL.LinkProgram(_mProgramHandle);

			GL.Viewport(0, 0, _viewportWidth, _viewportHeight);

			GL.UseProgram(_mProgramHandle);

			_initGl = true;
		}


		void Render()
		{
#pragma warning disable 0618
			GL.ClearColor(0, 0, 0, 0);
			GL.Clear(ClearBufferMask.ColorBufferBit);

			GL.Color3(1.0f, 0.85f, 0.35f);
			GL.Begin(BeginMode.Triangles);

			GL.Vertex3(0.0, 0.6, 0.0);
			GL.Vertex3(-0.2, -0.3, 0.0);
			GL.Vertex3(0.2, -0.3, 0.0);
#pragma warning restore 0618

			GL.End();

			GL.Flush();
		}
	}
}
#endif