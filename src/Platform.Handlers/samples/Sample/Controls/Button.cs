using System;
using Xamarin.Forms;
using Xamarin.Platform;

namespace Sample
{
	public class Button : Xamarin.Forms.View, IButton
	{
		public string Text { get; set; }
		public Color TextColor { get; set; }

		public FontAttributes FontAttributes => throw new NotImplementedException();
		public string FontFamily => throw new NotImplementedException();
		public double FontSize => throw new NotImplementedException();

		public Action Pressed { get; set; }
		public Action Released { get; set; }
		public Action Clicked { get; set; }

		void IButton.Pressed() => Pressed?.Invoke();
		void IButton.Released() => Released?.Invoke();
		void IButton.Clicked() => Clicked?.Invoke();

		public new double Width
		{
			get { return WidthRequest; }
			set { WidthRequest = value; }
		}

		public new double Height
		{
			get { return HeightRequest; }
			set { HeightRequest = value; }
		}
	}
}