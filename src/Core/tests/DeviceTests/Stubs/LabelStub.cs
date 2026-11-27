using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class LabelStub : StubBase, ILabel
	{
		public string Text { get; set; }

		public TextType TextType { get; set; } = TextType.Text;

		public Color TextColor { get; set; }

		public double CharacterSpacing { get; set; }

		public Thickness Padding { get; set; }

		public Font Font { get; set; }

		public TextAlignment HorizontalTextAlignment { get; set; }

		public TextAlignment VerticalTextAlignment { get; set; }

		public TextDecorations TextDecorations { get; set; }

		public double LineHeight { get; set; } = -1;
	}
}
