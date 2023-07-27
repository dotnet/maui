#nullable disable
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
	/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="Type[@FullName='Microsoft.Maui.Controls.BindableObject']/Docs/*" />
	public abstract class BindableObject : INotifyPropertyChanged, IDynamicResourceHandler
	{
		IDispatcher _dispatcher;

		// return the dispatcher that was available when this was created,
		// otherwise try to find the nearest dispatcher (probably the window/app)
		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='Dispatcher']/Docs/*" />
		public IDispatcher Dispatcher =>
			_dispatcher ??= this.FindDispatcher();

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public BindableObject()
		{
			// try use the current thread's dispatcher
			_dispatcher = Dispatching.Dispatcher.GetForCurrentThread();
		}

		readonly Dictionary<BindableProperty, BindablePropertyContext> _properties = new Dictionary<BindableProperty, BindablePropertyContext>(4);
		bool _applying;
		WeakReference _inheritedContext;

		/// <summary>Bindable property for <see cref="BindingContext"/>.</summary>
		public static readonly BindableProperty BindingContextProperty =
			BindableProperty.Create(nameof(BindingContext), typeof(object), typeof(BindableObject), default(object),
									BindingMode.OneWay, null, BindingContextPropertyChanged, null, null, BindingContextPropertyBindingChanging);

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='BindingContext']/Docs/*" />
		public object BindingContext
		{
			get => _inheritedContext?.Target ?? GetValue(BindingContextProperty);
			set => SetValue(BindingContextProperty, value);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public event PropertyChangingEventHandler PropertyChanging;
		public event EventHandler BindingContextChanged;

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='ClearValue'][1]/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='ClearValue'][2]/Docs/*" />
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

			var original = bpcontext.Values.LastOrDefault().Value;
			var newValue = bpcontext.Values.Count >= 2 ? bpcontext.Values[bpcontext.Values.Keys[bpcontext.Values.Count - 2]] : null;
			var changed = !Equals(original, newValue);
			if (changed)
			{
				property.PropertyChanging?.Invoke(this, original, newValue);
				OnPropertyChanging(property.PropertyName);
			}
			bpcontext.Values.Remove(specificity);
			if (changed)
			{
				OnPropertyChanged(property.PropertyName);
				property.PropertyChanged?.Invoke(this, original, newValue);
			}


		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='GetValue']/Docs/*" />
		public object GetValue(BindableProperty property)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			var context = property.DefaultValueCreator != null ? GetOrCreateContext(property) : GetContext(property);

			return context == null ? property.DefaultValue : context.Values.Last().Value;
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
					Current = new LocalValueEntry(_propertiesEnumerator.Current.Key, _propertiesEnumerator.Current.Value.Values.LastOrDefault().Value, _propertiesEnumerator.Current.Value.Attributes);
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
					resultArray[i].IsSet = context.Values.LastOrDefault().Key.CompareTo(SetterSpecificity.DefaultValue) != 0;
					resultArray[i].Value = (T)context.Values.LastOrDefault().Value;
				}
				else
				{
					resultArray[i].IsSet = false;
					resultArray[i].Value = default(T);
				}
			}

			return resultArray;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='IsSet']/Docs/*" />
		public bool IsSet(BindableProperty targetProperty)
		{
			var bpcontext = GetContext(targetProperty ?? throw new ArgumentNullException(nameof(targetProperty)));
			if (bpcontext == null)
				return false;
			if ((bpcontext.Attributes & BindableContextAttributes.IsDefaultValueCreated) == BindableContextAttributes.IsDefaultValueCreated)
				return true;
			return bpcontext.Values.LastOrDefault().Key.CompareTo(SetterSpecificity.DefaultValue) != 0;
		}


		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='RemoveBinding']/Docs/*" />
		public void RemoveBinding(BindableProperty property)
		{
			BindablePropertyContext context = GetContext(property ?? throw new ArgumentNullException(nameof(property)));

			if (context?.Binding != null)
				RemoveBinding(property, context);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='SetBinding']/Docs/*" />
		public void SetBinding(BindableProperty targetProperty, BindingBase binding)
			=> SetBinding(targetProperty, binding, SetterSpecificity.FromBinding);

		//FIXME, use specificity
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

			context.Binding?.Unapply();
			context.BindingSpecificity = specificity;

			BindingBase oldBinding = context.Binding;
			context.Binding = binding ?? throw new ArgumentNullException(nameof(binding));

			targetProperty.BindingChanging?.Invoke(this, oldBinding, binding);

			binding.Apply(BindingContext, this, targetProperty, false, specificity);


		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='SetInheritedBindingContext']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetInheritedBindingContext(BindableObject bindable, object value)
		{
			//I wonder if we coulnd't treat bindingcoutext with specificities
			BindablePropertyContext bpContext = bindable.GetContext(BindingContextProperty);
			if (bpContext != null && bpContext.Values.LastOrDefault().Key.CompareTo(SetterSpecificity.ManualValueSetter) >= 0)
				return;

			object oldContext = bindable._inheritedContext?.Target;

			if (ReferenceEquals(oldContext, value))
				return;

			if (bpContext != null && oldContext == null)
				oldContext = bpContext.Values.LastOrDefault().Value;

			if (bpContext != null && bpContext.Binding != null)
			{
				bpContext.Binding.Context = value;
				bindable._inheritedContext = null;
			}
			else
			{
				bindable._inheritedContext = new WeakReference(value);
			}

			bindable.ApplyBindings(skipBindingContext: false, fromBindingContextChanged: true);
			bindable.OnBindingContextChanged();
		}

		protected void ApplyBindings() => ApplyBindings(skipBindingContext: false, fromBindingContextChanged: false);

		protected virtual void OnBindingContextChanged()
		{
			BindingContextChanged?.Invoke(this, EventArgs.Empty);

			if (Shell.GetBackButtonBehavior(this) is BackButtonBehavior buttonBehavior)
				SetInheritedBindingContext(buttonBehavior, BindingContext);

			if (Shell.GetSearchHandler(this) is SearchHandler searchHandler)
				SetInheritedBindingContext(searchHandler, BindingContext);
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
			=> PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

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

		internal virtual void OnSetDynamicResource(BindableProperty property, string key, SetterSpecificity specificity)
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

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='SetValue'][1]/Docs/*" />
		public void SetValue(BindableProperty property, object value)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			if (property.IsReadOnly)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<BindableObject>()?.LogWarning($"Cannot set the BindableProperty \"{property.PropertyName}\" because it is readonly.");
				return;
			}
			SetValueCore(property, value, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearDynamicResource, SetValuePrivateFlags.Default, SetterSpecificity.ManualValueSetter);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='SetValue'][2]/Docs/*" />
		public void SetValue(BindablePropertyKey propertyKey, object value)
		{
			if (propertyKey == null)
				throw new ArgumentNullException(nameof(propertyKey));

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

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='SetValueCore']/Docs/*" />
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
				context.Attributes |= BindableContextAttributes.IsBeingSet;
				SetValueActual(property, context, value, currentlyApplying, attributes, specificity);

				Queue<SetValueArgs> delayQueue = context.DelayedSetters;
				if (delayQueue != null)
				{
					while (delayQueue.Count > 0)
					{
						SetValueArgs s = delayQueue.Dequeue();
						SetValueActual(s.Property, s.Context, s.Value, s.CurrentlyApplying, s.Attributes, s.Specificity);
					}

					context.DelayedSetters = null;
				}

				context.Attributes &= ~BindableContextAttributes.IsBeingSet;
			}
		}

		void SetValueActual(BindableProperty property, BindablePropertyContext context, object value, bool currentlyApplying, SetValueFlags attributes, SetterSpecificity specificity, bool silent = false)
		{
			object original = context.Values.LastOrDefault().Value;
			var originalSpecificity = context.Values.LastOrDefault().Key;

			//if the last value was set from handler, override it
			if (specificity != SetterSpecificity.FromHandler
				&& originalSpecificity == SetterSpecificity.FromHandler)
			{
				context.Values.Remove(SetterSpecificity.FromHandler);
				originalSpecificity = context.Values.LastOrDefault().Key;
			}

			//We keep setter of lower specificity so we can unapply
			if (specificity.CompareTo(originalSpecificity) < 0)
			{
				context.Values[specificity] = value;
				return;
			}

			bool raiseOnEqual = (attributes & SetValueFlags.RaiseOnEqual) != 0;

			bool clearDynamicResources = (attributes & SetValueFlags.ClearDynamicResource) != 0;
			bool clearOneWayBindings = (attributes & SetValueFlags.ClearOneWayBindings) != 0 && specificity != SetterSpecificity.FromHandler;
			bool clearTwoWayBindings = (attributes & SetValueFlags.ClearTwoWayBindings) != 0 && specificity != SetterSpecificity.FromHandler;

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

			BindingBase binding = context.Binding;

			if (!silent && (!sameValue || raiseOnEqual))
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
				binding.Apply(BindingContext, this, context.Property, fromBindingContextChanged, context.BindingSpecificity);
			}
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
			bindable.ApplyBindings(skipBindingContext: true, fromBindingContextChanged: true);
			bindable.OnBindingContextChanged();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		BindablePropertyContext CreateAndAddContext(BindableProperty property)
		{
			var defaultValueCreator = property.DefaultValueCreator;
			var context = new BindablePropertyContext { Property = property };
			context.Values.Add(SetterSpecificity.DefaultValue, defaultValueCreator != null ? defaultValueCreator(this) : property.DefaultValue);

			if (defaultValueCreator != null)
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

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='CoerceValue'][1]/Docs/*" />
		public void CoerceValue(BindableProperty property) => CoerceValue(property, checkAccess: true);

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObject.xml" path="//Member[@MemberName='CoerceValue'][2]/Docs/*" />
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

			object currentValue = bpcontext.Values.LastOrDefault().Value;

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

		internal class BindablePropertyContext
		{
			public BindableContextAttributes Attributes;

			//TODO should be a list of bindings/specificity
			public BindingBase Binding;
			public SetterSpecificity BindingSpecificity = SetterSpecificity.FromBinding;

			public Queue<SetValueArgs> DelayedSetters;
			public BindableProperty Property;
			public SortedList<SetterSpecificity, object> Values = new();
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

		internal class SetValueArgs
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
