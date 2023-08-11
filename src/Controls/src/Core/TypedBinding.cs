#nullable disable
#define DO_NOT_CHECK_FOR_BINDING_REUSE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls.Internals
{
	//FIXME: need a better name for this, and share with Binding, so we can share more unittests
	/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TypedBindingBase.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.TypedBindingBase']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class TypedBindingBase : BindingBase
	{
		IValueConverter _converter;
		object _converterParameter;
		object _source;
		string _updateSourceEventName;

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TypedBindingBase.xml" path="//Member[@MemberName='Converter']/Docs/*" />
		public IValueConverter Converter
		{
			get { return _converter; }
			set
			{
				ThrowIfApplied();
				_converter = value;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TypedBindingBase.xml" path="//Member[@MemberName='ConverterParameter']/Docs/*" />
		public object ConverterParameter
		{
			get { return _converterParameter; }
			set
			{
				ThrowIfApplied();
				_converterParameter = value;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TypedBindingBase.xml" path="//Member[@MemberName='Source']/Docs/*" />
		public object Source
		{
			get { return _source; }
			set
			{
				ThrowIfApplied();
				_source = value;
			}
		}

		internal string UpdateSourceEventName
		{
			get { return _updateSourceEventName; }
			set
			{
				ThrowIfApplied();
				_updateSourceEventName = value;
			}
		}

		internal TypedBindingBase()
		{
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class TypedBinding<TSource, TProperty> : TypedBindingBase
	{
		readonly Func<TSource, (TProperty value, bool success)> _getter;
		readonly Action<TSource, TProperty> _setter;
		readonly PropertyChangedProxy[] _handlers;

		public TypedBinding(Func<TSource, (TProperty value, bool success)> getter, Action<TSource, TProperty> setter, Tuple<Func<TSource, object>, string>[] handlers)
		{
			_getter = getter ?? throw new ArgumentNullException(nameof(getter));
			_setter = setter;

			if (handlers == null)
				return;

			_handlers = new PropertyChangedProxy[handlers.Length];
			for (var i = 0; i < handlers.Length; i++)
			{
				if (handlers[i] is null)
					continue;
				_handlers[i] = new PropertyChangedProxy(handlers[i].Item1, handlers[i].Item2, this);
			}
		}

		readonly WeakReference<object> _weakSource = new WeakReference<object>(null);
		readonly WeakReference<BindableObject> _weakTarget = new WeakReference<BindableObject>(null);
		SetterSpecificity _specificity;
		BindableProperty _targetProperty;

		// Applies the binding to a previously set source and target.
		internal override void Apply(bool fromTarget = false)
		{
			base.Apply(fromTarget);

			BindableObject target;
#if DO_NOT_CHECK_FOR_BINDING_REUSE
			if (!_weakTarget.TryGetTarget(out target))
				return;
#else
			if (!_weakTarget.TryGetTarget(out target) || target == null) {
				Unapply();
				return;
			}
#endif
			object source;
			if (_weakSource.TryGetTarget(out source) && source != null)
				ApplyCore(source, target, _targetProperty, fromTarget, _specificity);
		}

		// Applies the binding to a new source or target.
		internal override void Apply(object context, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged, SetterSpecificity specificity)
		{
			_targetProperty = targetProperty;
			this._specificity = specificity;
			var source = Source ?? Context ?? context;
			var isApplied = IsApplied;

			if (Source != null && isApplied && fromBindingContextChanged)
				return;

			base.Apply(source, bindObj, targetProperty, fromBindingContextChanged, specificity);

#if (!DO_NOT_CHECK_FOR_BINDING_REUSE)
			BindableObject prevTarget;
			if (_weakTarget.TryGetTarget(out prevTarget) && !ReferenceEquals(prevTarget, bindObj))
				throw new InvalidOperationException("Binding instances cannot be reused");

			object previousSource;
			if (_weakSource.TryGetTarget(out previousSource) && !ReferenceEquals(previousSource, source))
				throw new InvalidOperationException("Binding instances cannot be reused");
#endif
			_weakSource.SetTarget(source);
			_weakTarget.SetTarget(bindObj);

			ApplyCore(source, bindObj, targetProperty, false, specificity);
		}

		internal override BindingBase Clone()
		{
			Tuple<Func<TSource, object>, string>[] handlers = _handlers == null ? null : new Tuple<Func<TSource, object>, string>[_handlers.Length];
			if (handlers != null)
			{
				for (var i = 0; i < _handlers.Length; i++)
				{
					if (_handlers[i] == null)
						continue;
					handlers[i] = new Tuple<Func<TSource, object>, string>(_handlers[i].PartGetter, _handlers[i].PropertyName);
				}
			}
			return new TypedBinding<TSource, TProperty>(_getter, _setter, handlers)
			{
				Mode = Mode,
				Converter = Converter,
				ConverterParameter = ConverterParameter,
				StringFormat = StringFormat,
				Source = Source,
				UpdateSourceEventName = UpdateSourceEventName,
			};
		}

		internal override object GetSourceValue(object value, Type targetPropertyType)
		{
			if (Converter != null)
				value = Converter.Convert(value, targetPropertyType, ConverterParameter, CultureInfo.CurrentUICulture);

			return base.GetSourceValue(value, targetPropertyType);
		}

		internal override object GetTargetValue(object value, Type sourcePropertyType)
		{
			if (Converter != null)
				value = Converter.ConvertBack(value, sourcePropertyType, ConverterParameter, CultureInfo.CurrentUICulture);

			//return base.GetTargetValue(value, sourcePropertyType);
			return value;
		}

		internal override void Unapply(bool fromBindingContextChanged = false)
		{
			if (Source != null && fromBindingContextChanged && IsApplied)
				return;

#if (!DO_NOT_CHECK_FOR_BINDING_REUSE)
			base.Unapply(fromBindingContextChanged:fromBindingContextChanged);
#endif
			if (_handlers != null)
				Unsubscribe();

#if (!DO_NOT_CHECK_FOR_BINDING_REUSE)
			_weakSource.SetTarget(null);
			_weakTarget.SetTarget(null);
#endif
		}

		// ApplyCore is as slim as it should be:
		// Setting  100000 values						: 17ms.
		// ApplyCore  100000 (w/o INPC, w/o unnapply)	: 20ms.
		internal void ApplyCore(object sourceObject, BindableObject target, BindableProperty property, bool fromTarget, SetterSpecificity specificity)
		{
			var isTSource = sourceObject is TSource;
			var mode = this.GetRealizedMode(property);
			if ((mode == BindingMode.OneWay || mode == BindingMode.OneTime) && fromTarget)
				return;

			var needsGetter = (mode == BindingMode.TwoWay && !fromTarget) || mode == BindingMode.OneWay || mode == BindingMode.OneTime;

			if (isTSource && (mode == BindingMode.OneWay || mode == BindingMode.TwoWay) && _handlers != null)
				Subscribe((TSource)sourceObject);

			if (needsGetter)
			{
				var value = FallbackValue ?? property.GetDefaultValue(target);
				if (isTSource)
				{
					try
					{
						(var retval, bool success) = _getter((TSource)sourceObject);
						if (success) //if the getter failed, return the FallbackValue
							value = GetSourceValue(retval, property.ReturnType);
					}
					catch (Exception ex) when (ex is NullReferenceException || ex is KeyNotFoundException || ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException)
					{
					}
				}
				if (!BindingExpression.TryConvert(ref value, property, property.ReturnType, true))
				{
					BindingDiagnostics.SendBindingFailure(this, sourceObject, target, property, "Binding", BindingExpression.CannotConvertTypeErrorMessage, value, property.ReturnType);
					return;
				}
				target.SetValueCore(property, value, SetValueFlags.ClearDynamicResource, BindableObject.SetValuePrivateFlags.Default | BindableObject.SetValuePrivateFlags.Converted, specificity);
				return;
			}

			var needsSetter = (mode == BindingMode.TwoWay && fromTarget) || mode == BindingMode.OneWayToSource;
			if (needsSetter && _setter != null && isTSource)
			{
				var value = GetTargetValue(target.GetValue(property), typeof(TProperty));
				if (!BindingExpression.TryConvert(ref value, property, typeof(TProperty), false))
				{
					BindingDiagnostics.SendBindingFailure(this, sourceObject, target, property, "Binding", BindingExpression.CannotConvertTypeErrorMessage, value, typeof(TProperty));
					return;
				}
				_setter((TSource)sourceObject, (TProperty)value);
			}
		}

		class PropertyChangedProxy
		{
			public Func<TSource, object> PartGetter { get; }
			public string PropertyName { get; }
			public BindingExpression.WeakPropertyChangedProxy Listener { get; }
			readonly BindingBase _binding;
			PropertyChangedEventHandler handler;

			~PropertyChangedProxy() => Listener?.Unsubscribe();

			public INotifyPropertyChanged Part
			{
				get
				{
					if (Listener != null && Listener.TryGetSource(out var target))
						return target;
					return null;
				}
				set
				{
					if (Listener != null)
					{
						//Already subscribed
						if (Listener.TryGetSource(out var source) && ReferenceEquals(value, source))
							return;

						//clear out previous subscription
						Listener.Unsubscribe();
						Listener.Subscribe(value, handler);
					}
				}
			}

			public PropertyChangedProxy(Func<TSource, object> partGetter, string propertyName, BindingBase binding)
			{
				PartGetter = partGetter;
				PropertyName = propertyName;
				_binding = binding;
				Listener = new BindingExpression.WeakPropertyChangedProxy();
				//avoid GC collection, keep a ref to the OnPropertyChanged handler
				handler = new PropertyChangedEventHandler(OnPropertyChanged);
			}

			void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (!string.IsNullOrEmpty(e.PropertyName) && string.CompareOrdinal(e.PropertyName, PropertyName) != 0)
					return;

				IDispatcher dispatcher = (sender as BindableObject)?.Dispatcher;
				dispatcher.DispatchIfRequired(() => _binding.Apply(false));
			}
		}

		void Subscribe(TSource sourceObject)
		{
			for (var i = 0; i < _handlers.Length; i++)
			{
				if (_handlers[i] == null)
					continue;
				var part = _handlers[i].PartGetter(sourceObject);
				if (part == null)
					break;
				var inpc = part as INotifyPropertyChanged;
				if (inpc == null)
					continue;
				_handlers[i].Part = (inpc);
			}
		}

		void Unsubscribe()
		{
			for (var i = 0; i < _handlers.Length; i++)
				_handlers[i]?.Listener.Unsubscribe();
		}
	}
}
