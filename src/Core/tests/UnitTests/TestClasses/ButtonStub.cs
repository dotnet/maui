using Microsoft.Maui.Controls;
using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Tests
{
	class ButtonStub : View, IButton
	{
		public string Text => throw new NotImplementedException();

		public Color Color => throw new NotImplementedException();

		public Font Font => throw new NotImplementedException();

		public TextTransform TextTransform => throw new NotImplementedException();

		public double CharacterSpacing => throw new NotImplementedException();

		public FontAttributes FontAttributes => throw new NotImplementedException();

		public string FontFamily => throw new NotImplementedException();

		public double FontSize => throw new NotImplementedException();

		public TextAlignment HorizontalTextAlignment => throw new NotImplementedException();

		public TextAlignment VerticalTextAlignment => throw new NotImplementedException();

		public Color TextColor => throw new NotImplementedException();

		public void Clicked()
		{
			throw new NotImplementedException();
		}

		public void Pressed()
		{
			throw new NotImplementedException();
		}

		public void Released()
		{
			throw new NotImplementedException();
		}
	}
}
