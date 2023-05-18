#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ChildGestureRecognizer.xml" path="Type[@FullName='Microsoft.Maui.Controls.ChildGestureRecognizer']/Docs/*" />
	public sealed class ChildGestureRecognizer : IGestureRecognizer
	{
		private IGestureRecognizer _gestureRecognizer;
		/// <include file="../../docs/Microsoft.Maui.Controls/ChildGestureRecognizer.xml" path="//Member[@MemberName='GestureRecognizer']/Docs/*" />
		public IGestureRecognizer GestureRecognizer
		{
			get => _gestureRecognizer;
			set { _gestureRecognizer = value; OnPropertyChanged(); }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		/// <include file="../../docs/Microsoft.Maui.Controls/ChildGestureRecognizer.xml" path="//Member[@MemberName='OnPropertyChanged']/Docs/*" />
		public void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}