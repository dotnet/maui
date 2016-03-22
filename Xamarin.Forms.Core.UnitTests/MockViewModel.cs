using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms.Core.UnitTests
{
	internal class MockViewModel
		: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string text;
		public virtual string Text {
			get { return text; }
			set {
				if (text == value)
					return;

				text = value;
				OnPropertyChanged ("Text");
			}
		}

		protected void OnPropertyChanged ([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}
