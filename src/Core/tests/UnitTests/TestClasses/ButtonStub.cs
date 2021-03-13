using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Tests
{
	class ButtonStub : View, IButton
	{
		public string Text { get; set; }

		public Color TextColor { get; set; }

		public double CharacterSpacing { get; set; }

		public Thickness Padding { get; set; }

		public void Clicked() { }

		public void Pressed() { }

		public void Released() { }

		public Font Font { get; set; }
	}
}