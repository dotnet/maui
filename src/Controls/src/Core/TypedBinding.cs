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

		public string UpdateSourceEventName
		{
			internal get { return _updateSourceEventName; }
			set
			{
				ThrowIfApplied();
				_updateSourceEventName = value;
			}
		}

		internal TypedBindingBase()
		{
		}

		internal abstract void ApplyToResolvedSource(object sourceObject, BindableObject target, BindableProperty property, bool fromTarget, SetterSpecificity specificity);
		internal abstract void SubscribeToAncestryChanges(List<Element> chain, bool includeBindingContext, bool rootIsSource);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class TypedBinding<TSource, TProperty> : TypedBindingBase
	{
		readonly Func<TSource, (TProperty value, bool success)> _getter;
		readonly Action<TSource, TProperty> _setter;
		readonly IPropertyChangeHandler _propertyChangeHandler;

#nullable enable
#pragma warning disable RS0016 // Add public types and members to the declared API
		public TypedBinding(
#pragma warning restore RS0016 // Add public types and members to the declared API
			Func<TSource, (TProperty value, bool success)> getter,
			Action<TSource, TProperty>? setter,
			int handlersCount,
			Func<TSource, IEnumerable<ValueTuple<INotifyPropertyChanged?, string>>>? handlers)
		{
			_getter = getter ?? throw new ArgumentNullException(nameof(getter));
			_setter = setter;
			_propertyChangeHandler = new PropertyChangeHandler(this, handlersCount, handlers);
		}
#nullable disable

		// TODO make this constructor obsolete
		// [Obsolete("This constructor is obsolete. Please use the constructor that takes handlersCount and handlers function.")]
		public TypedBinding(
			Func<TSource, (TProperty value, bool success)> getter,
			Action<TSource, TProperty> setter,
			Tuple<Func<TSource, object>, string>[] handlers)
		{
			_getter = getter ?? throw new ArgumentNullException(nameof(getter));
			_setter = setter;
			_propertyChangeHandler = new LegacyPropertyChangeHandler(this, handlers);
		}

		readonly WeakReference<object> _weakSource = new WeakReference<object>(null);
		readonly WeakReference<BindableObject> _weakTarget = new WeakReference<BindableObject>(null);
		SetterSpecificity _specificity;
		BindableProperty _targetProperty;
		List<WeakReference<Element>> _ancestryChain;
		bool _isBindingContextRelativeSource;

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

			if (Source is RelativeBindingSource relativeSource)
			{
				var relativeSourceTarget = RelativeSourceTargetOverride ?? bindObj as Element;
				if (relativeSourceTarget is not Element)
				{
					var message = bindObj is not null
						? $"Cannot apply relative binding to {bindObj.GetType().FullName} because it is not a superclass of Element."
						: "Cannot apply relative binding when the target object is null.";

					throw new InvalidOperationException(message);
				}

				ApplyRelativeSourceBinding(relativeSource, relativeSourceTarget, bindObj, targetProperty, specificity);
			}
			else
			{
				ApplyToResolvedSource(source, bindObj, targetProperty, false, specificity);
			}
		}

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
		async void ApplyRelativeSourceBinding(
			RelativeBindingSource relativeSource, Element relativeSourceTarget, BindableObject targetObject, BindableProperty targetProperty, SetterSpecificity specificity)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
		{
			try
			{
				await relativeSource.Apply(this, relativeSourceTarget, targetObject, targetProperty, specificity);
			}
			catch (Exception ex)
			{
				BindingDiagnostics.SendBindingFailure(this, relativeSource, targetObject, targetProperty, "Binding", BindingExpression.ApplyingRelativeSourceBindingErrorMessage, relativeSource.Mode, ex.Message);
			}
		}

		internal override BindingBase Clone()
		{
			return _propertyChangeHandler.CloneBinding();
		}

		internal override void ApplyToResolvedSource(object source, BindableObject target, BindableProperty targetProperty, bool fromBindingContextChanged, SetterSpecificity specificity)
		{
#if (!DO_NOT_CHECK_FOR_BINDING_REUSE)
			BindableObject prevTarget;
			if (_weakTarget.TryGetTarget(out prevTarget) && !ReferenceEquals(prevTarget, target))
				throw new InvalidOperationException("Binding instances cannot be reused");

			object previousSource;
			if (_weakSource.TryGetTarget(out previousSource) && !ReferenceEquals(previousSource, source))
				throw new InvalidOperationException("Binding instances cannot be reused");
#endif
			_weakTarget.SetTarget(target);
			_weakSource.SetTarget(source);

			ApplyCore(source, target, targetProperty, fromBindingContextChanged, specificity);
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
			_propertyChangeHandler.Unsubscribe();

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
			if (!isTSource && sourceObject is not null)
			{
				BindingDiagnostics.SendBindingFailure(this, "Binding", $"Mismatch between the specified x:DataType ({typeof(TSource)}) and the current binding context ({sourceObject.GetType()}).");
			}

			var mode = this.GetRealizedMode(property);
			if ((mode == BindingMode.OneWay || mode == BindingMode.OneTime) && fromTarget)
				return;

			var needsGetter = (mode == BindingMode.TwoWay && !fromTarget) || mode == BindingMode.OneWay || mode == BindingMode.OneTime;

			if (isTSource && (mode == BindingMode.OneWay || mode == BindingMode.TwoWay))
				_propertyChangeHandler.Subscribe((TSource)sourceObject);

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
					catch (Exception ex)
					{
						BindingDiagnostics.SendBindingFailure(this, sourceObject, target, property, "Binding", $"Exception thrown from getter: {ex.Message}");
					}
				}
				if (!BindingExpressionHelper.TryConvert(ref value, property, property.ReturnType, true))
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
				if (!BindingExpressionHelper.TryConvert(ref value, property, typeof(TProperty), false))
				{
					BindingDiagnostics.SendBindingFailure(this, sourceObject, target, property, "Binding", BindingExpression.CannotConvertTypeErrorMessage, value, typeof(TProperty));
					return;
				}

				try
				{
					_setter((TSource)sourceObject, (TProperty)value);
				}
				catch (Exception ex) when (ex is NullReferenceException || ex is KeyNotFoundException || ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException)
				{
					// Ignore exceptions that are thrown when the source object is null or the property
					// cannot be found. This can happen when the source object is a collection and the
					// property is not found in the collection item.
				}
				catch (Exception ex)
				{
					BindingDiagnostics.SendBindingFailure(this, sourceObject, target, property, "Binding", $"Exception thrown from setter: {ex.Message}");
				}
			}
		}

		// SubscribeToAncestryChanges, ClearAncestryChangeSubscriptions, FindAncestryIndex, and
		// OnElementParentSet are used with RelativeSource ancestor-type bindings, to detect when
		// there has been an ancestry change requiring re-applying the binding, and to minimize
		// re-applications especially during visual tree building.
		internal override void SubscribeToAncestryChanges(List<Element> chain, bool includeBindingContext, bool rootIsSource)
		{
			ClearAncestryChangeSubscriptions();
			if (chain == null)
				return;
			_isBindingContextRelativeSource = includeBindingContext;
			_ancestryChain = new List<WeakReference<Element>>();
			for (int i = 0; i < chain.Count; i++)
			{
				var elem = chain[i];
				if (i != chain.Count - 1 || !rootIsSource)
					// don't care about a successfully resolved source's parents
					elem.ParentSet += OnElementParentSet;
				if (_isBindingContextRelativeSource)
					elem.BindingContextChanged += OnElementBindingContextChanged;
				_ancestryChain.Add(new WeakReference<Element>(elem));
			}
		}

		void ClearAncestryChangeSubscriptions(int beginningWith = 0)
		{
			if (_ancestryChain == null || _ancestryChain.Count == 0)
				return;
			int count = _ancestryChain.Count;
			for (int i = beginningWith; i < count; i++)
			{
				Element elem;
				var weakElement = _ancestryChain.Last();
				if (weakElement.TryGetTarget(out elem))
				{
					elem.ParentSet -= OnElementParentSet;
					if (_isBindingContextRelativeSource)
						elem.BindingContextChanged -= OnElementBindingContextChanged;
				}
				_ancestryChain.RemoveAt(_ancestryChain.Count - 1);
			}
		}

		// Returns -1 if the member is not in the chain or the
		// chain is no longer valid.
		int FindAncestryIndex(Element elem)
		{
			for (int i = 0; i < _ancestryChain.Count; i++)
			{
				WeakReference<Element> weak = _ancestryChain[i];
				Element chainMember = null;
				if (!weak.TryGetTarget(out chainMember))
					return -1;
				else if (object.Equals(elem, chainMember))
					return i;
			}
			return -1;
		}

		void OnElementBindingContextChanged(object sender, EventArgs e)
		{
			if (!(sender is Element elem))
				return;

			BindableObject target = null;
			if (_weakTarget?.TryGetTarget(out target) != true)
				return;

			object currentSource = null;
			if (_weakSource?.TryGetTarget(out currentSource) == true)
			{
				// make sure that this isn't just a repeat notice
				// from someone else in the chain about our already-resolved 
				// binding source
				if (object.ReferenceEquals(currentSource, elem.BindingContext))
					return;
			}

			Unapply();
			Apply(null, target, _targetProperty, false, SetterSpecificity.FromBinding);
		}

		void OnElementParentSet(object sender, EventArgs e)
		{
			if (!(sender is Element elem))
				return;

			BindableObject target = null;
			if (_weakTarget?.TryGetTarget(out target) != true)
				return;

			if (elem.Parent == null)
			{
				// Remove anything further up in the chain
				// than the element with the null parent
				int index = FindAncestryIndex(elem);
				if (index == -1)
				{
					Unapply();
					return;
				}
				if (index + 1 < _ancestryChain.Count)
					ClearAncestryChangeSubscriptions(index + 1);

				// Force the binding expression to resolve to null
				// for now, until someone in the chain gets a new
				// non-null parent.
				ApplyCore(null, target, _targetProperty, false, _specificity);
			}
			else
			{
				Unapply();
				Apply(null, target, _targetProperty, false, _specificity);
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

#nullable enable
		private interface IPropertyChangeHandler
		{
			void Subscribe(object sourceObject);
			void Unsubscribe();
			TypedBinding<TSource, TProperty> CloneBinding();
		}

		private class LegacyPropertyChangeHandler : IPropertyChangeHandler
		{
			private readonly TypedBinding<TSource, TProperty> _binding;
			private readonly PropertyChangedProxy[]? _handlers;

			public LegacyPropertyChangeHandler(
				TypedBinding<TSource, TProperty> binding,
				Tuple<Func<TSource, object>, string>[]? handlers)
			{
				_binding = binding;

				if (handlers == null)
					return;

				_handlers = new PropertyChangedProxy[handlers.Length];
				for (var i = 0; i < handlers.Length; i++)
				{
					if (handlers[i] is null)
						continue;
					_handlers[i] = new PropertyChangedProxy(handlers[i].Item1, handlers[i].Item2, binding);
				}
			}

			public void Subscribe(object sourceObject)
			{
				if (sourceObject is not TSource source || _handlers is null)
				{
					return;
				}

				for (var i = 0; i < _handlers.Length; i++)
				{
					if (_handlers[i] == null)
						continue;
					var part = _handlers[i].PartGetter(source);
					if (part == null)
						break;
					var inpc = part as INotifyPropertyChanged;
					if (inpc == null)
						continue;
					_handlers[i].Part = (inpc);
				}
			}

			public void Unsubscribe()
			{
				if (_handlers is null)
					return;

				for (var i = 0; i < _handlers.Length; i++)
				{
					_handlers[i]?.Listener.Unsubscribe();
				}
			}

			public TypedBinding<TSource, TProperty> CloneBinding()
			{
				Tuple<Func<TSource, object>, string>[]? handlers = null;
				if (_handlers != null)
				{
					handlers = new Tuple<Func<TSource, object>, string>[_handlers.Length];
					for (var i = 0; i < _handlers.Length; i++)
					{
						if (_handlers[i] == null)
							continue;
						handlers[i] = new Tuple<Func<TSource, object>, string>(_handlers[i].PartGetter, _handlers[i].PropertyName);
					}
				}

				return new TypedBinding<TSource, TProperty>(_binding._getter, _binding._setter, handlers)
				{
					Mode = _binding.Mode,
					Converter = _binding.Converter,
					ConverterParameter = _binding.ConverterParameter,
					StringFormat = _binding.StringFormat,
					Source = _binding.Source,
					UpdateSourceEventName = _binding.UpdateSourceEventName,
				};
			}
		}

		private class PropertyChangeHandler(
			TypedBinding<TSource, TProperty> binding,
			int handlersCount,
			Func<TSource, IEnumerable<ValueTuple<INotifyPropertyChanged?, string>>>? handlers) : IPropertyChangeHandler
		{
			private readonly TypedBinding<TSource, TProperty> _binding = binding;
			private readonly BindingExpression.WeakPropertyChangedProxy?[] _listeners = new BindingExpression.WeakPropertyChangedProxy?[handlersCount];
			private readonly string?[] _propertyNames = new string?[handlersCount];
			private readonly Func<TSource, IEnumerable<ValueTuple<INotifyPropertyChanged?, string>>>? _handlers = handlers;

			public void Subscribe(object sourceObject)
			{
				if (sourceObject is not TSource source || _handlers is null)
				{
					return;
				}

				int index = 0;

				foreach ((INotifyPropertyChanged? part, string propertyName) in _handlers(source))
				{
					if (part is null || index >= _listeners.Length)
						break;

					_propertyNames[index] = propertyName;
					var listener = _listeners[index] ??= new();
					index++;

					// Check if we're already subscribed to the same object
					if (listener.TryGetSource(out var existingSource) && ReferenceEquals(part, existingSource))
					{
						// Already subscribed to the same object, no need to re-subscribe
						continue;
					}

					// Different object or first subscription, unsubscribe from old and subscribe to new
					listener.Unsubscribe();
					listener.Subscribe(part, OnPropertyChanged);
				}

				Unsubscribe(startIndex: index);
			}

			void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (ShouldApplyChanges(sender, e.PropertyName))
				{
					var dispatcher = (sender as BindableObject)?.Dispatcher;
					dispatcher.DispatchIfRequired(ApplyChanges);
				}
			}

			bool ShouldApplyChanges(object? sender, string? propertyName)
			{
				for (int i = 0; i < _listeners.Length; i++)
				{
					if (_listeners[i] is {} listener
						&& listener.TryGetSource(out var source)
						&& ReferenceEquals(source, sender)
						&& MatchEventPropertyName(_propertyNames[i], propertyName))
					{
						return true;
					}
				}

				return false;
			}

			void ApplyChanges()
			{
				_binding.Apply(fromTarget: false);
			}

			static bool MatchEventPropertyName(string? expectedPropertyName, string? eventPropertyName)
			{
				return string.IsNullOrEmpty(eventPropertyName)
					|| string.Equals(expectedPropertyName, eventPropertyName, StringComparison.Ordinal);
			}

			public void Unsubscribe()
				=> Unsubscribe(startIndex: 0);

			void Unsubscribe(int startIndex)
			{
				for (int i = startIndex; i < _listeners.Length; i++)
				{
					_listeners[i]?.Unsubscribe();
					_listeners[i] = null;
				}
			}

			public TypedBinding<TSource, TProperty> CloneBinding()
			{
				return new TypedBinding<TSource, TProperty>(_binding._getter, _binding._setter, _listeners.Length, _handlers)
				{
					Mode = _binding.Mode,
					Converter = _binding.Converter,
					ConverterParameter = _binding.ConverterParameter,
					StringFormat = _binding.StringFormat,
					Source = _binding.Source,
					UpdateSourceEventName = _binding.UpdateSourceEventName,
				};
			}
		}
#nullable disable
	}
}
