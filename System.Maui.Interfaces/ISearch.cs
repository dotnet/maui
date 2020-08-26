using Xamarin.Forms;

namespace System.Maui
{
	public interface ISearch : ITextInput
	{
		void Search();
		void Cancel();
		Color CancelColor { get; }
	}
}
