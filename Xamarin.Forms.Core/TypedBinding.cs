#define DO_NOT_CHECK_FOR_BINDING_REUSE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Internals
{
	//FIXME: need a better name for this, and share with Binding, so we can share more unittests
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class TypedBindingBase : BindingBase
	{
		IValueConverter _converter;
		object _converterParameter;
		object _source;
		string _updateSourceEventName;

		public IValueConverter Converter
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

		[Obsolete("deprecated one. kept for backcompat")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TypedBinding(Func<TSource, TProperty> getter, Action<TSource, TProperty> setter, Tuple<Func<TSource, object>, string>[] handlers)
				: this(s => (getter(s), true), setter, handlers)
		{
			if (getter == null)
				throw new ArgumentNullException(nameof(getter));
		}

		public TypedBinding(Func<TSource, (TProperty value, bool success)> getter, Action<TSource, TProperty> setter, Tuple<Func<TSource, object>, string>[] handlers)
		{
			_getter = getter ?? throw new ArgumentNullException(nameof(getter));
			_setter = setter;

			if (handlers == null)
				return;

			_handlers = new PropertyChangedProxy[handlers.Length];
			for (var i = 0; i < handlers.Length; i++)
				_handlers[i] = new PropertyChangedProxy(handlers[i].Item1, handlers[i].Item2, this);
		}

		readonly WeakReference<object> _weakSource = new WeakReference<object>(null);
		readonly WeakReference<BindableObject> _weakTarget = new WeakReference<BindableObject>(null);
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
				ApplyCore(source, target, _targetProperty, fromTarget);
		}

		// Applies the binding to a new source or target.
		internal override void Apply(object context, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged = false)
		{
			_targetProperty = targetProperty;
			var source = Source ?? Context ?? context;
			var isApplied = IsApplied;

			if (Source != null && isApplied && fromBindingContextChanged)
				return;

			base.Apply(source, bindObj, targetProperty, fromBindingContextChanged);

#if (!DO_NOT_CHECK_FOR_BINDING_REUSE)
			BindableObject prevTarget;
			if (_weakTarget.TryGetTarget(out prevTarget) && !ReferenceEquals(prevTarget, bindObj))
				throw new InvalidOperationException("Binding instances can not be reused");

			object previousSource;
			if (_weakSource.TryGetTarget(out previousSource) && !ReferenceEquals(previousSource, source))
				throw new InvalidOperationException("Binding instances can not be reused");
#endif
			_weakSource.SetTarget(source);
			_weakTarget.SetTarget(bindObj);

			ApplyCore(source, bindObj, targetProperty);
		}

		internal override BindingBase Clone()
		{
			Tuple<Func<TSource, object>, string>[] handlers = _handlers == null ? null : new Tuple<Func<TSource, object>, string>[_handlers.Length];
			if (handlers != null)
			{
				for (var i = 0; i < _handlers.Length; i++)
					handlers[i] = new Tuple<Func<TSource, object>, string>(_handlers[i].PartGetter, _handlers[i].PropertyName);
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
		internal void ApplyCore(object sourceObject, BindableObject target, BindableProperty property, bool fromTarget = false)
		{
			var isTSource = sourceObject != null && sourceObject is TSource;
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
					Log.Warning("Binding", "'{0}' can not be converted to type '{1}'.", value, property.ReturnType);
					return;
				}
				target.SetValueCore(property, value, SetValueFlags.ClearDynamicResource, BindableObject.SetValuePrivateFlags.Default | BindableObject.SetValuePrivateFlags.Converted);
				return;
			}

			var needsSetter = (mode == BindingMode.TwoWay && fromTarget) || mode == BindingMode.OneWayToSource;
			if (needsSetter && _setter != null && isTSource)
			{
				var value = GetTargetValue(target.GetValue(property), typeof(TProperty));
				if (!BindingExpression.TryConvert(ref value, property, typeof(TProperty), false))
				{
					Log.Warning("Binding", "'{0}' can not be converted to type '{1}'.", value, typeof(TProperty));
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
			WeakReference<INotifyPropertyChanged> _weakPart = new WeakReference<INotifyPropertyChanged>(null);
			readonly BindingBase _binding;
			PropertyChangedEventHandler handler;
			public INotifyPropertyChanged Part
			{
				get
				{
					INotifyPropertyChanged target;
					if (_weakPart.TryGetTarget(out target))
						return target;
					return null;
				}
				set
				{
					if (Listener != null && Listener.Source.TryGetTarget(out var source) && ReferenceEquals(value, source))
						//Already subscribed
						return;

					//clear out previous subscription
					Listener?.Unsubscribe();

					_weakPart.SetTarget(value);
					Listener.SubscribeTo(value, handler);
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
				dispatcher.Dispatch(() => _binding.Apply(false));
			}
		}

		void Subscribe(TSource sourceObject)
		{
			for (var i = 0; i < _handlers.Length; i++)
			{
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
				_handlers[i].Listener.Unsubscribe();
		}
	}
}