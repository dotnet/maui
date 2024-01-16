#nullable disable
using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace Microsoft.Maui.Controls
{
	class AppThemeBinding : BindingBase
	{
		WeakReference<BindableObject> _weakTarget;
		BindableProperty _targetProperty;
		bool _attached;
		SetterSpecificity specificity;

		internal override BindingBase Clone()
		{
			var clone = new AppThemeBinding
			{
				Light = Light,
				_isLightSet = _isLightSet,
				Dark = Dark,
				_isDarkSet = _isDarkSet,
				Default = Default
			};

			if (DebuggerHelper.DebuggerIsAttached && VisualDiagnostics.GetSourceInfo(this) is SourceInfo info)
				VisualDiagnostics.RegisterSourceInfo(clone, info.SourceUri, info.LineNumber, info.LinePosition);

			return clone;
		}

		internal override void Apply(bool fromTarget)
		{
			base.Apply(fromTarget);
			ApplyCore();
			SetAttached(true);
		}

		internal override void Apply(object context, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged, SetterSpecificity specificity)
		{
			_weakTarget = new WeakReference<BindableObject>(bindObj);
			_targetProperty = targetProperty;
			base.Apply(context, bindObj, targetProperty, fromBindingContextChanged, specificity);
			this.specificity = specificity;
			ApplyCore(false);
			SetAttached(true);
		}

		internal override void Unapply(bool fromBindingContextChanged = false)
		{
			SetAttached(false);
			base.Unapply(fromBindingContextChanged);
			_weakTarget = null;
			_targetProperty = null;
		}

		void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
			=> ApplyCore(true);

		void OnRequestedThemeChanged(object sender, EventArgs e)
			=> ApplyCore(true);

		void ApplyCore(bool dispatch = false)
		{
			if (_weakTarget == null || !_weakTarget.TryGetTarget(out var target))
			{
				SetAttached(false);
				return;
			}

			if (dispatch)
				target.Dispatcher.DispatchIfRequired(Set);
			else
				Set();

			void Set()
			{
				var value = GetValue();
				if (value is DynamicResource dynamicResource)
					target.SetDynamicResource(_targetProperty, dynamicResource.Key, specificity);
				else
				{
					if (!BindingExpression.TryConvert(ref value, _targetProperty, _targetProperty.ReturnType, true))
					{
						BindingDiagnostics.SendBindingFailure(this, null, target, _targetProperty, "AppThemeBinding", BindingExpression.CannotConvertTypeErrorMessage, value, _targetProperty.ReturnType);
						return;
					}
					target.SetValueCore(_targetProperty, value, Internals.SetValueFlags.ClearDynamicResource, BindableObject.SetValuePrivateFlags.Default | BindableObject.SetValuePrivateFlags.Converted, specificity);
				}
			};
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
			// First, try use the theme from the target VisualElement because that is the fastest
			// way to get the theme. If this element is attached to the UI, then it will have been
			// set by the parent VisualElement, Window or Application.
			var appTheme = AppTheme.Unspecified;
			if (_weakTarget?.TryGetTarget(out var target) == true && target is VisualElement ve)
				appTheme = ve.RequestedTheme;

			// If there is no VisualElement (OR no theme set because it is not attached to the UI),
			// then try the current app. If that fails, just ask the OS for the current theme.
			if (appTheme == AppTheme.Unspecified)
				appTheme = Application.Current?.RequestedTheme ?? AppInfo.RequestedTheme;

			return appTheme switch
			{
				AppTheme.Dark => _isDarkSet ? Dark : Default,
				_ => _isLightSet ? Light : Default,
			};
		}

		void SetAttached(bool value)
		{
			if (_attached == value)
				return;

			_attached = value;

			if (_weakTarget?.TryGetTarget(out var target) == true && target is VisualElement ve)
			{
				// Use the VisualElement as this is faster than Application.RequestedThemeChanged
				// by a significant margin: https://github.com/dotnet/maui/issues/18505
				//
				// We do "lose" a feature where all bindings would listen to the (at the time
				// of first Apply) the current application. However, this has the drawback where
				// the list of subscribers grows so large it takes too long to add/remove handlers.
				//
				// This logic here does also have a strong reference to the target object when
				// applied, however this does not appear to be a problem in my tests. I also
				// tested with making the _weakTarget field be a normal reference and still
				// did not leak.

				if (value)
					ve.RequestedThemeChanged += OnRequestedThemeChanged;
				else
					ve.RequestedThemeChanged -= OnRequestedThemeChanged;
			}
			else
			{
				// Fall back to the app for listening to theme changes if the target is not a
				// VisualElement.

				// This is still bad, but the work to make all the various elements respond to
				// the theme propagation is too much work for this PR. Instead, things like
				// Shell and MenuItems will subscribe directly to the app. The main issue is
				// for thing like scrolling a list view or pages with large numbers of controls.

				var app = Application.Current;
				if (value)
					app.RequestedThemeChanged += OnRequestedThemeChanged;
				else
					app.RequestedThemeChanged -= OnRequestedThemeChanged;
			}
		}
	}
}
