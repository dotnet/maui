using System;

namespace System.Maui
{
	public class OnAppTheme<T> : BindingBase
	{
		WeakReference<BindableObject> _weakTarget;
		BindableProperty _targetProperty;
		
		public OnAppTheme()
		{
			Application.Current.RequestedThemeChanged += ThemeChanged;
		}

		void ThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			Device.BeginInvokeOnMainThread(() => ApplyCore());
		}
		public new BindingMode Mode
		{
			get => base.Mode;
			private set { }
		}
		internal override BindingBase Clone()
		{
			return new OnAppTheme<T> { Light = Light, Dark = Dark, Default = Default };
		}

		internal override void Apply(bool fromTarget)
		{
			base.Apply(fromTarget);
			ApplyCore();
		}

		internal override void Apply(object context, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged = false)
		{
			_weakTarget = new WeakReference<BindableObject>(bindObj);
			_targetProperty = targetProperty;
			base.Apply(context, bindObj, targetProperty, fromBindingContextChanged);
			ApplyCore();
		}
		void ApplyCore()
		{
			if (_weakTarget == null || !_weakTarget.TryGetTarget(out var target))
				return;

			target?.SetValueCore(_targetProperty, GetValue());
		}

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