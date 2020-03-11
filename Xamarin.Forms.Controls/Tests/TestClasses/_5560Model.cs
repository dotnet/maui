namespace Xamarin.Forms.Controls.Tests
{
	public class _5560Model : System.ComponentModel.INotifyPropertyChanged
	{
		string _text;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}

		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				OnPropertyChanged(nameof(Text));
			}
		}
	}
}
