
using Xamarin.Forms;

namespace System.Maui
{
	public interface ITextInput : IText
	{
		int MaxLength { get; }
		string Placeholder { get; }
		Color PlaceholderColor { get; }
		new string Text { get; set; }
		TextTransform TextTransform { get; }

		string UpdateFormsText(string text, TextTransform textTransform);
		//string IText.Text => Text;
	}
}