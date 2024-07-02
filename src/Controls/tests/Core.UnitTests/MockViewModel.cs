using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class MockViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public MockViewModel(string text = null)
		{
			_text = text;
		}

		string _text;
		public virtual string Text
		{
			get { return _text; }
			set
			{
				if (_text == value)
					return;

				_text = value;
				OnPropertyChanged("Text");
			}
		}

		int _value;
		public virtual int Value
		{
			get { return _value; }
			set
			{
				if (_value == value)
					return;

				_value = value;
				OnPropertyChanged("Value");
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}