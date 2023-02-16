#nullable disable
using System;
using System.ComponentModel;

using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Controls
{
	class AppThemeBinding : BindingBase
	{
		WeakReference<BindableObject> _weakTarget;

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

			AttachEvents();
		}

		internal override void Apply(object context, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged = false)
		{
			Target = bindObj;
			TargetProperty = targetProperty;
			base.Apply(context, bindObj, targetProperty, fromBindingContextChanged);
			ApplyCore();

			AttachEvents();
		}

		internal override void Unapply(bool fromBindingContextChanged = false)
		{
			DetachEvents();

			base.Unapply(fromBindingContextChanged);
			TargetProperty = null;
			Target = null;
		}

		void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
			=> ApplyCore(true);

		void OnApplicationChanging(object sender, ParentChangingEventArgs e)
		{
			if (Target is VisualElement { Window.Parent: Application app })
				app.RequestedThemeChanged -= OnRequestedThemeChanged;
		}

		void OnApplicationChanged(object sender, EventArgs e)
		{
			if (Target is VisualElement { Window.Parent: Application app })
				app.RequestedThemeChanged += OnRequestedThemeChanged;

			ApplyCore(true);
		}

		void OnWindowChanging(object sender, PropertyChangingEventArgs e)
		{
			if (string.Equals(e.PropertyName, VisualElement.WindowProperty.PropertyName, StringComparison.Ordinal))
				if (Target is VisualElement { Window: { } window })
				{
					if (window.Parent is Application app)
						app.RequestedThemeChanged -= OnRequestedThemeChanged;

					window.ParentChanging -= OnApplicationChanging;
					window.ParentChanged -= OnApplicationChanged;
				}
		}

		void OnWindowChanged(object sender, PropertyChangedEventArgs e)
		{
			if (string.Equals(e.PropertyName, VisualElement.WindowProperty.PropertyName, StringComparison.Ordinal))
			{
				if (Target is VisualElement { Window: { } window })
				{
					window.ParentChanging += OnApplicationChanging;
					window.ParentChanged += OnApplicationChanged;
					if (window.Parent is Application app)
						app.RequestedThemeChanged += OnRequestedThemeChanged;
				}

				ApplyCore(true);
			}
		}

		void ApplyCore(bool dispatch = false)
		{
			BindableObject target = Target;
			if (target is null)
			{
				DetachEvents();
				return;
			}

			BindableProperty targetProperty = TargetProperty;
			if (targetProperty is null)
				return;

			if (dispatch)
				target.Dispatcher.DispatchIfRequired(Set);
			else
				Set();

			void Set() =>
			target.SetValueCore(targetProperty, GetValue());
		}

		object _light;
		object _dark;
		bool _isLightSet;
		bool _isDarkSet;

		BindableObject Target
		{
			get =>
			_weakTarget.TryGetTarget(out BindableObject target)
			? target
			: null;
			set
			{
				DetachEvents();

				_weakTarget = new WeakReference<BindableObject>(value);

				AttachEvents();
			}
		}

		BindableProperty TargetProperty { get; set; }

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

		AppTheme GetCurrentTheme()
		{
			return Target is VisualElement { Window.Parent: Application appFromTarget }
				   ? appFromTarget.RequestedTheme
				   : Application.Current is { } appFromStatic
				   ? appFromStatic.RequestedTheme
				   : AppInfo.RequestedTheme;
		}

		// Ideally this will get reworked to not use `Application.Current` at all
		// https://github.com/dotnet/maui/issues/8713
		// But I'm going with a simple nudge for now so that we can get our
		// device tests back to a working state and address issues
		// of the more crashing variety
		object GetValue()
		{
			return GetCurrentTheme() switch
			{
				AppTheme.Dark => _isDarkSet
								 ? Dark
								 : Default,
				_ => _isLightSet
					 ? Light
					 : Default
			};
		}

		void AttachEvents()
		{
			DetachEvents();

			if (Target is VisualElement newVisualElementTarget)
			{
				newVisualElementTarget.PropertyChanging += OnWindowChanging;
				newVisualElementTarget.PropertyChanged += OnWindowChanged;
				if (newVisualElementTarget.Window is { } newWindow)
				{
					newWindow.ParentChanging += OnApplicationChanging;
					newWindow.ParentChanged += OnApplicationChanged;
					if (newWindow.Parent is Application newApplication)
						newApplication.RequestedThemeChanged += OnRequestedThemeChanged;
				}
			}
		}

		void DetachEvents()
		{
			if (Target is VisualElement oldVisualElementTarget)
			{
				if (oldVisualElementTarget.Window is { } oldWindow)
				{
					if (oldWindow.Parent is Application oldApplication)
						oldApplication.RequestedThemeChanged -= OnRequestedThemeChanged;

					oldWindow.ParentChanging -= OnApplicationChanging;
					oldWindow.ParentChanged -= OnApplicationChanged;
				}

				oldVisualElementTarget.PropertyChanging -= OnWindowChanging;
				oldVisualElementTarget.PropertyChanged -= OnWindowChanged;
			}
		}
	}
}