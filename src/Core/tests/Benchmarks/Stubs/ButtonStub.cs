using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	class ButtonStub : StubBase, IButton
	{
		public ButtonStub()
		{

		}

		public ButtonStub(string text)
		{
			Text = text;
		}

		public string Text { get; set; }

		public Color TextColor { get; set; }

		public double CharacterSpacing { get; set; }

		public Font Font { get; set; }

		public Thickness Padding { get; set; }

		public event EventHandler Pressed;
		public event EventHandler Released;
		public event EventHandler Clicked;

		void IButton.Pressed() => Pressed?.Invoke(this, EventArgs.Empty);
		void IButton.Released() => Released?.Invoke(this, EventArgs.Empty);
		void IButton.Clicked() => Clicked?.Invoke(this, EventArgs.Empty);
	}
}