#if HAVE_OPENTK
using System;
using OpenTK;

#if __WPF__ || __GTK__ || __MACOS__
using OpenTK.Graphics.OpenGL;
#elif __ANDROID__ || __IOS__
using OpenTK.Graphics.ES20;
#endif

namespace Xamarin.Forms.Controls
{
	public class AdvancedOpenGLGallery : ContentPage
	{
		private const string VertexShader = @"
            uniform mat4 uMVPMatrix;
            attribute vec4 vColor;
            attribute vec4 vPosition; 
            varying vec4 color;
            void main()   
            {              
                color = vColor;
                gl_Position = uMVPMatrix * vPosition;
            }";

		private const string FragmentShader = @"
            varying lowp vec4 color;
            void main (void)
            {
                gl_FragColor = color;
            }";

		private bool _initGl = false;
		private int _viewportWidth;
		private int _viewportHeight;
		private Vector4[] _vertices;
		private Vector4[] _colors;

		private uint _mProgramHandle;
		private int _mColorHandle;
		private int _mPositionHandle;
		private int _mMVPMatrixHandle;
		private Matrix4 _mProjectionMatrix;
		private Matrix4 _mViewMatrix;
		private Matrix4 _mModelViewProjectionMatrix;

		private Xamarin.Forms.OpenGLView _openGLView = null;

		public AdvancedOpenGLGallery()
		{
			Title = "Advanced OpenGLView Sample";

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

			_vertices = new Vector4[]
			{
				new Vector4(0.0f, 0.5f, 0.0f, 1.0f),
				new Vector4(0.5f, -0.5f, 0.0f, 1.0f),
				new Vector4(-0.5f, -0.5f, 0.0f, 1.0f)
			};

			_colors = new Vector4[]
			{
				new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
				new Vector4(0.0f, 0.0f, 1.0f, 1.0f)
			};

			uint vertexShader = CompileShader(VertexShader, ShaderType.VertexShader);
			uint fragmentShader = CompileShader(FragmentShader, ShaderType.FragmentShader);

			_mProgramHandle = (uint)GL.CreateProgram();
			if (_mProgramHandle == 0)
				throw new InvalidOperationException("Unable to create program");

			GL.AttachShader(_mProgramHandle, vertexShader);
			GL.AttachShader(_mProgramHandle, fragmentShader);

			GL.BindAttribLocation(_mProgramHandle, 0, "vPosition");
			GL.LinkProgram(_mProgramHandle);

			GL.Viewport(0, 0, _viewportWidth, _viewportHeight);

			GL.UseProgram(_mProgramHandle);

			_initGl = true;
		}

		public static uint CompileShader(string shaderString, ShaderType shaderType)
		{
			uint shaderHandle = (uint)GL.CreateShader(shaderType);
			GL.ShaderSource((int)shaderHandle, shaderString);
			GL.CompileShader(shaderHandle);

			return shaderHandle;
		}

		public static void UniformMatrix4(int location, Matrix4 value)
		{
			GL.UniformMatrix4(location, 1, false, ref value.Row0.X);
		}

		void Render()
		{
			GL.UseProgram(_mProgramHandle);

			GL.ClearColor(0.7f, 0.7f, 0.7f, 1);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

			float aspectRatio = ((float)Width) / ((float)Height);
			float ratio = ((float)_viewportWidth) / ((float)_viewportHeight);

			_mProjectionMatrix = Matrix4.CreateOrthographicOffCenter(-ratio, ratio, -1, 1, 0.1f, 10.0f);

			_mViewMatrix = Matrix4.LookAt(new Vector3(0, 0, 5), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

			_mModelViewProjectionMatrix = Matrix4.Mult(_mViewMatrix, _mProjectionMatrix);

			Matrix4 mModel = Matrix4.CreateRotationY((float)(Math.PI * 2.0 * ReferenceTime.GetTimeFromReferenceMs() / 5000.0));

			_mModelViewProjectionMatrix = Matrix4.Mult(_mModelViewProjectionMatrix, mModel);

			_mPositionHandle = GL.GetAttribLocation(_mProgramHandle, "vPosition");

			_mColorHandle = GL.GetAttribLocation(_mProgramHandle, "vColor");

			GL.EnableVertexAttribArray(_mPositionHandle);
			GL.EnableVertexAttribArray(_mColorHandle);

			unsafe
			{
				fixed (Vector4* pvertices = _vertices)
				{
					GL.VertexAttribPointer(_mPositionHandle, Vector4.SizeInBytes / 4, VertexAttribPointerType.Float, false, 0, new IntPtr(pvertices));
				}

				fixed (Vector4* pcolors = _colors)
				{
					GL.VertexAttribPointer(_mColorHandle, Vector4.SizeInBytes / 4, VertexAttribPointerType.Float, false, 0, new IntPtr(pcolors));
				}
			}

			_mMVPMatrixHandle = GL.GetUniformLocation(_mProgramHandle, "uMVPMatrix");

			UniformMatrix4(_mMVPMatrixHandle, _mModelViewProjectionMatrix);

#pragma warning disable 0618
			GL.DrawArrays(BeginMode.Triangles, 0, 3);
#pragma warning restore 0618
			GL.Finish();
			GL.DisableVertexAttribArray(_mPositionHandle);
			GL.DisableVertexAttribArray(_mColorHandle);
		}
	}

	public class ReferenceTime
	{
		private static DateTime reference_time;
		private static bool reference_time_set = false;

		public static double GetTimeFromReferenceMs()
		{
			if (!reference_time_set)
			{
				reference_time = DateTime.Now;
				reference_time_set = true;

				return 0.0;
			}

			DateTime actual_time = DateTime.Now;
			TimeSpan ts = new TimeSpan(actual_time.Ticks - reference_time.Ticks);

			return ts.TotalMilliseconds;
		}
	}
}
#endif