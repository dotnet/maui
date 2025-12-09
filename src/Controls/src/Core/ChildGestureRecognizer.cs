#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls
{
	/// <summary>A gesture recognizer for use as a child of another.</summary>
	public sealed class ChildGestureRecognizer : IGestureRecognizer
	{
		private IGestureRecognizer _gestureRecognizer;
		/// <summary>Gets or sets the recognizer.</summary>
		public IGestureRecognizer GestureRecognizer
		{
			get => _gestureRecognizer;
			set { _gestureRecognizer = value; OnPropertyChanged(); }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>Method that is called when the recognizer is changed.</summary>
		/// <param name="propertyName">The property that changed.</param>
		public void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}