using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms
{
	public class OnAppTheme<T> : INotifyPropertyChanged
	{
		T _light;
		T _dark;
		T _default;
		bool _isLightSet;
		bool _isDarkSet;
		bool _isDefaultSet;

		public T Light
		{
			get => _light;
			set
			{
				_light = value;
				_isLightSet = true;
			}
		}
		public T Dark
		{
			get => _dark;
			set
			{
				_dark = value;
				_isDarkSet = true;
			}
		}
		public T Default
		{
			get => _default;
			set
			{
				_default = value;
				_isDefaultSet = true;
			}
		}

		public static implicit operator T(OnAppTheme<T> onAppTheme)
		{
			return onAppTheme.GetValue();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public T Value => GetValue();

		T GetValue()
		{
			switch (Application.Current.RequestedTheme)
			{
				default:
				case OSAppTheme.Light:
					return _isLightSet ? Light : (_isDefaultSet ? Default : default);
				case OSAppTheme.Dark:
					return _isDarkSet ? Dark : (_isDefaultSet ? Default : default);
			}
		}
	}
}