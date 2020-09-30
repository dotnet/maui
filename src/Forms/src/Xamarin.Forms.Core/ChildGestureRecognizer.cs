using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms
{
	public sealed class ChildGestureRecognizer : IGestureRecognizer
	{
		private IGestureRecognizer _gestureRecognizer;
		public IGestureRecognizer GestureRecognizer
		{
			get => _gestureRecognizer;
			set { _gestureRecognizer = value; OnPropertyChanged(); }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}