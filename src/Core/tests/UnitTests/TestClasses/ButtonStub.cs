using Microsoft.Maui.Controls;

namespace Microsoft.Maui.UnitTests
{
	class ButtonStub : View, IButton
	{
		public string Text { get; set; }

		public Color TextColor { get; set; }

		public double CharacterSpacing { get; set; }
		public TextTransform TextTransform { get; set; }

		public Thickness Padding { get; set; }

		public void Clicked() { }

		public void Pressed() { }

		public void Released() { }

		public Font Font { get; set; }
	}
}