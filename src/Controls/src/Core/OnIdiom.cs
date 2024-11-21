#nullable disable
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls
{
	public class OnIdiom<T> : IWrappedValue
	{
		T _phone;
		T _tablet;
		T _desktop;
		T _tV;
		T _watch;
		T _default;
		bool _isPhoneSet;
		bool _isTabletSet;
		bool _isDesktopSet;
		bool _isTVSet;
		bool _isWatchSet;
		bool _isDefaultSet;

		public T Phone
		{
			get => _phone;
			set
			{
				_phone = value;
				_isPhoneSet = true;
			}
		}
		public T Tablet
		{
			get => _tablet;
			set
			{
				_tablet = value;
				_isTabletSet = true;
			}
		}
		public T Desktop
		{
			get => _desktop;
			set
			{
				_desktop = value;
				_isDesktopSet = true;
			}
		}
		public T TV
		{
			get => _tV;
			set
			{
				_tV = value;
				_isTVSet = true;
			}
		}
		public T Watch
		{
			get => _watch;
			set
			{
				_watch = value;
				_isWatchSet = true;
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

		public static implicit operator T(OnIdiom<T> onIdiom)
		{
			var idiom = DeviceInfo.Idiom;
			if (idiom == DeviceIdiom.Tablet)
				return onIdiom._isTabletSet ? onIdiom.Tablet : (onIdiom._isDefaultSet ? onIdiom.Default : default(T));
			else if (idiom == DeviceIdiom.Desktop)
				return onIdiom._isDesktopSet ? onIdiom.Desktop : (onIdiom._isDefaultSet ? onIdiom.Default : default(T));
			else if (idiom == DeviceIdiom.TV)
				return onIdiom._isTVSet ? onIdiom.TV : (onIdiom._isDefaultSet ? onIdiom.Default : default(T));
			else if (idiom == DeviceIdiom.Watch)
				return onIdiom._isWatchSet ? onIdiom.Watch : (onIdiom._isDefaultSet ? onIdiom.Default : default(T));
			else
				return onIdiom._isPhoneSet ? onIdiom.Phone : (onIdiom._isDefaultSet ? onIdiom.Default : default(T));
		}

		object IWrappedValue.Value => (T)this;
		System.Type IWrappedValue.ValueType => typeof(T);
	}
}
