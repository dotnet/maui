#nullable disable
using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace Microsoft.Maui.Controls
{
	class AppThemeBinding : BindingBase
	{
		public const string AppThemeResource = "__MAUI_ApplicationTheme__";
		class AppThemeProxy : Element
		{
			public AppThemeProxy(Element parent, AppThemeBinding binding)
			{
				_parent = parent;
				Binding = binding;
				this.SetDynamicResource(AppThemeProperty, AppThemeResource);
				((IElementDefinition)parent)?.AddResourcesChangedListener(OnParentResourcesChanged);
			}

			public static BindableProperty AppThemeProperty = BindableProperty.Create("AppTheme", typeof(AppTheme), typeof(AppThemeBinding), AppTheme.Unspecified,
				propertyChanged: (bindable, oldValue, newValue) => ((AppThemeProxy)bindable).OnAppThemeChanged());
			readonly Element _parent;

			public void OnAppThemeChanged()
			{
				Binding.Apply(false);
			}

			public AppThemeBinding Binding { get; }

			public void Unsubscribe()
			{
				((IElementDefinition)_parent)?.RemoveResourcesChangedListener(OnParentResourcesChanged);
			}
		}

		WeakReference<BindableObject> _weakTarget;
		BindableProperty _targetProperty;
		SetterSpecificity specificity;
		AppThemeProxy _appThemeProxy;

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
		}

		internal override void Apply(object context, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged, SetterSpecificity specificity)
		{
			_weakTarget = new WeakReference<BindableObject>(bindObj);
			_targetProperty = targetProperty;
			base.Apply(context, bindObj, targetProperty, fromBindingContextChanged, specificity);
			this.specificity = specificity;
			ApplyCore(false);
			_appThemeProxy = new AppThemeProxy(bindObj as Element, this);

		}

		internal override void Unapply(bool fromBindingContextChanged = false)
		{
			_appThemeProxy?.Unsubscribe();
			_appThemeProxy = null;

			base.Unapply(fromBindingContextChanged);
			_weakTarget = null;
			_targetProperty = null;
		}

		void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
			=> ApplyCore(true);

		void ApplyCore(bool dispatch = false)
		{
			if (_weakTarget == null || !_weakTarget.TryGetTarget(out var target))
			{
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

		// Ideally this will get reworked to not use `Application.Current` at all
		// https://github.com/dotnet/maui/issues/8713
		// But I'm going with a simple nudge for now so that we can get our 
		// device tests back to a working state and address issues
		// of the more crashing variety
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
