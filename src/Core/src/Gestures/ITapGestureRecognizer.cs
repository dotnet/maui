using System.Windows.Input;

namespace Microsoft.Maui
{
	public interface ITapGestureRecognizer : IGestureRecognizer
	{
		public ICommand Command { get; set; }

		public object CommandParameter { get; set; }

		public int NumberOfTapsRequired { get; set; }

		void Tapped(IView view);
	}
}