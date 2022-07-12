using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Controls
{
	class AppThemeBinding : BindingBase
	{
		WeakReference<BindableObject> _weakTarget;
		BindableProperty _targetProperty;

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

			if (Application.Current != null)
				Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
		}

		internal override void Unapply(bool fromBindingContextChanged = false)
		{
			base.Unapply(fromBindingContextChanged);
			_weakTarget = null;
			_targetProperty = null;

			if (Application.Current != null)
				Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;
		}

		void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
			=> ApplyCore(true);

		void ApplyCore(bool dispatch = false)
		{
			if (_weakTarget == null || !_weakTarget.TryGetTarget(out var target))
				return;

			if (dispatch)
				target.Dispatcher.DispatchIfRequired(Set);
			else
				Set();

			void Set() => target.SetValueCore(_targetProperty, GetValue());
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
			Application app;

			if (_weakTarget?.TryGetTarget(out var target) == true &&
				target is VisualElement ve &&
				ve?.Window?.Parent is Application a)
			{
				app = a;
			}
			else
			{
				app = Application.Current;
			}

			AppTheme appTheme;
			if (app == null)
				appTheme = AppInfo.RequestedTheme;
			else
				appTheme = app.RequestedTheme;

			return appTheme switch
			{
				AppTheme.Dark => _isDarkSet ? Dark : Default,
				_ => _isLightSet ? Light : Default,
			};
		}
	}
}