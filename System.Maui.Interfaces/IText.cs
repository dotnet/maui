using Xamarin.Forms;

namespace System.Maui
{
	public interface IText : IView
	{
		string Text { get; }

		TextType TextType { get; }
		//TODO: Add fonts and Colors
		Color Color { get; }
	}
}
