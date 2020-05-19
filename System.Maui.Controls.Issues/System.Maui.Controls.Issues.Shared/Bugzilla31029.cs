using System;
using System.IO;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 31029, "[Windows Phone 8.1]Generating an Image via MemoryStream does not appear")]
	public class Bugzilla31029 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{
			var generatedImage = new Image { Aspect = Aspect.AspectFit };

			var btn = new Button { Text="generate" };

			btn.Clicked += (sender, e) => {
				var source =  GenerateBmp (60, 60, Color.Red);
				generatedImage.Source = source;

			};

			Content = new StackLayout {
				Children = {
						btn,
#pragma warning disable 618
                    new Label {Text = "GeneratedImage", Font=Font.BoldSystemFontOfSize(NamedSize.Medium)},
#pragma warning restore 618
                    generatedImage
                },
				Padding = new Thickness (0, 20, 0, 0),
				VerticalOptions = LayoutOptions.StartAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
		}
		public ImageSource GenerateBmp (int rows, int cols, Color color)
		{
			BmpMaker bmpMaker = new BmpMaker (rows, cols);
			//background color to white
			for (int i = 0; i < rows; i++) {
				for (int j = 0; j < cols; j++) {
					bmpMaker.SetPixel (i, j, Color.White);
				}
			}
			//draw a square
			int marginX = rows / 10;
			int marginY = cols / 10;
			for (int row = marginX; row < (rows - marginX); row++) {
				for (int col = marginY; col < (cols - marginY); col++) {
					bmpMaker.SetPixel (row, col, color);
				}
			}
			ImageSource resultImage = bmpMaker.Generate ();
			return resultImage;
		}
	}

	public class BmpMaker
	{
		const int HeaderSize = 54;
		readonly byte[] _buffer;
		public BmpMaker (int width, int height)
		{
			Width = width;
			Height = height;
			int numPixels = Width * Height;
			int numPixelBytes = 4 * numPixels;
			int fileSize = HeaderSize + numPixelBytes;
			_buffer = new byte[fileSize];
			// Write headers in MemoryStream and hence the buffer. 
			using (MemoryStream memoryStream = new MemoryStream (_buffer)) {
				using (BinaryWriter writer = new BinaryWriter (memoryStream, Encoding.UTF8)) {
					// Construct BMP header (14 bytes). 
					writer.Write (new char[] { 'B', 'M' }); // Signature 
					writer.Write (fileSize); // File size 
					writer.Write ((short) 0); // Reserved 
					writer.Write ((short) 0); // Reserved 
					writer.Write (HeaderSize); // Offset to pixels 
					// Construct BitmapInfoHeader (40 bytes). 
					writer.Write (40); // Header size 
					writer.Write (Width); // Pixel width 
					writer.Write (Height); // Pixel height 
					writer.Write ((short) 1); // Planes 
					writer.Write ((short) 32); // Bits per pixel 
					writer.Write (0); // Compression 
					writer.Write (numPixelBytes); // Image size in bytes 
					writer.Write (0); // X pixels per meter 
					writer.Write (0); // Y pixels per meter 
					writer.Write (0); // Number colors in color table 
					writer.Write (0); // Important color count 
				}
			}
		}

		public int Width { get; private set; }
		public int Height { get; private set; }

		public void SetPixel (int row, int col, Color color)
		{
			SetPixel (row, col, (int) (255 * color.R),
				(int) (255 * color.G),
				(int) (255 * color.B),
				(int) (255 * color.A));
		}

		public void SetPixel (int row, int col, int r, int g, int b, int a = 255)
		{
			int index = (row * Width + col) * 4 + HeaderSize;
			_buffer[index + 0] = (byte) b;
			_buffer[index + 1] = (byte) g;
			_buffer[index + 2] = (byte) r;
			_buffer[index + 3] = (byte) a;
		}

		public ImageSource Generate ()
		{
			Stream memoryStream = new MemoryStream (_buffer);
			ImageSource imageSource = ImageSource.FromStream (() => { return memoryStream; });
			return imageSource;
		}
	}
}
