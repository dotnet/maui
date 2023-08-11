#nullable disable
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/MultiBinding.xml" path="Type[@FullName='Microsoft.Maui.Controls.MultiBinding']/Docs/*" />
	[ContentProperty(nameof(Bindings))]
	public sealed class MultiBinding : BindingBase
	{
		IMultiValueConverter _converter;
		object _converterParameter;
		IList<BindingBase> _bindings;
		BindableProperty _targetProperty;
		BindableObject _targetObject;
		BindableObject _proxyObject;
		BindableProperty[] _bpProxies;
		bool _applying;

		/// <include file="../../docs/Microsoft.Maui.Controls/MultiBinding.xml" path="//Member[@MemberName='Converter']/Docs/*" />
		public IMultiValueConverter Converter
		{
			get { return _converter; }
			set
			{
				ThrowIfApplied();
				_converter = value;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MultiBinding.xml" path="//Member[@MemberName='ConverterParameter']/Docs/*" />
		public object ConverterParameter
		{
			get { return _converterParameter; }
			set
			{
				ThrowIfApplied();
				_converterParameter = value;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/MultiBinding.xml" path="//Member[@MemberName='Bindings']/Docs/*" />
		public IList<BindingBase> Bindings
		{
			get => _bindings ?? (_bindings = new List<BindingBase>());
			set
			{
				ThrowIfApplied();
				_bindings = value;
			}
		}

		internal override BindingBase Clone()
		{
			var bindingsclone = new List<BindingBase>(Bindings.Count);
			foreach (var b in Bindings)
				bindingsclone.Add(b.Clone());

			return new MultiBinding()
			{
				Converter = Converter,
				ConverterParameter = ConverterParameter,
				Bindings = bindingsclone,
				FallbackValue = FallbackValue,
				Mode = Mode,
				TargetNullValue = TargetNullValue,
				StringFormat = StringFormat,
			};
		}

		internal override void Apply(bool fromTarget)
		{
			if (_applying)
				return;

			base.Apply(fromTarget);

			if (this.GetRealizedMode(_targetProperty) == BindingMode.OneTime)
				return;

			if (fromTarget && this.GetRealizedMode(_targetProperty) == BindingMode.OneWay)
				return;

			if (!fromTarget && this.GetRealizedMode(_targetProperty) == BindingMode.OneWayToSource)
				return;

			if (!fromTarget)
			{
				var value = GetSourceValue(GetValueArray(), _targetProperty.ReturnType);
				if (value != Binding.DoNothing)
				{
					_applying = true;
					if (!BindingExpression.TryConvert(ref value, _targetProperty, _targetProperty.ReturnType, true))
					{
						BindingDiagnostics.SendBindingFailure(this, null, _targetObject, _targetProperty, "MultiBinding", BindingExpression.CannotConvertTypeErrorMessage, value, _targetProperty.ReturnType);
						return;
					}
					_targetObject.SetValueCore(_targetProperty, value, SetValueFlags.ClearDynamicResource, BindableObject.SetValuePrivateFlags.Default | BindableObject.SetValuePrivateFlags.Converted, specificity: SetterSpecificity.FromBinding);
					_applying = false;
				}
			}
			else
			{
				try
				{
					_applying = true;

					//https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.imultivalueconverter.convertback?view=netframework-4.8#remarks
					if (!(GetTargetValue(_targetObject.GetValue(_targetProperty), null) is object[] values)) //converter failed
						return;
					for (var i = 0; i < Math.Min(_bpProxies.Length, values.Length); i++)
					{
						if (ReferenceEquals(values[i], Binding.DoNothing) || ReferenceEquals(values[i], BindableProperty.UnsetValue))
							continue;
						_proxyObject.SetValue(_bpProxies[i], values[i]);
					}
				}
				finally
				{
					_applying = false;
				}

			}
		}

		internal override void Apply(object context, BindableObject targetObject, BindableProperty targetProperty, bool fromBindingContextChanged, SetterSpecificity specificity)
		{
			if (_bindings == null)
				throw new InvalidOperationException("Bindings is null");

			if (Converter == null && StringFormat == null)
				throw new InvalidOperationException("Cannot apply MultiBinding because both Converter and StringFormat are null.");

			base.Apply(context, targetObject, targetProperty, fromBindingContextChanged, specificity);

			if (!ReferenceEquals(_targetObject, targetObject))
			{
				_targetObject = targetObject;
				_proxyObject = new ProxyElement() { Parent = targetObject as Element };
				_targetProperty = targetProperty;

				if (_bpProxies == null)
				{
					_bpProxies = new BindableProperty[Bindings.Count];
					_applying = true;
					var bindingMode = Mode == BindingMode.Default ? targetProperty.DefaultBindingMode : Mode;
					for (var i = 0; i < Bindings.Count; i++)
					{
						var binding = Bindings[i];
						binding.RelativeSourceTargetOverride = targetObject as Element;
						var bp = _bpProxies[i] = BindableProperty.Create($"mb-proxy{i}", typeof(object), typeof(MultiBinding), null, bindingMode, propertyChanged: OnBindingChanged);
						_proxyObject.SetBinding(bp, binding);
					}
					_applying = false;
				}
			}
			_proxyObject.BindingContext = context;

			if (this.GetRealizedMode(_targetProperty) == BindingMode.OneWayToSource)
				return;

			var value = GetSourceValue(GetValueArray(), _targetProperty.ReturnType);
			if (value != Binding.DoNothing)
			{
				_applying = true;
				if (!BindingExpression.TryConvert(ref value, _targetProperty, _targetProperty.ReturnType, true))
				{
					BindingDiagnostics.SendBindingFailure(this, context, _targetObject, _targetProperty, "MultiBinding", BindingExpression.CannotConvertTypeErrorMessage, value, _targetProperty.ReturnType);
					return;
				}
				_targetObject.SetValueCore(_targetProperty, value, SetValueFlags.ClearDynamicResource, BindableObject.SetValuePrivateFlags.Default | BindableObject.SetValuePrivateFlags.Converted, specificity);
				_applying = false;
			}
		}

		class ProxyElement : Element
		{
		}

		object[] GetValueArray()
		{
			var valuearray = new object[_bpProxies.Length];
			for (var i = 0; i < _bpProxies.Length; i++)
				valuearray[i] = _proxyObject.GetValue(_bpProxies[i]);
			return valuearray;
		}

		internal override object GetSourceValue(object value, Type targetPropertyType)
		{
			var valuearray = value as object[];
			if (valuearray != null && Converter != null)
				value = Converter.Convert(valuearray, targetPropertyType, ConverterParameter, CultureInfo.CurrentUICulture);

			if (valuearray != null && Converter == null && StringFormat != null && BindingBase.TryFormat(StringFormat, valuearray, out var formatted))
				return formatted;

			if (ReferenceEquals(BindableProperty.UnsetValue, value))
				return FallbackValue;

			return base.GetSourceValue(value, targetPropertyType);
		}

		internal override object GetTargetValue(object value, Type sourcePropertyType)
		{
			if (Converter != null)
			{
				var values = GetValueArray();
				var types = new Type[_bpProxies.Length];
				for (var i = 0; i < _bpProxies.Length; i++)
					types[i] = values[i]?.GetType() ?? typeof(object);
				return Converter.ConvertBack(value, types, ConverterParameter, CultureInfo.CurrentUICulture);
			}

			return base.GetTargetValue(value, sourcePropertyType);
		}

		void OnBindingChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!_applying)
				Apply(fromTarget: false);
		}

		internal override void Unapply(bool fromBindingContextChanged = false)
		{
			if (!fromBindingContextChanged)
			{
				if (_bpProxies != null && _proxyObject != null)
					foreach (var proxybp in _bpProxies)
						_proxyObject.RemoveBinding(proxybp);

				_bpProxies = null;
				_proxyObject = null;
			}

			base.Unapply(fromBindingContextChanged: fromBindingContextChanged);
		}
	}
}
