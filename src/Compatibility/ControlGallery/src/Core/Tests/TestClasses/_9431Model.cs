using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Tests
{
	public class _9431Model : System.ComponentModel.INotifyPropertyChanged
	{
		Color _bGColor;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}

		public Color BGColor
		{
			get => _bGColor;
			set
			{
				_bGColor = value;
				OnPropertyChanged(nameof(BGColor));
			}
		}
	}
}
