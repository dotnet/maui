using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
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

		public IMultiValueConverter Converter
		{
			get { return _converter; }
			set
			{
				ThrowIfApplied();
				_converter = value;
			}
		}

		public object ConverterParameter
		{
			get { return _converterParameter; }
			set
			{
				ThrowIfApplied();
				_converterParameter = value;
			}
		}

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
						Log.Warning("MultiBinding", "'{0}' can not be converted to type '{1}'.", value, _targetProperty.ReturnType);
						return;
					}
					_targetObject.SetValueCore(_targetProperty, value, SetValueFlags.ClearDynamicResource, BindableObject.SetValuePrivateFlags.Default | BindableObject.SetValuePrivateFlags.Converted);
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
						_proxyObject.SetValueCore(_bpProxies[i], values[i], SetValueFlags.None);
					}
				}
				finally
				{
					_applying = false;
				}

			}
		}

		internal override void Apply(object context, BindableObject targetObject, BindableProperty targetProperty, bool fromBindingContextChanged = false)
		{
			if (_bindings == null)
				throw new InvalidOperationException("Bindings is null");

			if (Converter == null && StringFormat == null)
				throw new InvalidOperationException("Cannot apply MultiBinding because both Converter and StringFormat are null.");

			base.Apply(context, targetObject, targetProperty, fromBindingContextChanged);

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
					Log.Warning("MultiBinding", "'{0}' can not be converted to type '{1}'.", value, _targetProperty.ReturnType);
					return;
				}
				_targetObject.SetValueCore(_targetProperty, value, SetValueFlags.ClearDynamicResource, BindableObject.SetValuePrivateFlags.Default | BindableObject.SetValuePrivateFlags.Converted);
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

			if (valuearray != null && StringFormat != null && TryFormat(StringFormat, valuearray, out var formatted))
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