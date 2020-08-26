using Xamarin.Forms;

namespace System.Maui
{
	public interface IEditor : ITextInput
	{
		public EditorAutoSizeOption AutoSize { get; }

		void Completed();
	}
}