using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if HAVE_OPENTK
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
#endif

namespace Xamarin.Forms.Controls
{
#if HAVE_OPENTK
	public class OpenGLGallery : ContentPage
	{
		float red, green, blue;

		public OpenGLGallery ()
		{
			var view = new OpenGLView {HasRenderLoop = true};
			var toggle = new Switch {IsToggled = true};
			var button = new Button {Text = "Display"};

			view.HeightRequest = 300;
			view.WidthRequest = 300;

			view.OnDisplay = r =>{
				/*
				if (!inited) {
					var shader = GL.CreateShader (All.VertexShader);
					int length = 0;
					GL.ShaderSource (shader, 1, new string [] { "void main() { gl_FragColor = vec4(0.6, 1.0, 0.0, 1.0); }"}, ref length);
					GL.CompileShader (shader);
					int status = 0;
					GL.GetShader (shader, All.CompileStatus, ref status);
					if (status == 0) {
						GL.DeleteShader (shader);
						throw new Exception();
					}

					inited = true;
				}
				 */
				GL.ClearColor (red, green, blue, 1.0f);
				GL.Clear ((ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

				red += 0.01f;
				if (red >= 1.0f)
					red -= 1.0f;
				green += 0.02f;
				if (green >= 1.0f)
					green -= 1.0f;
				blue += 0.03f;
				if (blue >= 1.0f)
					blue -= 1.0f;
			};

			toggle.Toggled += (s, a) => { view.HasRenderLoop = toggle.IsToggled; };
			button.Activated += (s, a) => view.Display ();

			var stack = new StackLayout {Padding = new Size (20, 20)};
			stack.Add (view);
			stack.Add (toggle);
			stack.Add (button);

			Content = stack;
		}
	}
#endif
}
