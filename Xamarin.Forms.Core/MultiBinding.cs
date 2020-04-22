using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty(nameof(Bindings))]
	public sealed class MultiBinding : BindingBase
	{
		IMultiValueConverter _converter;
		object _converterParameter;
		BindingExpression _expression;
		IList<BindingBase> _bindings;
		BindableProperty _targetProperty;
		bool _isApplying;
		bool _isCreating;
		bool _hasSuccessfullyConverted;

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

		internal List<MultiBindingProxy> SourceProxies { get; private set; }

		internal MultiBindingProxy TargetProxy { get; private set; }

		internal override BindingBase Clone()
		{
			return new MultiBinding()
			{
				Converter = Converter,
				ConverterParameter = ConverterParameter,
				Bindings = new Collection<BindingBase>(this.Bindings),
				FallbackValue = FallbackValue,
				Mode = Mode,
				TargetNullValue = TargetNullValue,
				StringFormat = StringFormat,
			};
		}

		internal override void Apply(bool fromTarget)
		{
			VerifyConverterBeforeApply();

			base.Apply(fromTarget);		

			if (_hasSuccessfullyConverted && this.GetRealizedMode(_targetProperty) == BindingMode.OneTime)
				return;

			if (_expression == null)
				_expression = new BindingExpression(this, Binding.SelfPath);

			if (fromTarget && _isApplying)
				return;

			_expression.Apply(fromTarget);			
			ApplyBindingProxyValues(fromTarget ? TargetProxy : null, false);			
		}

		internal override void Apply(
			object context, 
			BindableObject bindObj, 
			BindableProperty targetProperty, 
			bool fromBindingContextChanged = false)
		{
			VerifyConverterBeforeApply();

			base.Apply(context, bindObj, targetProperty, fromBindingContextChanged);

			if (IsApplied && fromBindingContextChanged)
			{
				bool childContextChanged = false;
				foreach (var proxy in SourceProxies)
				{
					if (!object.ReferenceEquals(proxy.BindingContext, context))
					{
						childContextChanged = true;
						proxy.SetValueSilent(Element.BindingContextProperty, context);
					}
				}
				if ( childContextChanged )
					ApplyBindingProxyValues(null, true);
				return;
			}
		
			_targetProperty = targetProperty;
			
			if (_expression == null)
				_expression = new BindingExpression(this, nameof(MultiBindingProxy.Value));

			CreateBindingProxies(bindObj, context);

			_isApplying = true;			
			try
			{
				ApplyBindingProxyValues(TargetProxy, reapplyExpression: false, firstApplication: true);
				_expression.Apply(TargetProxy, bindObj, targetProperty);
				if (this.GetRealizedMode(_targetProperty) == BindingMode.OneWayToSource &&
					 this.FallbackValue != null)
					bindObj.SetValue(targetProperty, this.FallbackValue);
			}
			finally
			{
				_isApplying = false;
			}
		}

		internal override void Unapply(bool fromBindingContextChanged = false)
		{
			if (fromBindingContextChanged && IsApplied)
				return;

			base.Unapply(fromBindingContextChanged: fromBindingContextChanged);

			if (_expression != null)
				_expression.Unapply();

			TargetProxy.RemoveBinding(MultiBindingProxy.ValueProperty);
			if (this.SourceProxies?.Count > 0)
			{
				foreach (var proxy in this.SourceProxies)
					proxy.RemoveBinding(MultiBindingProxy.ValueProperty);
			}

			TargetProxy = null;
			SourceProxies = null;
		}

		internal void ApplyBindingProxyValues(MultiBindingProxy trigger, bool reapplyExpression = true, bool firstApplication = false)
		{
			if (_isCreating)
				return;

			BindingMode mode = this.GetRealizedMode(_targetProperty);
			bool convertBackFailed = false;
			if (trigger?.IsTarget == true && 
				!trigger.SuspendValueChangeNotification &&
				(!firstApplication || mode == BindingMode.OneWayToSource) &&
				(mode == BindingMode.TwoWay || mode == BindingMode.OneWayToSource))
			{
				// triggered because the target property was updated
				convertBackFailed = !ApplyTargetValueUpdate();
			}

			if (mode != BindingMode.OneWayToSource && 
				!convertBackFailed &&
				(trigger?.IsTarget != true || !TargetProxy.SuspendValueChangeNotification))
			{
				object newTargetValue = BindableProperty.UnsetValue;
				if (this.Converter != null)
				{
					newTargetValue = this.Converter.Convert(
							SourceProxies.Select(p => p.Value).ToArray(),
							_targetProperty.ReturnType,
							this.ConverterParameter,
							CultureInfo.CurrentUICulture);
					if (newTargetValue == Binding.DoNothing)
						return;
				}

				if (newTargetValue == BindableProperty.UnsetValue)
					newTargetValue = this.FallbackValue ?? _targetProperty.DefaultValue;
				else if (newTargetValue == null)
					newTargetValue = this.TargetNullValue ?? _targetProperty.DefaultValue;
				else
					_hasSuccessfullyConverted = true;
					
				TargetProxy.SetValueSilent(MultiBindingProxy.ValueProperty, newTargetValue);										
			}

			if (reapplyExpression)
			{
				bool wasApplying = _isApplying;
				_isApplying = true;
				_expression.Apply();
				_isApplying = wasApplying;
			}
			return;
		}

		bool ApplyTargetValueUpdate()
		{
			var types = SourceProxies
				.Select(p => p.Value?.GetType() ?? _targetProperty.ReturnType)
				.ToArray();
			var convertedValues = this.Converter?.ConvertBack(
				TargetProxy.Value,
				types,
				this.ConverterParameter,
				CultureInfo.CurrentUICulture);

			if (convertedValues == null || convertedValues.Any(val=>object.ReferenceEquals(val, BindableProperty.UnsetValue)))
			{
				// https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.imultivalueconverter.convertback?view=netframework-4.8
				// Return null to indicate that the converter cannot perform the 
				// conversion or that it does not support conversion in this direction.		
				// Return DependencyProperty.UnsetValue at position i to indicate that 
				// the converter is unable to provide a value for the source binding at 
				// index i, and that no value is to be set on it.
				return false;
			}

			int count = Math.Min(convertedValues.Length, this.SourceProxies.Count);
			for (int i = 0; i < count; i++)
			{
				// https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.imultivalueconverter.convertback?view=netframework-4.8
				// Return DoNothing at position i to indicate that no value is to 
				// be set on the source binding at index i.
				if (convertedValues[i] == Binding.DoNothing)
					continue;

				var childMode = this.SourceProxies[i].RealizedMode;
				if (childMode != BindingMode.TwoWay && childMode != BindingMode.OneWayToSource)
					continue;
				this.SourceProxies[i].SetValueSilent(MultiBindingProxy.ValueProperty, convertedValues[i]);
			}
			return true;
		}

		void CreateBindingProxies(BindableObject target, object context)
		{
			_hasSuccessfullyConverted = false;
			_isCreating = true;
			try
			{
				TargetProxy = new MultiBindingProxy(this, true);
				SourceProxies = new List<MultiBindingProxy>();

				if (this.Bindings.Count == 0)
					return;

				var mode = this.GetRealizedMode(_targetProperty);

				if (mode == BindingMode.OneWayToSource)
					TargetProxy.Value = this.FallbackValue;

				foreach (var binding in _bindings)
				{
					var proxy = new MultiBindingProxy(this, false);
					proxy.BindingContext = context;

					// Bind proxy's Value property using the source binding settings
					var proxyBinding = binding.Clone();

					// Ensures that RelativeSource bindings resolve using the
					// MultiBinding's BindableObject target rather than the proxy.
					if (target is MultiBindingProxy)
						proxyBinding.RelativeSourceTargetOverride = this.RelativeSourceTargetOverride;
					else if (target is Element e)
						proxyBinding.RelativeSourceTargetOverride = e;

					// OneWayToSource, OneTime, or OneWay mode on the MultiBinding effectively
					// override the childrens' modes 
					if ( mode == BindingMode.OneWayToSource ||
						 mode == BindingMode.OneTime ||
						 mode == BindingMode.OneWay ||
						 proxyBinding.Mode == BindingMode.Default)
						proxyBinding.Mode = mode;
					
					proxy.SetBinding(MultiBindingProxy.ValueProperty, proxyBinding);
					proxy.RealizedMode = proxyBinding.Mode;
					SourceProxies.Add(proxy);
				}
			}
			finally
			{
				_isCreating = false;
			}
		}

		void VerifyConverterBeforeApply()
		{
			if (this.Converter == null)
				throw new InvalidOperationException($"{nameof(MultiBinding)} requires {nameof(Converter)} be set.");
		}
	}
}
