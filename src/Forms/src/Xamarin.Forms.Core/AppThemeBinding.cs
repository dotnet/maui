using System;

namespace Xamarin.Forms
{
	class AppThemeBinding : BindingBase
	{
		WeakReference<BindableObject> _weakTarget;
		BindableProperty _targetProperty;

		public AppThemeBinding() => Application.Current.RequestedThemeChanged += (o, e) => Device.BeginInvokeOnMainThread(() => ApplyCore());

		internal override BindingBase Clone() => new AppThemeBinding
		{
			Light = Light,
			_isLightSet = _isLightSet,
			Dark = Dark,
			_isDarkSet = _isDarkSet,
			Default = Default
		};

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

		internal override void Unapply(bool fromBindingContextChanged = false)
		{
			base.Unapply(fromBindingContextChanged);
			_weakTarget = null;
			_targetProperty = null;
		}

		void ApplyCore()
		{
			if (_weakTarget == null || !_weakTarget.TryGetTarget(out var target))
				return;

			target?.SetValueCore(_targetProperty, GetValue());
		}

		object _light;
		object _dark;
		bool _isLightSet;
		bool _isDarkSet;

		public object Light
		{
			get => _light;
			set
			{
				_light = value;
				_isLightSet = true;
			}
		}

		public object Dark
		{
			get => _dark;
			set
			{
				_dark = value;
				_isDarkSet = true;
			}
		}

		public object Default { get; set; }

		object GetValue()
		{
			switch (Application.Current.RequestedTheme)
			{
				default:
				case OSAppTheme.Light:
					return _isLightSet ? Light : Default;
				case OSAppTheme.Dark:
					return _isDarkSet ? Dark : Default;
			}
		}
	}
}