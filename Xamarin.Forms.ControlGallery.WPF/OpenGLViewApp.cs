using System;
using OpenTK.Graphics.OpenGL;

namespace Xamarin.Forms.ControlGallery.WPF
{
    public class OpenGLViewApp : Application
    {
        public OpenGLViewApp()
        {
            MainPage = new OpenGLViewSample();
        }
    }

    public class OpenGLViewSample : ContentPage
    {
        private bool _initGl = false;
        private int _viewportWidth;
        private int _viewportHeight;
        private uint _mProgramHandle;
        private OpenGLView _openGLView = null;

        public OpenGLViewSample()
        {
            Title = "OpenGLView Sample";

            var titleLabel = new Label
            {
                Text = "OpenGLView",
                FontSize = 36
            };

            _openGLView = new OpenGLView
            {
                HeightRequest = 300,
                WidthRequest = 300,
                HasRenderLoop = false
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
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Color3(1.0f, 0.85f, 0.35f);
            GL.Begin(BeginMode.Triangles);
            GL.Vertex3(0.0, 0.6, 0.0);
            GL.Vertex3(-0.2, -0.3, 0.0);
            GL.Vertex3(0.2, -0.3, 0.0);
            GL.End();
            GL.Flush();
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