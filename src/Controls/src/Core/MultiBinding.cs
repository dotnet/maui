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
		private bool _proxyBindingWasUpdated;

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

			this.ApplyCore( fromTarget );
		}

		internal override void Apply(object context, BindableObject targetObject, BindableProperty targetProperty, bool fromBindingContextChanged = false)
		{
			if (_bindings == null)
				throw new InvalidOperationException("Bindings is null");

			if (Converter == null && StringFormat == null)
				throw new InvalidOperationException("Cannot apply MultiBinding because both Converter and StringFormat are null.");

			context = Context ?? context;

			base.Apply(context, targetObject, targetProperty, fromBindingContextChanged);

			_applying = true;
			if (!ReferenceEquals(_targetObject, targetObject))
			{
				_proxyBindingWasUpdated = true;
				_targetObject = targetObject;
				_proxyObject = new ProxyElement() { Parent = targetObject as Element };
				_targetProperty = targetProperty;

				if (_bpProxies == null)
				{
					_bpProxies = new BindableProperty[Bindings.Count];
					var bindingMode = Mode == BindingMode.Default ? targetProperty.DefaultBindingMode : Mode;
					for (var i = 0; i < Bindings.Count; i++)
					{
						var binding = Bindings[i];
						binding.RelativeSourceTargetOverride = targetObject as Element;
						var bp = _bpProxies[i] = BindableProperty.Create($"mb-proxy{i}", typeof(object), typeof(MultiBinding), null, bindingMode, propertyChanged: OnBindingChanged);
						_proxyObject.SetBinding(bp, binding);
					}
				}
			}
			else
			{
				// watch for updated bindings to avoid unnecessary target update.
				_proxyBindingWasUpdated = false;
				_proxyObject.BindingContext = context;
			}
			_applying = false;

			if (this.GetRealizedMode(_targetProperty) == BindingMode.OneWayToSource || !_proxyBindingWasUpdated )
				return;
			
			ApplyCore();
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

		internal override object GetSourceValue(object value, BindableObject bindObj, BindableProperty targetProperty)
		{
			var valuearray = value as object[];
			if ( valuearray != null && Converter != null )
			{
				value = Converter.Convert(valuearray, targetProperty.ReturnType, ConverterParameter, CultureInfo.CurrentUICulture);

				if (ReferenceEquals(value, BindableProperty.UnsetValue))
				{
					return FallbackValue ?? targetProperty.GetDefaultValue(bindObj);
				}

				if (ReferenceEquals(value, Binding.DoNothing))
					return value;
			}

			if (valuearray != null && Converter == null && StringFormat != null && TryFormat(StringFormat, valuearray, out var formatted))
				return formatted;

			return base.GetSourceValue(value, bindObj, targetProperty);
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

		void ApplyCore(bool fromTarget = false )
		{
			if (!fromTarget)
			{
				var value = GetSourceValue(GetValueArray(), _targetObject, _targetProperty);

				if (ReferenceEquals(value, Binding.DoNothing))
					return;

				_applying = true;
				if (!BindingExpression.TryConvert(ref value, _targetProperty, _targetProperty.ReturnType, true))
				{
					BindingDiagnostics.SendBindingFailure(this, null, _targetObject, _targetProperty, "MultiBinding", BindingExpression.CannotConvertTypeErrorMessage, value, _targetProperty.ReturnType);
					return;
				}
				_targetObject.SetValueCore(_targetProperty, value, SetValueFlags.ClearDynamicResource, BindableObject.SetValuePrivateFlags.Default | BindableObject.SetValuePrivateFlags.Converted);
				_applying = false;
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

		void OnBindingChanged(BindableObject bindable, object oldValue, object newValue)
		{
			_proxyBindingWasUpdated = true;

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
