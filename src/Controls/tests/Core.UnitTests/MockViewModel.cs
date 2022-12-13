using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class MockViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public MockViewModel(string text = null, string @object = null)
		{
			_text = text;

			_object = @object;
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

		object _object;
		public virtual object Object
		{
			get { return _object; }
			set
			{
				if (_object == value)
					return;

				_object = value;
				OnPropertyChanged("Object");
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}