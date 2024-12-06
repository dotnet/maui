#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides a mechanism to propagate data changes from one object to another. Enables validation, type coercion, and an event system.
	/// </summary>
	/// <remarks>The <see cref="BindableObject" /> class provides a data storage mechanism that enables the application developer to synchronize data between objects in response to changes, for example, between the View and View Model in the MVVM design pattern. All of the visual elements in the <c>Microsoft.Maui.Controls</c> namespace inherit from <see cref="BindableObject" /> class, so they can all be used to bind the data behind their user interface.</remarks>
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicEvents)]
	public abstract class BindableObject : INotifyPropertyChanged, IDynamicResourceHandler
	{
		IDispatcher _dispatcher;

		/// <summary>
		/// Gets the dispatcher that was available when this bindable object was created,
		/// otherwise tries to find the nearest available dispatcher (probably the window's/app's).
		/// </summary>
		public IDispatcher Dispatcher =>
			_dispatcher ??= this.FindDispatcher();

		/// <summary>
		/// Initializes a new instance of the <see cref="BindableObject"/> class.
		/// </summary>
		public BindableObject()
		{
			// try use the current thread's dispatcher
			_dispatcher = Dispatching.Dispatcher.GetForCurrentThread();
		}

		internal ushort _triggerCount = 0;
		internal Dictionary<TriggerBase, SetterSpecificity> _triggerSpecificity = new Dictionary<TriggerBase, SetterSpecificity>();
		readonly Dictionary<BindableProperty, BindablePropertyContext> _properties = new Dictionary<BindableProperty, BindablePropertyContext>(4);
		bool _applying;
		WeakReference _inheritedContext;

		/// <summary>Bindable property for <see cref="BindingContext"/>.</summary>
		public static readonly BindableProperty BindingContextProperty =
			BindableProperty.Create(nameof(BindingContext), typeof(object), typeof(BindableObject), default(object),
									BindingMode.OneWay, null, BindingContextPropertyChanged, null, null, BindingContextPropertyBindingChanging);

		/// <summary>
		/// Gets or sets an object that contains the properties that will be targeted by the bound properties that belong to this <see cref="BindableObject" />.
		/// This is a bindable property.
		/// </summary>
		public object BindingContext
		{
			get => _inheritedContext?.Target ?? GetValue(BindingContextProperty);
			set => SetValue(BindingContextProperty, value);
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Occurs when a property value is changing.
		/// </summary>
		public event PropertyChangingEventHandler PropertyChanging;

		/// <summary>
		/// Occurs when the value of the <see cref="BindingContext"/> property changes.
		/// </summary>
		public event EventHandler BindingContextChanged;

		/// <summary>
		/// Clears any value that is previously set for a bindable property.
		/// </summary>
		/// <param name="property">The <see cref="BindableProperty"/> to clear the value for.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <see langword="null"/>.</exception>
		/// <remarks>When <paramref name="property"/> is read-only, nothing will happen.</remarks>
		public void ClearValue(BindableProperty property)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			if (property.IsReadOnly)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>()?.LogWarning($"Cannot set the BindableProperty \"{property.PropertyName}\" because it is readonly.");
				return;
			}

			ClearValueCore(property, SetterSpecificity.ManualValueSetter);
		}

		internal void ClearValue(BindableProperty property, SetterSpecificity specificity)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			if (property.IsReadOnly)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>()?.LogWarning($"Cannot set the BindableProperty \"{property.PropertyName}\" because it is readonly.");
				return;
			}

			ClearValueCore(property, specificity);
		}

		/// <summary>
		/// Clears any value that is previously set for a bindable property, identified by its key.
		/// </summary>
		/// <param name="propertyKey">The key that identifies the bindable property to clear the value for.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyKey"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown when <paramref name="propertyKey"/> is a read-only property.</exception>
		public void ClearValue(BindablePropertyKey propertyKey)
		{
			if (propertyKey == null)
				throw new ArgumentNullException(nameof(propertyKey));

			ClearValueCore(propertyKey.BindableProperty, SetterSpecificity.ManualValueSetter);
		}

		void ClearValueCore(BindableProperty property, SetterSpecificity specificity)
		{

			BindablePropertyContext bpcontext = GetContext(property);
			if (bpcontext == null)
				return;

			var original = bpcontext.Values.GetSpecificityAndValue();
			if (original.Key == SetterSpecificity.FromHandler)
			{
				bpcontext.Values.Remove(SetterSpecificity.FromHandler);
			}

			var newValue = bpcontext.Values.GetClearedValue(specificity);
			var changed = !Equals(original.Value, newValue);
			if (changed)
			{
				property.PropertyChanging?.Invoke(this, original.Value, newValue);
				OnPropertyChanging(property.PropertyName);
			}

			bpcontext.Values.Remove(specificity);

			//there's some side effect implemented in CoerceValue (see IsEnabled) that we need to trigger here
			if (property.CoerceValue != null)
				property.CoerceValue(this, newValue);

			OnBindablePropertySet(property, original.Value, newValue, changed, changed);
		}

		/// <summary>
		/// Returns the value that is contained in the given bindable property.
		/// </summary>
		/// <param name="property">The bindable property for which to get the value.</param>
		/// <returns>The value that is contained in the <see cref="BindableProperty" />.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <see langword="null"/>.</exception>
		/// <remarks>
		/// <see cref="GetValue(BindableProperty)" /> and <see cref="SetValue(BindableProperty, object)" /> are used to access the values of properties that are implemented by a <see cref="BindableProperty" />.
		/// That is, application developers typically provide an interface for a bound property by defining a <see langword="public" /> property whose <see langword="get" /> accessor casts the result of <see cref="GetValue(BindableProperty)" /> to the appropriate type and returns it, and whose <see langword="set" /> accessor uses <see cref="SetValue(BindableProperty, object)" /> to set the value on the correct property.
		/// Application developers should perform no other steps in the public property that defines the interface of the bound property.
		/// </remarks>
		public object GetValue(BindableProperty property)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			var context = property.DefaultValueCreator != null ? GetOrCreateContext(property) : GetContext(property);

			return context == null ? property.DefaultValue : context.Values.GetValue();
		}

		internal LocalValueEnumerator GetLocalValueEnumerator() => new LocalValueEnumerator(this);

		internal sealed class LocalValueEnumerator : IEnumerator<LocalValueEntry>
		{
			Dictionary<BindableProperty, BindablePropertyContext>.Enumerator _propertiesEnumerator;
			internal LocalValueEnumerator(BindableObject bindableObject) => _propertiesEnumerator = bindableObject._properties.GetEnumerator();

			object IEnumerator.Current => Current;
			public LocalValueEntry Current { get; private set; }

			public bool MoveNext()
			{
				if (_propertiesEnumerator.MoveNext())
				{
					Current = new LocalValueEntry(_propertiesEnumerator.Current.Key, _propertiesEnumerator.Current.Value.Values.GetValue(), _propertiesEnumerator.Current.Value.Attributes);
					return true;
				}
				return false;
			}

			public void Dispose() => _propertiesEnumerator.Dispose();

			void IEnumerator.Reset()
			{
				((IEnumerator)_propertiesEnumerator).Reset();
				Current = null;
			}
		}

		internal sealed class LocalValueEntry
		{
			internal LocalValueEntry(BindableProperty property, object value, BindableContextAttributes attributes)
			{
				Property = property;
				Value = value;
				Attributes = attributes;
			}

			public BindableProperty Property { get; }
			public object Value { get; }
			public BindableContextAttributes Attributes { get; }
		}

		internal (bool IsSet, T Value)[] GetValues<T>(BindableProperty[] propArray)
		{
			Dictionary<BindableProperty, BindablePropertyContext> properties = _properties;
			var resultArray = new (bool IsSet, T Value)[propArray.Length];

			for (int i = 0; i < propArray.Length; i++)
			{
				if (properties.TryGetValue(propArray[i], out var context))
				{
					var pair = context.Values.GetSpecificityAndValue();
					resultArray[i].IsSet = pair.Key != SetterSpecificity.DefaultValue;
					resultArray[i].Value = (T)pair.Value;
				}
				else
				{
					resultArray[i].IsSet = false;
					resultArray[i].Value = default(T);
				}
			}

			return resultArray;
		}

		/// <summary>
		/// Determines whether or not a bindable property exists and has a value set.
		/// </summary>
		/// <param name="targetProperty">The bindable property to check if a value is currently set.</param>
		/// <returns><see langword="true"/> if the target property exists and has been set. Otherwise <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="targetProperty"/> is <see langword="null"/>.</exception>
		public bool IsSet(BindableProperty targetProperty)
		{
			var bpcontext = GetContext(targetProperty ?? throw new ArgumentNullException(nameof(targetProperty)));
			if (bpcontext == null)
				return false;
			if ((bpcontext.Attributes & BindableContextAttributes.IsDefaultValueCreated) == BindableContextAttributes.IsDefaultValueCreated)
				return true;
			return bpcontext.Values.GetSpecificity() != SetterSpecificity.DefaultValue;
		}


		/// <summary>
		/// Removes a previously set binding from a bindable property.
		/// </summary>
		/// <param name="property">The bindable property from which to remove bindings.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <see langword="null"/>.</exception>
		/// <remarks>When <paramref name="property" /> is not currently bound, nothing will happen.</remarks>
		public void RemoveBinding(BindableProperty property)
		{
			BindablePropertyContext context = GetContext(property ?? throw new ArgumentNullException(nameof(property)));

			var specificity = SetterSpecificity.FromBinding;
			if (context != null && context.Bindings.Count > 0)
				specificity = context.Bindings.GetSpecificity();

			RemoveBinding(property, specificity);
		}

		internal void RemoveBinding(BindableProperty property, SetterSpecificity specificity)
		{
			BindablePropertyContext context = GetContext(property ?? throw new ArgumentNullException(nameof(property)));

			if (context != null && context.Bindings.Count > 0)
				RemoveBinding(property, context, specificity);
		}

		/// <summary>
		/// Assigns a binding to a bindable property.
		/// </summary>
		/// <param name="targetProperty">The bindable property on which to apply <paramref name="binding"/>.</param>
		/// <param name="binding">The binding to set for <paramref name="targetProperty"/>.</param>
		public void SetBinding(BindableProperty targetProperty, BindingBase binding)
			=> SetBinding(targetProperty, binding, binding != null && targetProperty != null && binding.GetRealizedMode(targetProperty) == BindingMode.TwoWay ? SetterSpecificity.FromHandler : SetterSpecificity.FromBinding);

		internal void SetBinding(BindableProperty targetProperty, BindingBase binding, SetterSpecificity specificity)
		{
			if (targetProperty == null)
				throw new ArgumentNullException(nameof(targetProperty));

			if (targetProperty.IsReadOnly && binding.Mode == BindingMode.OneWay)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>()?.LogWarning($"Cannot set the a OneWay Binding \"{targetProperty.PropertyName}\" because it is readonly.");
				return;
			}

			var context = GetOrCreateContext(targetProperty);

			//if the value is manually set (has highest specificity than FromBinding), we reassign the specificity so it'll get replaced when the binding is applied
			var currentSpecificity = context.Values.GetSpecificity();
			if (currentSpecificity > SetterSpecificity.FromBinding)
			{
				var currentValue = context.Values.GetValue();

				context.Values.Remove(currentSpecificity);
				context.Values[SetterSpecificity.FromBinding] = currentValue;
			}

			BindingBase oldBinding = null;
			SetterSpecificity oldSpecificity = default;
			if (context.Bindings.Count > 0)
			{
				var b_p = context.Bindings.GetSpecificityAndValue();
				oldSpecificity = b_p.Key;
				oldBinding = b_p.Value;
			}

			if (oldBinding != null && specificity < oldSpecificity)
			{
				context.Bindings[specificity] = binding;
				return;
			}

			oldBinding?.Unapply();

			context.Bindings[specificity] = binding ?? throw new ArgumentNullException(nameof(binding));

			targetProperty.BindingChanging?.Invoke(this, oldBinding, binding);

			binding.Apply(BindingContext, this, targetProperty, false, specificity);
		}

		/// <summary>
		/// Sets the inherited context to a nested element.
		/// </summary>
		/// <param name="bindable">The object on which to set the inherited binding context.</param>
		/// <param name="value">The inherited context to set.</param>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetInheritedBindingContext(BindableObject bindable, object value)
		{
			// I wonder if we couldn't treat BindingContext with specificities
			BindablePropertyContext bpContext = bindable.GetContext(BindingContextProperty);
			if (bpContext != null && bpContext.Values.GetSpecificity() >= SetterSpecificity.ManualValueSetter)
				return;

			if (ReferenceEquals(bindable._inheritedContext?.Target, value))
				return;

			var binding = bpContext?.Bindings.GetValue();

			if (binding != null)
			{
				binding.Context = value;
				bindable._inheritedContext = null;
				// OnBindingContextChanged fires from within BindingContextProperty propertyChanged callback
				bindable.ApplyBinding(bpContext, fromBindingContextChanged: true);
			}
			else
			{
				bindable._inheritedContext = new WeakReference(value);
				bindable.ApplyBindings(fromBindingContextChanged: true);
				bindable.OnBindingContextChanged();
			}
		}

		/// <summary>
		/// Applies all the current bindings to <see cref="BindingContext" />.
		/// </summary>
		protected void ApplyBindings()
		{
			BindablePropertyContext bpContext = GetContext(BindingContextProperty);
			var binding = bpContext?.Bindings.GetValue();
			if (binding != null)
			{
				ApplyBinding(bpContext, fromBindingContextChanged: false);
			}
			else
			{
				ApplyBindings(fromBindingContextChanged: false);
			}
		}

		/// <summary>
		/// Raises the <see cref="BindingContextChanged"/> event.
		/// </summary>
		protected virtual void OnBindingContextChanged()
		{
			BindingContextChanged?.Invoke(this, EventArgs.Empty);

			if (Shell.GetBackButtonBehavior(this) is BackButtonBehavior buttonBehavior)
				SetInheritedBindingContext(buttonBehavior, BindingContext);

			if (Shell.GetSearchHandler(this) is SearchHandler searchHandler)
				SetInheritedBindingContext(searchHandler, BindingContext);
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">The name of the property that has changed.</param>
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		/// <summary>
		/// Raises the <see cref="PropertyChanging"/> event.
		/// </summary>
		/// <param name="propertyName">The name of the property that is changing.</param>
		protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
			=> PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

		/// <summary>
		/// Removes all current bindings from the current context.
		/// </summary>
		protected void UnapplyBindings()
		{
			foreach (var context in _properties.Values)
				context.Bindings.GetValue()?.Unapply();
		}

		internal bool GetIsBound(BindableProperty targetProperty)
		{
			if (targetProperty == null)
				throw new ArgumentNullException(nameof(targetProperty));

			BindablePropertyContext bpcontext = GetContext(targetProperty);
			return bpcontext != null && bpcontext.Bindings.Count > 0;
		}

		internal virtual void OnRemoveDynamicResource(BindableProperty property)
		{
		}

		internal virtual void OnSetDynamicResource(BindableProperty property, string key, SetterSpecificity specificity)
		{
		}

		internal void RemoveDynamicResource(BindableProperty property)
			=> RemoveDynamicResource(property, SetterSpecificity.DynamicResourceSetter);

		internal void RemoveDynamicResource(BindableProperty property, SetterSpecificity specificity)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			OnRemoveDynamicResource(property);
			BindablePropertyContext context = GetOrCreateContext(property);
			context.Attributes &= ~BindableContextAttributes.IsDynamicResource;
		}

		void IDynamicResourceHandler.SetDynamicResource(BindableProperty property, string key)
			=> SetDynamicResource(property, key, SetterSpecificity.DynamicResourceSetter);

		internal void SetDynamicResource(BindableProperty property, string key)
			=> SetDynamicResource(property, key, SetterSpecificity.DynamicResourceSetter);

		//FIXME, use specificity
		internal void SetDynamicResource(BindableProperty property, string key, SetterSpecificity specificity)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException(nameof(key));

			OnSetDynamicResource(property, key, specificity);
		}

		/// <summary>
		/// Sets the value of the specified bindable property.
		/// </summary>
		/// <param name="property">The bindable property on which to assign a value.</param>
		/// <param name="value">The value to set.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <see langword="null"/>.</exception>
		/// <remarks>If <paramref name="property"/> is read-only, nothing will happen.</remarks>
		public void SetValue(BindableProperty property, object value)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			if (value is BindingBase binding && !property.ReturnType.IsAssignableFrom(typeof(BindableProperty)))
			{
				SetBinding(property, binding);
				return;
			}

			if (property.IsReadOnly)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>()?.LogWarning($"Cannot set the BindableProperty \"{property.PropertyName}\" because it is readonly.");
				return;
			}
			SetValueCore(property, value, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearDynamicResource, SetValuePrivateFlags.Default, SetterSpecificity.ManualValueSetter);
		}

		/// <summary>
		/// Sets the value of the specified bindable property.
		/// </summary>
		/// <param name="propertyKey">The key that identifies the bindable property to assign the value to.</param>
		/// <param name="value">The value to set.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyKey"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the bindable property identified by <paramref name="propertyKey"/> is read-only.</exception>
		public void SetValue(BindablePropertyKey propertyKey, object value)
		{
			if (propertyKey == null)
				throw new ArgumentNullException(nameof(propertyKey));

			SetValueCore(propertyKey.BindableProperty, value, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearDynamicResource, SetValuePrivateFlags.Default, SetterSpecificity.ManualValueSetter);
		}

		internal void SetValue(BindableProperty property, object value, SetterSpecificity specificity)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			if (property.IsReadOnly)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>()?.LogWarning($"Cannot set the BindableProperty \"{property.PropertyName}\" because it is readonly.");
				return;
			}

			SetValueCore(property, value, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearDynamicResource, SetValuePrivateFlags.Default, specificity);
		}

		internal void SetValue(BindablePropertyKey propertyKey, object value, SetterSpecificity specificity)
		{
			if (propertyKey == null)
				throw new ArgumentNullException(nameof(propertyKey));

			SetValueCore(propertyKey.BindableProperty, value, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearDynamicResource, SetValuePrivateFlags.Default, specificity);

		}

		/// <summary>
		/// Method for internal use to set the value of the specified property.
		/// </summary>
		/// <param name="property">The bindable property to assign a value to.</param>
		/// <param name="value">The value to set.</param>
		/// <param name="attributes">The flags that are applied for setting this value.</param>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("go away")]
		internal void SetValueCore(BindableProperty property, object value, SetValueFlags attributes = SetValueFlags.None)
			=> SetValueCore(property, value, attributes, SetValuePrivateFlags.Default, new SetterSpecificity());

		//FIXME: GO AWAY
		internal void SetValueCore(BindableProperty property, object value, SetValueFlags attributes, SetValuePrivateFlags privateAttributes)
			=> SetValueCore(property, value, attributes, privateAttributes, new SetterSpecificity());

		internal void SetValueCore(BindableProperty property, object value, SetValueFlags attributes, SetValuePrivateFlags privateAttributes, SetterSpecificity specificity)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			bool converted = (privateAttributes & SetValuePrivateFlags.Converted) != 0;

			if (!converted && !property.TryConvert(ref value))
			{
				Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>()?.LogWarning($"Cannot convert {value} to type '{property.ReturnType}'");
				return;
			}

			if (property.ValidateValue != null && !property.ValidateValue(this, value))
			{
				Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>()?.LogWarning($"Value is an invalid value for {property.PropertyName}");
				return;
			}

			if (property.CoerceValue != null)
				value = property.CoerceValue(this, value);

			BindablePropertyContext context = GetOrCreateContext(property);

			bool currentlyApplying = _applying;

			if ((context.Attributes & BindableContextAttributes.IsBeingSet) != 0)
			{
				Queue<SetValueArgs> delayQueue = context.DelayedSetters;
				if (delayQueue == null)
					context.DelayedSetters = delayQueue = new Queue<SetValueArgs>();

				delayQueue.Enqueue(new SetValueArgs(property, context, value, currentlyApplying, attributes, specificity));
			}
			else
			{
				var silent = (privateAttributes & SetValuePrivateFlags.Silent) != 0;
				context.Attributes |= BindableContextAttributes.IsBeingSet;
				SetValueActual(property, context, value, currentlyApplying, attributes, specificity, silent);

				Queue<SetValueArgs> delayQueue = context.DelayedSetters;
				if (delayQueue != null)
				{
					while (delayQueue.Count > 0)
					{
						SetValueArgs s = delayQueue.Dequeue();
						if (s != null)
							SetValueActual(s.Property, s.Context, s.Value, s.CurrentlyApplying, s.Attributes, s.Specificity, silent);
					}

					context.DelayedSetters = null;
				}

				context.Attributes &= ~BindableContextAttributes.IsBeingSet;
			}
		}

		void SetValueActual(BindableProperty property, BindablePropertyContext context, object value, bool currentlyApplying, SetValueFlags attributes, SetterSpecificity specificity, bool silent = false)
		{
			var specificityAndValue = context.Values.GetSpecificityAndValue();
			var original = specificityAndValue.Value;
			var originalSpecificity = specificityAndValue.Key;

			//if the last value was set from handler, override it
			if (specificity != SetterSpecificity.FromHandler
				&& originalSpecificity == SetterSpecificity.FromHandler)
			{
				context.Values.Remove(SetterSpecificity.FromHandler);
				originalSpecificity = context.Values.GetSpecificity();
			}

			//We keep setter of lower specificity so we can unapply
			if (specificity < originalSpecificity)
			{
				context.Values[specificity] = value;
				return;
			}

			bool raiseOnEqual = (attributes & SetValueFlags.RaiseOnEqual) != 0;

			bool clearDynamicResources = (attributes & SetValueFlags.ClearDynamicResource) != 0;
			// bool clearOneWayBindings = (attributes & SetValueFlags.ClearOneWayBindings) != 0 && specificity != SetterSpecificity.FromHandler;
			// bool clearTwoWayBindings = (attributes & SetValueFlags.ClearTwoWayBindings) != 0 && specificity != SetterSpecificity.FromHandler;

			bool sameValue = ReferenceEquals(context.Property, BindingContextProperty) ? ReferenceEquals(value, original) : Equals(value, original);
			if (!silent && (!sameValue || raiseOnEqual))
			{
				property.PropertyChanging?.Invoke(this, original, value);

				OnPropertyChanging(property.PropertyName);
			}

			context.Values[specificity] = value;

			context.Attributes &= ~BindableContextAttributes.IsDefaultValueCreated;

			if ((context.Attributes & BindableContextAttributes.IsDynamicResource) != 0 && clearDynamicResources)
				RemoveDynamicResource(property);

			BindingBase binding = context.Bindings.GetValue();

			if (!silent && (!sameValue || raiseOnEqual))
			{
				if (binding != null && !currentlyApplying)
				{
					_applying = true;
					binding.Apply(true);
					_applying = false;
				}

				OnBindablePropertySet(property, original, value, !sameValue, true);
			}
			else
			{
				OnBindablePropertySet(property, original, value, !sameValue, false);
			}
		}

		private protected virtual void OnBindablePropertySet(BindableProperty property, object original, object value, bool didChange, bool willFirePropertyChanged)
		{
			if (willFirePropertyChanged)
			{
				OnPropertyChanged(property.PropertyName);
				property.PropertyChanged?.Invoke(this, original, value);
			}
		}

		void ApplyBindings(bool fromBindingContextChanged)
		{
			var prop = _properties.Values.ToArray();

			for (int i = 0, propLength = prop.Length; i < propLength; i++)
			{
				BindablePropertyContext context = prop[i];
				if (ReferenceEquals(context.Property, BindingContextProperty))
				{
					// BindingContextProperty Binding is handled separately within SetInheritedBindingContext
					continue;
				}

				ApplyBinding(context, fromBindingContextChanged);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void ApplyBinding(BindablePropertyContext context, bool fromBindingContextChanged)
		{
			var bindings = context.Bindings;
			if (bindings.Count == 0)
			{
				return;
			}

			var kvp = bindings.GetSpecificityAndValue();
			var binding = kvp.Value;

			if (binding == null)
			{
				return;
			}

			var specificity = kvp.Key;
			binding.Unapply(fromBindingContextChanged);
			binding.Apply(BindingContext, this, context.Property, fromBindingContextChanged, specificity);
		}

		static void BindingContextPropertyBindingChanging(BindableObject bindable, BindingBase oldBindingBase, BindingBase newBindingBase)
		{
			object context = bindable._inheritedContext?.Target;
			var oldBinding = oldBindingBase as Binding;
			var newBinding = newBindingBase as Binding;

			if (context == null && oldBinding != null)
				context = oldBinding.Context;
			if (context != null && newBinding != null)
				newBinding.Context = context;
		}

		static void BindingContextPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			bindable._inheritedContext = null;
			bindable.ApplyBindings(fromBindingContextChanged: true);
			bindable.OnBindingContextChanged();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		BindablePropertyContext CreateAndAddContext(BindableProperty property)
		{
			var defaultValueCreator = property.DefaultValueCreator;
			var context = new BindablePropertyContext { Property = property };
			context.Values[SetterSpecificity.DefaultValue] = defaultValueCreator != null ? defaultValueCreator(this) : property.DefaultValue;

			if (defaultValueCreator != null)
				context.Attributes = BindableContextAttributes.IsDefaultValueCreated;

			_properties.Add(property, context);
			return context;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal BindablePropertyContext GetContext(BindableProperty property) => _properties.TryGetValue(property, out var result) ? result : null;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		BindablePropertyContext GetOrCreateContext(BindableProperty property) => GetContext(property) ?? CreateAndAddContext(property);

		void RemoveBinding(BindableProperty property, BindablePropertyContext context, SetterSpecificity specificity)
		{
			var count = context.Bindings.Count;

			if (count == 0)
				return; //used to fail;

			var currentbinding = context.Bindings.GetValue();
			var binding = context.Bindings[specificity];
			var isCurrent = binding == currentbinding;

			if (isCurrent)
			{
				binding.Unapply();

				currentbinding = null;
				if (count > 1)
					currentbinding = context.Bindings.GetClearedValue();

				property.BindingChanging?.Invoke(this, binding, currentbinding);

				currentbinding?.Apply(BindingContext, this, property, false, context.Bindings.GetClearedSpecificity());
			}

			context.Bindings.Remove(specificity);
		}

		/// <summary>
		/// Coerces the value of the specified bindable property.
		/// This is done by invoking <see cref="BindableProperty.CoerceValueDelegate"/> of the specified bindable property.
		/// </summary>
		/// <param name="property">The bindable property to coerce the value of.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown when <paramref name="property"/> is read-only.</exception>
		/// <exception cref="ArgumentException">Thrown when the value is invalid according to the assigned logic in <see cref="BindableProperty.ValidateValueDelegate"/>.</exception>
		/// <remarks>If <see cref="BindableProperty.CoerceValueDelegate"/> is not assigned to, nothing will happen.</remarks>
		public void CoerceValue(BindableProperty property) => CoerceValue(property, checkAccess: true);

		/// <summary>
		/// Coerces the value of the specified bindable property.
		/// This is done by invoking <see cref="BindableProperty.CoerceValueDelegate"/> of the specified bindable property.
		/// </summary>
		/// <param name="propertyKey">The key that identifies the bindable property to coerce the value of.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyKey"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the bindable property identified by <paramref name="propertyKey"/> is read-only.</exception>
		/// <exception cref="ArgumentException">Thrown when the value is invalid according to the assigned logic in <see cref="BindableProperty.ValidateValueDelegate"/>.</exception>
		/// <remarks>If <see cref="BindableProperty.CoerceValueDelegate"/> is not assigned to, nothing will happen.</remarks>
		public void CoerceValue(BindablePropertyKey propertyKey)
		{
			if (propertyKey == null)
				throw new ArgumentNullException(nameof(propertyKey));

			CoerceValue(propertyKey.BindableProperty, checkAccess: false);
		}

		void CoerceValue(BindableProperty property, bool checkAccess)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			if (checkAccess && property.IsReadOnly)
				throw new InvalidOperationException($"The BindableProperty \"{property.PropertyName}\" is readonly.");

			BindablePropertyContext bpcontext = GetContext(property);
			if (bpcontext == null)
				return;

			object currentValue = bpcontext.Values.GetValue();

			if (property.ValidateValue != null && !property.ValidateValue(this, currentValue))
				throw new ArgumentException($"Value is an invalid value for {property.PropertyName}", nameof(currentValue));

			property.CoerceValue?.Invoke(this, currentValue);
		}

		[Flags]
		internal enum BindableContextAttributes
		{
			IsBeingSet = 1 << 1,
			//GO AWAY
			IsDynamicResource = 1 << 2,
			IsSetFromStyle = 1 << 3,
			IsDefaultValueCreated = 1 << 5,
		}

		internal sealed class BindablePropertyContext
		{
			public BindableContextAttributes Attributes;

			public SetterSpecificityList<BindingBase> Bindings = new();

			public Queue<SetValueArgs> DelayedSetters;
			public BindableProperty Property;
			public readonly SetterSpecificityList<object> Values = new(3);
		}


		[Flags]
		internal enum SetValuePrivateFlags
		{
			None = 0,
			Silent = 1 << 1,
			FromStyle = 1 << 3,
			Converted = 1 << 4,
			Default = None
		}

		internal sealed class SetValueArgs
		{
			public readonly SetValueFlags Attributes;
			public readonly BindablePropertyContext Context;
			public readonly bool CurrentlyApplying;
			public readonly BindableProperty Property;
			public readonly object Value;
			public readonly SetterSpecificity Specificity;

			public SetValueArgs(BindableProperty property, BindablePropertyContext context, object value, bool currentlyApplying, SetValueFlags attributes, SetterSpecificity specificity)
			{
				Property = property;
				Context = context;
				Value = value;
				CurrentlyApplying = currentlyApplying;
				Attributes = attributes;
				Specificity = specificity;
			}
		}
	}

	namespace Internals
	{
		[Flags]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public enum SetValueFlags
		{
			None = 0,
			ClearOneWayBindings = 1 << 0,
			ClearTwoWayBindings = 1 << 1,
			ClearDynamicResource = 1 << 2,
			RaiseOnEqual = 1 << 3
		}
	}
}
