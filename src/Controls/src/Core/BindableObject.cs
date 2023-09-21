using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

		readonly Dictionary<BindableProperty, BindablePropertyContext> _properties = new Dictionary<BindableProperty, BindablePropertyContext>(4);
		bool _applying;
		object _inheritedContext;

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
			get => _inheritedContext ?? GetValue(BindingContextProperty);
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
		public void ClearValue(BindableProperty property) => ClearValue(property, fromStyle: false, checkAccess: true);

		internal void ClearValue(BindableProperty property, bool fromStyle) => ClearValue(property, fromStyle: fromStyle, checkAccess: true);

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

			ClearValue(propertyKey.BindableProperty, fromStyle: false, checkAccess: false);
		}

		void ClearValue(BindableProperty property, bool fromStyle, bool checkAccess)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			if (checkAccess && property.IsReadOnly)
				throw new InvalidOperationException($"The BindableProperty \"{property.PropertyName}\" is readonly.");

			BindablePropertyContext bpcontext = GetContext(property);
			if (bpcontext == null)
				return;

			if (fromStyle)
				bpcontext.StyleValueSet = false;

			if (fromStyle && !CanBeSetFromStyle(property))
				return;

			object original = bpcontext.Value;

			object newValue = bpcontext.StyleValueSet ? bpcontext.StyleValue : property.GetDefaultValue(this);

			bool same = Equals(original, newValue);
			if (!same)
			{
				property.PropertyChanging?.Invoke(this, original, newValue);
				OnPropertyChanging(property.PropertyName);
			}

			bpcontext.Attributes &= ~BindableContextAttributes.IsManuallySet;
			bpcontext.Value = newValue;
			if (bpcontext.StyleValueSet)
				bpcontext.Attributes |= BindableContextAttributes.IsSetFromStyle;
			else if (property.DefaultValueCreator == null)
				bpcontext.Attributes |= BindableContextAttributes.IsDefaultValue;
			else
				bpcontext.Attributes |= BindableContextAttributes.IsDefaultValueCreated;

			if (!same)
			{
				OnPropertyChanged(property.PropertyName);
				property.PropertyChanged?.Invoke(this, original, newValue);
			}
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

			return context == null ? property.DefaultValue : context.Value;
		}

		internal LocalValueEnumerator GetLocalValueEnumerator() => new LocalValueEnumerator(this);

		internal class LocalValueEnumerator : IEnumerator<LocalValueEntry>
		{
			Dictionary<BindableProperty, BindablePropertyContext>.Enumerator _propertiesEnumerator;
			internal LocalValueEnumerator(BindableObject bindableObject) => _propertiesEnumerator = bindableObject._properties.GetEnumerator();

			object IEnumerator.Current => Current;
			public LocalValueEntry Current { get; private set; }

			public bool MoveNext()
			{
				if (_propertiesEnumerator.MoveNext())
				{
					Current = new LocalValueEntry(_propertiesEnumerator.Current.Key, _propertiesEnumerator.Current.Value.Value, _propertiesEnumerator.Current.Value.Attributes);
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

		internal class LocalValueEntry
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
					resultArray[i].IsSet = (context.Attributes & BindableContextAttributes.IsDefaultValue) == 0;
					resultArray[i].Value = (T)context.Value;
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
			return bpcontext != null
				&& (bpcontext.Attributes & BindableContextAttributes.IsDefaultValue) == 0;
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

			if (context?.Binding != null)
				RemoveBinding(property, context);
		}

		/// <summary>
		/// Assigns a binding to a bindable property.
		/// </summary>
		/// <param name="targetProperty">The bindable property on which to apply <paramref name="binding"/>.</param>
		/// <param name="binding">The binding to set for <paramref name="targetProperty"/>.</param>
		public void SetBinding(BindableProperty targetProperty, BindingBase binding) => SetBinding(targetProperty, binding, false);

		internal void SetBinding(BindableProperty targetProperty, BindingBase binding, bool fromStyle)
		{
			if (targetProperty == null)
				throw new ArgumentNullException(nameof(targetProperty));

			if (fromStyle && !CanBeSetFromStyle(targetProperty))
				return;

			var context = GetOrCreateContext(targetProperty);
			if (fromStyle)
				context.Attributes |= BindableContextAttributes.IsSetFromStyle;
			else
				context.Attributes &= ~BindableContextAttributes.IsSetFromStyle;

			if (context.Binding != null)
				context.Binding.Unapply();

			BindingBase oldBinding = context.Binding;
			context.Binding = binding ?? throw new ArgumentNullException(nameof(binding));

			targetProperty.BindingChanging?.Invoke(this, oldBinding, binding);

			binding.Apply(BindingContext, this, targetProperty);
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
			BindablePropertyContext bpContext = bindable.GetContext(BindingContextProperty);
			if (bpContext != null && ((bpContext.Attributes & BindableContextAttributes.IsManuallySet) != 0))
				return;

			object oldContext = bindable._inheritedContext;

			if (ReferenceEquals(oldContext, value))
				return;

			if (bpContext != null && oldContext == null)
				oldContext = bpContext.Value;

			if (bpContext != null && bpContext.Binding != null)
			{
				bpContext.Binding.Context = value;
				bindable._inheritedContext = null;
			}
			else
			{
				bindable._inheritedContext = value;
			}

			bindable.ApplyBindings(skipBindingContext: false, fromBindingContextChanged: true);
			bindable.OnBindingContextChanged();
		}

		/// <summary>
		/// Applies all the current bindings to <see cref="BindingContext" />.
		/// </summary>
		protected void ApplyBindings() => ApplyBindings(skipBindingContext: false, fromBindingContextChanged: false);

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

			if (Shell.GetTitleView(this) is View titleView)
				SetInheritedBindingContext(titleView, BindingContext);

			if (FlyoutBase.GetContextFlyout(this) is BindableObject contextFlyout)
				SetInheritedBindingContext(contextFlyout, BindingContext);
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
			{
				if (context.Binding == null)
					continue;

				context.Binding.Unapply();
			}
		}

		internal bool GetIsBound(BindableProperty targetProperty)
		{
			if (targetProperty == null)
				throw new ArgumentNullException(nameof(targetProperty));

			BindablePropertyContext bpcontext = GetContext(targetProperty);
			return bpcontext != null && bpcontext.Binding != null;
		}



		internal virtual void OnRemoveDynamicResource(BindableProperty property)
		{
		}

		internal virtual void OnSetDynamicResource(BindableProperty property, string key)
		{
		}

		internal void RemoveDynamicResource(BindableProperty property)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			OnRemoveDynamicResource(property);
			BindablePropertyContext context = GetOrCreateContext(property);
			context.Attributes &= ~BindableContextAttributes.IsDynamicResource;
		}

		bool CanBeSetFromStyle(BindableProperty property)
		{
			var context = GetContext(property);
			if (context == null)
				return true;
			if ((context.Attributes & BindableContextAttributes.IsSetFromStyle) == BindableContextAttributes.IsSetFromStyle)
				return true;
			if ((context.Attributes & BindableContextAttributes.IsDefaultValue) == BindableContextAttributes.IsDefaultValue)
				return true;
			if ((context.Attributes & BindableContextAttributes.IsDefaultValueCreated) == BindableContextAttributes.IsDefaultValueCreated)
				return true;
			return false;
		}

		void IDynamicResourceHandler.SetDynamicResource(BindableProperty property, string key) => SetDynamicResource(property, key, false);

		internal void SetDynamicResource(BindableProperty property, string key) => SetDynamicResource(property, key, false);

		internal void SetDynamicResource(BindableProperty property, string key, bool fromStyle)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException(nameof(key));
			if (fromStyle && !CanBeSetFromStyle(property))
				return;

			var context = GetOrCreateContext(property);

			context.Attributes |= BindableContextAttributes.IsDynamicResource;
			if (fromStyle)
				context.Attributes |= BindableContextAttributes.IsSetFromStyle;
			else
				context.Attributes &= ~BindableContextAttributes.IsSetFromStyle;

			OnSetDynamicResource(property, key);
		}

		/// <summary>
		/// Sets the value of the specified bindable property.
		/// </summary>
		/// <param name="property">The bindable property on which to assign a value.</param>
		/// <param name="value">The value to set.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <see langword="null"/>.</exception>
		/// <remarks>If <paramref name="property"/> is read-only, nothing will happen.</remarks>
		public void SetValue(BindableProperty property, object value) => SetValue(property, value, false, true);

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

			SetValue(propertyKey.BindableProperty, value, false, false);
		}

		internal void SetValue(BindableProperty property, object value, bool fromStyle) => SetValue(property, value, fromStyle, true);

		void SetValue(BindableProperty property, object value, bool fromStyle, bool checkAccess)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			if (checkAccess && property.IsReadOnly)
				throw new InvalidOperationException($"The BindableProperty \"{property.PropertyName}\" is readonly.");

			if (fromStyle)
				SetBackupStyleValue(property, value);
			if (fromStyle && !CanBeSetFromStyle(property))
				return;

			SetValueCore(property, value, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearDynamicResource,
				(fromStyle ? SetValuePrivateFlags.FromStyle : SetValuePrivateFlags.ManuallySet) | (checkAccess ? SetValuePrivateFlags.CheckAccess : 0));
		}

		internal void SetValueCore(BindablePropertyKey propertyKey, object value, SetValueFlags attributes = SetValueFlags.None)
			=> SetValueCore(propertyKey.BindableProperty, value, attributes, SetValuePrivateFlags.None);

		/// <summary>
		/// Method for internal use to set the value of the specified property.
		/// </summary>
		/// <param name="property">The bindable property to assign a value to.</param>
		/// <param name="value">The value to set.</param>
		/// <param name="attributes">The flags that are applied for setting this value.</param>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetValueCore(BindableProperty property, object value, SetValueFlags attributes = SetValueFlags.None)
			=> SetValueCore(property, value, attributes, SetValuePrivateFlags.Default);

		void SetBackupStyleValue(BindableProperty property, object value)
		{
			var context = GetOrCreateContext(property);
			context.StyleValueSet = true;
			context.StyleValue = value;
		}

		internal void SetValueCore(BindableProperty property, object value, SetValueFlags attributes, SetValuePrivateFlags privateAttributes)
		{
			bool checkAccess = (privateAttributes & SetValuePrivateFlags.CheckAccess) != 0;
			bool manuallySet = (privateAttributes & SetValuePrivateFlags.ManuallySet) != 0;
			bool silent = (privateAttributes & SetValuePrivateFlags.Silent) != 0;
			bool fromStyle = (privateAttributes & SetValuePrivateFlags.FromStyle) != 0;
			bool converted = (privateAttributes & SetValuePrivateFlags.Converted) != 0;

			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (checkAccess && property.IsReadOnly)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>()?.LogWarning($"Cannot set the BindableProperty \"{property.PropertyName}\" because it is readonly.");
				return;
			}

			if (!converted && !property.TryConvert(ref value))
			{
				Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>()?.LogWarning($"Cannot convert {value} to type '{property.ReturnType}'");
				return;
			}

			if (property.ValidateValue != null && !property.ValidateValue(this, value))
				throw new ArgumentException($"Value is an invalid value for {property.PropertyName}", nameof(value));

			if (property.CoerceValue != null)
				value = property.CoerceValue(this, value);

			BindablePropertyContext context = GetOrCreateContext(property);
			if (manuallySet)
			{
				context.Attributes |= BindableContextAttributes.IsManuallySet;
				context.Attributes &= ~BindableContextAttributes.IsSetFromStyle;
			}
			else
				context.Attributes &= ~BindableContextAttributes.IsManuallySet;

			if (fromStyle)
				context.Attributes |= BindableContextAttributes.IsSetFromStyle;
			// else omitted on purpose

			//if we're updating a dynamic resource from style, set the default backup value
			if ((context.Attributes & (BindableContextAttributes.IsDynamicResource | BindableContextAttributes.IsSetFromStyle)) != 0
				&& (attributes & SetValueFlags.ClearDynamicResource) == 0)
				SetBackupStyleValue(property, value);

			bool currentlyApplying = _applying;

			if ((context.Attributes & BindableContextAttributes.IsBeingSet) != 0)
			{
				Queue<SetValueArgs> delayQueue = context.DelayedSetters;
				if (delayQueue == null)
					context.DelayedSetters = delayQueue = new Queue<SetValueArgs>();

				delayQueue.Enqueue(new SetValueArgs(property, context, value, currentlyApplying, attributes));
			}
			else
			{
				context.Attributes |= BindableContextAttributes.IsBeingSet;
				SetValueActual(property, context, value, currentlyApplying, attributes, silent);

				Queue<SetValueArgs> delayQueue = context.DelayedSetters;
				if (delayQueue != null)
				{
					while (delayQueue.Count > 0)
					{
						SetValueArgs s = delayQueue.Dequeue();
						SetValueActual(s.Property, s.Context, s.Value, s.CurrentlyApplying, s.Attributes);
					}

					context.DelayedSetters = null;
				}

				context.Attributes &= ~BindableContextAttributes.IsBeingSet;
			}
		}

		void SetValueActual(BindableProperty property, BindablePropertyContext context, object value, bool currentlyApplying, SetValueFlags attributes, bool silent = false)
		{
			object original = context.Value;
			bool raiseOnEqual = (attributes & SetValueFlags.RaiseOnEqual) != 0;
			bool clearDynamicResources = (attributes & SetValueFlags.ClearDynamicResource) != 0;
			bool clearOneWayBindings = (attributes & SetValueFlags.ClearOneWayBindings) != 0;
			bool clearTwoWayBindings = (attributes & SetValueFlags.ClearTwoWayBindings) != 0;

			bool same = ReferenceEquals(context.Property, BindingContextProperty) ? ReferenceEquals(value, original) : Equals(value, original);
			if (!silent && (!same || raiseOnEqual))
			{
				property.PropertyChanging?.Invoke(this, original, value);

				OnPropertyChanging(property.PropertyName);
			}

			if (!same || raiseOnEqual)
			{
				context.Value = value;
			}

			context.Attributes &= ~BindableContextAttributes.IsDefaultValue;
			context.Attributes &= ~BindableContextAttributes.IsDefaultValueCreated;

			if ((context.Attributes & BindableContextAttributes.IsDynamicResource) != 0 && clearDynamicResources)
				RemoveDynamicResource(property);

			BindingBase binding = context.Binding;
			if (binding != null)
			{
				if (clearOneWayBindings && binding.GetRealizedMode(property) == BindingMode.OneWay || clearTwoWayBindings && binding.GetRealizedMode(property) == BindingMode.TwoWay)
				{
					RemoveBinding(property, context);
					binding = null;
				}
			}

			if (!silent && (!same || raiseOnEqual))
			{
				if (binding != null && !currentlyApplying)
				{
					_applying = true;
					binding.Apply(true);
					_applying = false;
				}

				OnPropertyChanged(property.PropertyName);

				property.PropertyChanged?.Invoke(this, original, value);
			}
		}

		internal void ApplyBindings(bool skipBindingContext, bool fromBindingContextChanged)
		{
			var prop = _properties.Values.ToArray();
			for (int i = 0, propLength = prop.Length; i < propLength; i++)
			{
				BindablePropertyContext context = prop[i];
				BindingBase binding = context.Binding;
				if (binding == null)
					continue;

				if (skipBindingContext && ReferenceEquals(context.Property, BindingContextProperty))
					continue;

				binding.Unapply(fromBindingContextChanged: fromBindingContextChanged);
				binding.Apply(BindingContext, this, context.Property, fromBindingContextChanged: fromBindingContextChanged);
			}
		}

		static void BindingContextPropertyBindingChanging(BindableObject bindable, BindingBase oldBindingBase, BindingBase newBindingBase)
		{
			object context = bindable._inheritedContext;
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
			bindable.ApplyBindings(skipBindingContext: true, fromBindingContextChanged: true);
			bindable.OnBindingContextChanged();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		BindablePropertyContext CreateAndAddContext(BindableProperty property)
		{
			var defaultValueCreator = property.DefaultValueCreator;
			var context = new BindablePropertyContext { Property = property, Value = defaultValueCreator != null ? defaultValueCreator(this) : property.DefaultValue };

			if (defaultValueCreator == null)
				context.Attributes = BindableContextAttributes.IsDefaultValue;
			else
				context.Attributes = BindableContextAttributes.IsDefaultValueCreated;

			_properties.Add(property, context);
			return context;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal BindablePropertyContext GetContext(BindableProperty property) => _properties.TryGetValue(property, out var result) ? result : null;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		BindablePropertyContext GetOrCreateContext(BindableProperty property) => GetContext(property) ?? CreateAndAddContext(property);

		void RemoveBinding(BindableProperty property, BindablePropertyContext context)
		{
			context.Binding.Unapply();

			property.BindingChanging?.Invoke(this, context.Binding, null);

			context.Binding = null;
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

			object currentValue = bpcontext.Value;

			if (property.ValidateValue != null && !property.ValidateValue(this, currentValue))
				throw new ArgumentException($"Value is an invalid value for {property.PropertyName}", nameof(currentValue));

			property.CoerceValue?.Invoke(this, currentValue);
		}

		[Flags]
		internal enum BindableContextAttributes
		{
			IsManuallySet = 1 << 0,
			IsBeingSet = 1 << 1,
			IsDynamicResource = 1 << 2,
			IsSetFromStyle = 1 << 3,
			IsDefaultValue = 1 << 4,
			IsDefaultValueCreated = 1 << 5,
		}

		internal class BindablePropertyContext
		{
			public BindableContextAttributes Attributes;
			public BindingBase Binding;
			public Queue<SetValueArgs> DelayedSetters;
			public BindableProperty Property;
			public object Value;

			public bool StyleValueSet;
			public object StyleValue;
		}

		[Flags]
		internal enum SetValuePrivateFlags
		{
			None = 0,
			CheckAccess = 1 << 0,
			Silent = 1 << 1,
			ManuallySet = 1 << 2,
			FromStyle = 1 << 3,
			Converted = 1 << 4,
			Default = CheckAccess
		}

		internal class SetValueArgs
		{
			public readonly SetValueFlags Attributes;
			public readonly BindablePropertyContext Context;
			public readonly bool CurrentlyApplying;
			public readonly BindableProperty Property;
			public readonly object Value;

			public SetValueArgs(BindableProperty property, BindablePropertyContext context, object value, bool currentlyApplying, SetValueFlags attributes)
			{
				Property = property;
				Context = context;
				Value = value;
				CurrentlyApplying = currentlyApplying;
				Attributes = attributes;
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
