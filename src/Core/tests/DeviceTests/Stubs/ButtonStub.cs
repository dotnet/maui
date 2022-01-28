using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class ButtonStub : StubBase, IButton, ITextButton, IImageButton
	{
		public string Text { get; set; }

		public Color TextColor { get; set; }

		public Color StrokeColor { get; set; }

		public double StrokeThickness { get; set; }

		public int CornerRadius { get; set; }

		public double CharacterSpacing { get; set; }

		public Font Font { get; set; }

		public Thickness Padding { get; set; }

		public IImageSource ImageSource { get; set; }

		Aspect IImage.Aspect => Aspect.Fill;

		bool IImage.IsOpaque => true;

		IImageSource IImageSourcePart.Source => ImageSource;

		bool IImageSourcePart.IsAnimationPlaying => false;

		void IImageSourcePart.UpdateIsLoading(bool isLoading) { }

		public event EventHandler Pressed;
		public event EventHandler Released;
		public event EventHandler Clicked;

		void IButton.Pressed() => Pressed?.Invoke(this, EventArgs.Empty);
		void IButton.Released() => Released?.Invoke(this, EventArgs.Empty);
		void IButton.Clicked() => Clicked?.Invoke(this, EventArgs.Empty);

		public void ImageSourceLoaded()
		{
		}
	}
}