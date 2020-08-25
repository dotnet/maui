using System;
using Xamarin.Forms;

namespace System.Maui.Boring.Android
{
	public class BoringEntry : BoringView, ITextInput
	{
		public int MaxLength { get; set; }

		public string Placeholder { get; set; }

		public Color PlaceholderColor { get; set; }

		public string Text { get; set; }

		public TextTransform TextTransform { get; set; }

		public TextType TextType { get; set; }

		public Color Color { get; set; }

		string IText.Text => Text;

		public string UpdateFormsText(string text, TextTransform textTransform)
		{
			return text;
		}
	}
}
