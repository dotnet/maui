using System;
using Xamarin.Platform;

namespace Xamarin.Forms
{
	public partial class Button : IButton
	{
		public TextAlignment HorizontalTextAlignment => throw new NotImplementedException();

		public TextAlignment VerticalTextAlignment => throw new NotImplementedException();

		Color IText.Color
		{
			get => this.TextColor;
		}

		void IButton.Clicked()
		{
			throw new NotImplementedException();
		}

		void IButton.Pressed()
		{
			throw new NotImplementedException();
		}

		void IButton.Released()
		{
			throw new NotImplementedException();
		}
	}
}