using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public abstract class BindableObject : INotifyPropertyChanged, IDynamicResourceHandler
	{
		IDispatcher _dispatcher;
		public virtual IDispatcher Dispatcher
		{
			get
			{
				if (_dispatcher == null)
				{
					_dispatcher = this.GetDispatcher();
				}

				return _dispatcher;
			}
			internal set
			{
				_dispatcher = value;
			}
		}

		readonly Dictionary<BindableProperty, BindablePropertyContext> _properties = new Dictionary<BindableProperty, BindablePropertyContext>(4);
		bool _applying;
		object _inheritedContext;

		public static readonly BindableProperty BindingContextProperty =
			BindableProperty.Create("BindingContext", typeof(object), typeof(BindableObject), default(object),
									BindingMode.OneWay, null, BindingContextPropertyChanged, null, null, BindingContextPropertyBindingChanging);

		public object BindingContext
		{
			get => _inheritedContext ?? GetValue(BindingContextProperty);
			set => SetValue(BindingContextProperty, value);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public event PropertyChangingEventHandler PropertyChanging;
		public event EventHandler BindingContextChanged;

		public void ClearValue(BindableProperty property) => ClearValue(property, fromStyle: false, checkAccess: true);

		internal void ClearValue(BindableProperty property, bool fromStyle) => ClearValue(property, fromStyle: fromStyle, checkAccess: true);

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

			if (fromStyle && !CanBeSetFromStyle(property))
				return;

			object original = bpcontext.Value;

			object newValue = property.GetDefaultValue(this);

			bool same = Equals(original, newValue);
			if (!same)
			{
				property.PropertyChanging?.Invoke(this, original, newValue);

				OnPropertyChanging(property.PropertyName);
			}

			bpcontext.Attributes &= ~BindableContextAttributes.IsManuallySet;
			bpcontext.Value = newValue;
			if (property.DefaultValueCreator == null)
				bpcontext.Attributes |= BindableContextAttributes.IsDefaultValue;
			else
				bpcontext.Attributes |= BindableContextAttributes.IsDefaultValueCreated;

			if (!same)
			{
				OnPropertyChanged(property.PropertyName);
				property.PropertyChanged?.Invoke(this, original, newValue);
			}
		}

		public object GetValue(BindableProperty property)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			var context = property.DefaultValueCreator != null ? GetOrCreateContext(property) : GetContext(property);

			return context == null ? property.DefaultValue : context.Value;
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("GetValues is obsolete as of 4.0.0. Please use GetValue instead.")]
		public object[] GetValues(BindableProperty property0, BindableProperty property1) => new object[] { GetValue(property0), GetValue(property1) };

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("GetValues is obsolete as of 4.0.0. Please use GetValue instead.")]
		public object[] GetValues(BindableProperty property0, BindableProperty property1, BindableProperty property2) => new object[] { GetValue(property0), GetValue(property1), GetValue(property2) };

		public bool IsSet(BindableProperty targetProperty)
		{
			var bpcontext = GetContext(targetProperty ?? throw new ArgumentNullException(nameof(targetProperty)));
			return bpcontext != null
				&& (bpcontext.Attributes & BindableContextAttributes.IsDefaultValue) == 0;
		}


		public void RemoveBinding(BindableProperty property)
		{
			BindablePropertyContext context = GetContext(property ?? throw new ArgumentNullException(nameof(property)));

			if (context?.Binding != null)
				RemoveBinding(property, context);
		}

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

		protected void ApplyBindings() => ApplyBindings(skipBindingContext: false, fromBindingContextChanged: false);

		protected virtual void OnBindingContextChanged()
		{
			BindingContextChanged?.Invoke(this, EventArgs.Empty);
			if (Shell.GetBackButtonBehavior(this) is BackButtonBehavior buttonBehavior)
				SetInheritedBindingContext(buttonBehavior, BindingContext);

			if (Shell.GetSearchHandler(this) is SearchHandler searchHandler)
				SetInheritedBindingContext(searchHandler, BindingContext);

			if (Shell.GetTitleView(this) is View titleView)
				SetInheritedBindingContext(titleView, BindingContext);
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

		public void SetValue(BindableProperty property, object value) => SetValue(property, value, false, true);

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

			if (fromStyle && !CanBeSetFromStyle(property))
				return;

			SetValueCore(property, value, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearDynamicResource,
				(fromStyle ? SetValuePrivateFlags.FromStyle : SetValuePrivateFlags.ManuallySet) | (checkAccess ? SetValuePrivateFlags.CheckAccess : 0));
		}

		internal void SetValueCore(BindablePropertyKey propertyKey, object value, SetValueFlags attributes = SetValueFlags.None)
			=> SetValueCore(propertyKey.BindableProperty, value, attributes, SetValuePrivateFlags.None);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetValueCore(BindableProperty property, object value, SetValueFlags attributes = SetValueFlags.None)
			=> SetValueCore(property, value, attributes, SetValuePrivateFlags.Default);

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
				Log.Warning("BindableObject", $"Can not set the BindableProperty \"{property.PropertyName}\" because it is readonly.");
				return;
			}

			if (!converted && !property.TryConvert(ref value))
			{
				Log.Warning("SetValue", $"Can not convert {value} to type '{property.ReturnType}'");
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
		BindablePropertyContext GetContext(BindableProperty property) => _properties.TryGetValue(property, out var result) ? result : null;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		BindablePropertyContext GetOrCreateContext(BindableProperty property) => GetContext(property) ?? CreateAndAddContext(property);

		void RemoveBinding(BindableProperty property, BindablePropertyContext context)
		{
			context.Binding.Unapply();

			property.BindingChanging?.Invoke(this, context.Binding, null);

			context.Binding = null;
		}

		public void CoerceValue(BindableProperty property) => CoerceValue(property, checkAccess: true);

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
		enum BindableContextAttributes
		{
			IsManuallySet = 1 << 0,
			IsBeingSet = 1 << 1,
			IsDynamicResource = 1 << 2,
			IsSetFromStyle = 1 << 3,
			IsDefaultValue = 1 << 4,
			IsDefaultValueCreated = 1 << 5,
		}

		class BindablePropertyContext
		{
			public BindableContextAttributes Attributes;
			public BindingBase Binding;
			public Queue<SetValueArgs> DelayedSetters;
			public BindableProperty Property;
			public object Value;
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

		class SetValueArgs
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