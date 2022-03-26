using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.UnitTests
{
	class ButtonStub : View, IButton, ITextButton, IImageButton
	{
		public string Text { get; set; }

		public Color TextColor { get; set; }

		public Color StrokeColor { get; set; }

		public double StrokeThickness { get; set; }

		public int CornerRadius { get; set; }

		public double CharacterSpacing { get; set; }

		public Thickness Padding { get; set; }

		public LineBreakMode LineBreakMode { get; set; }

		public void Clicked() { }

		public void Pressed() { }

		public void Released() { }

		public Font Font { get; set; }

		public IImageSource ImageSource { get; set; }
		Aspect IImage.Aspect => Aspect.Fill;

		bool IImage.IsOpaque => true;

		IImageSource IImageSourcePart.Source => ImageSource;

		bool IImageSourcePart.IsAnimationPlaying => false;

		void IImageSourcePart.UpdateIsLoading(bool isLoading) { }

		public void ImageSourceLoaded()
		{
		}
	}
}