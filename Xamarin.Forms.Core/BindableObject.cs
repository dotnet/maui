using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public abstract class BindableObject : INotifyPropertyChanged, IDynamicResourceHandler
	{
		public static readonly BindableProperty BindingContextProperty =
			BindableProperty.Create("BindingContext", typeof(object), typeof(BindableObject), default(object),
									BindingMode.OneWay, null, BindingContextPropertyChanged, null, null, BindingContextPropertyBindingChanging);

		readonly List<BindablePropertyContext> _properties = new List<BindablePropertyContext>(4);

		bool _applying;
		object _inheritedContext;

		public object BindingContext
		{
			get { return _inheritedContext ?? GetValue(BindingContextProperty); }
			set { SetValue(BindingContextProperty, value); }
		}

		void IDynamicResourceHandler.SetDynamicResource(BindableProperty property, string key)
		{
			SetDynamicResource(property, key, false);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event EventHandler BindingContextChanged;

		internal void ClearValue(BindableProperty property, bool fromStyle)
		{
			ClearValue(property, fromStyle: fromStyle, checkAccess: true);
		}

		public void ClearValue(BindableProperty property)
		{
			ClearValue(property, fromStyle: false, checkAccess: true);
		}

		public void ClearValue(BindablePropertyKey propertyKey)
		{
			if (propertyKey == null)
				throw new ArgumentNullException("propertyKey");

			ClearValue(propertyKey.BindableProperty, fromStyle:false, checkAccess: false);
		}

		public bool IsSet(BindableProperty targetProperty)
		{
			if (targetProperty == null)
				throw new ArgumentNullException(nameof(targetProperty));

			return GetContext(targetProperty) != null;
		}

		public object GetValue(BindableProperty property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			BindablePropertyContext context = property.DefaultValueCreator != null ? GetOrCreateContext(property) : GetContext(property);

			if (context == null)
				return property.DefaultValue;

			return context.Value;
		}

		public event PropertyChangingEventHandler PropertyChanging;

		public void RemoveBinding(BindableProperty property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			BindablePropertyContext context = GetContext(property);
			if (context == null || context.Binding == null)
				return;

			RemoveBinding(property, context);
		}

		public void SetBinding(BindableProperty targetProperty, BindingBase binding)
		{
			SetBinding(targetProperty, binding, false);
		}

		public void SetValue(BindableProperty property, object value)
		{
			SetValue(property, value, false, true);
		}

		public void SetValue(BindablePropertyKey propertyKey, object value)
		{
			if (propertyKey == null)
				throw new ArgumentNullException("propertyKey");

			SetValue(propertyKey.BindableProperty, value, false, false);
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

			bindable.ApplyBindings(skipBindingContext:false, fromBindingContextChanged:true);
			bindable.OnBindingContextChanged();
		}

		protected void ApplyBindings()
		{
			ApplyBindings(skipBindingContext: false, fromBindingContextChanged: false);
		}

		protected virtual void OnBindingContextChanged()
		{
			BindingContextChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
		{
			PropertyChangingEventHandler changing = PropertyChanging;
			if (changing != null)
				changing(this, new PropertyChangingEventArgs(propertyName));
		}

		protected void UnapplyBindings()
		{
			for (int i = 0, _propertiesCount = _properties.Count; i < _propertiesCount; i++) {
				BindablePropertyContext context = _properties [i];
				if (context.Binding == null)
					continue;

				context.Binding.Unapply();
			}
		}

		internal bool GetIsBound(BindableProperty targetProperty)
		{
			if (targetProperty == null)
				throw new ArgumentNullException("targetProperty");

			BindablePropertyContext bpcontext = GetContext(targetProperty);
			return bpcontext != null && bpcontext.Binding != null;
		}

		internal bool GetIsDefault(BindableProperty targetProperty)
		{
			if (targetProperty == null)
				throw new ArgumentNullException(nameof(targetProperty));

			return GetContext(targetProperty) == null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public object[] GetValues(BindableProperty property0, BindableProperty property1)
		{
			var values = new object[2];

			for (var i = 0; i < _properties.Count; i++)
			{
				BindablePropertyContext context = _properties[i];

				if (ReferenceEquals(context.Property, property0))
				{
					values[0] = context.Value;
					property0 = null;
				}
				else if (ReferenceEquals(context.Property, property1))
				{
					values[1] = context.Value;
					property1 = null;
				}

				if (property0 == null && property1 == null)
					return values;
			}

			if (!ReferenceEquals(property0, null))
				values[0] = property0.DefaultValueCreator == null ? property0.DefaultValue : CreateAndAddContext(property0).Value;
			if (!ReferenceEquals(property1, null))
				values[1] = property1.DefaultValueCreator == null ? property1.DefaultValue : CreateAndAddContext(property1).Value;

			return values;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public object[] GetValues(BindableProperty property0, BindableProperty property1, BindableProperty property2)
		{
			var values = new object[3];

			for (var i = 0; i < _properties.Count; i++)
			{
				BindablePropertyContext context = _properties[i];

				if (ReferenceEquals(context.Property, property0))
				{
					values[0] = context.Value;
					property0 = null;
				}
				else if (ReferenceEquals(context.Property, property1))
				{
					values[1] = context.Value;
					property1 = null;
				}
				else if (ReferenceEquals(context.Property, property2))
				{
					values[2] = context.Value;
					property2 = null;
				}

				if (property0 == null && property1 == null && property2 == null)
					return values;
			}

			if (!ReferenceEquals(property0, null))
				values[0] = property0.DefaultValueCreator == null ? property0.DefaultValue : CreateAndAddContext(property0).Value;
			if (!ReferenceEquals(property1, null))
				values[1] = property1.DefaultValueCreator == null ? property1.DefaultValue : CreateAndAddContext(property1).Value;
			if (!ReferenceEquals(property2, null))
				values[2] = property2.DefaultValueCreator == null ? property2.DefaultValue : CreateAndAddContext(property2).Value;

			return values;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal object[] GetValues(params BindableProperty[] properties)
		{
			var values = new object[properties.Length];
			for (var i = 0; i < _properties.Count; i++) {
				var context = _properties[i];
				var index = properties.IndexOf(context.Property);
				if (index < 0)
					continue;
				values[index] = context.Value;
			}
			for (var i = 0; i < values.Length; i++) {
				if (!ReferenceEquals(values[i], null))
					continue;
				values[i] = properties[i].DefaultValueCreator == null ? properties[i].DefaultValue : CreateAndAddContext(properties[i]).Value;
			}
			return values;
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
				throw new ArgumentNullException("property");

			OnRemoveDynamicResource(property);
			BindablePropertyContext context = GetOrCreateContext(property);
			context.Attributes &= ~BindableContextAttributes.IsDynamicResource;
		}

		internal void SetBinding(BindableProperty targetProperty, BindingBase binding, bool fromStyle)
		{
			if (targetProperty == null)
				throw new ArgumentNullException("targetProperty");
			if (binding == null)
				throw new ArgumentNullException("binding");

			BindablePropertyContext context = null;
			if (fromStyle && (context = GetContext(targetProperty)) != null && (context.Attributes & BindableContextAttributes.IsDefaultValue) == 0 &&
				(context.Attributes & BindableContextAttributes.IsSetFromStyle) == 0)
				return;

			context = context ?? GetOrCreateContext(targetProperty);
			if (fromStyle)
				context.Attributes |= BindableContextAttributes.IsSetFromStyle;
			else
				context.Attributes &= ~BindableContextAttributes.IsSetFromStyle;

			if (context.Binding != null)
				context.Binding.Unapply();

			BindingBase oldBinding = context.Binding;
			context.Binding = binding;

			if (targetProperty.BindingChanging != null)
				targetProperty.BindingChanging(this, oldBinding, binding);

			binding.Apply(BindingContext, this, targetProperty);
		}

		internal void SetDynamicResource(BindableProperty property, string key)
		{
			SetDynamicResource(property, key, false);
		}

		internal void SetDynamicResource(BindableProperty property, string key, bool fromStyle)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException(nameof(key));

			BindablePropertyContext context = null;
			if (fromStyle && (context = GetContext(property)) != null && (context.Attributes & BindableContextAttributes.IsDefaultValue) == 0 &&
				(context.Attributes & BindableContextAttributes.IsSetFromStyle) == 0)
				return;

			context = context ?? GetOrCreateContext(property);

			context.Attributes |= BindableContextAttributes.IsDynamicResource;
			if (fromStyle)
				context.Attributes |= BindableContextAttributes.IsSetFromStyle;
			else
				context.Attributes &= ~BindableContextAttributes.IsSetFromStyle;

			OnSetDynamicResource(property, key);
		}

		internal void SetValue(BindableProperty property, object value, bool fromStyle)
		{
			SetValue(property, value, fromStyle, true);
		}

		internal void SetValueCore(BindablePropertyKey propertyKey, object value, SetValueFlags attributes = SetValueFlags.None)
		{
			SetValueCore(propertyKey.BindableProperty, value, attributes, SetValuePrivateFlags.None);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetValueCore(BindableProperty property, object value, SetValueFlags attributes = SetValueFlags.None)
		{
			SetValueCore(property, value, attributes, SetValuePrivateFlags.Default);
		}

		internal void SetValueCore(BindableProperty property, object value, SetValueFlags attributes, SetValuePrivateFlags privateAttributes)
		{
			bool checkAccess = (privateAttributes & SetValuePrivateFlags.CheckAccess) != 0;
			bool manuallySet = (privateAttributes & SetValuePrivateFlags.ManuallySet) != 0;
			bool silent = (privateAttributes & SetValuePrivateFlags.Silent) != 0;
			bool fromStyle = (privateAttributes & SetValuePrivateFlags.FromStyle) != 0;
			bool converted = (privateAttributes & SetValuePrivateFlags.Converted) != 0;

			if (property == null)
				throw new ArgumentNullException("property");
			if (checkAccess && property.IsReadOnly)
			{
				Debug.WriteLine("Can not set the BindableProperty \"{0}\" because it is readonly.", property.PropertyName);
				return;
			}

			if (!converted && !property.TryConvert(ref value))
			{
				Log.Warning("SetValue", "Can not convert {0} to type '{1}'", value, property.ReturnType);
				return;
			}

			if (property.ValidateValue != null && !property.ValidateValue(this, value))
				throw new ArgumentException("Value was an invalid value for " + property.PropertyName, "value");

			if (property.CoerceValue != null)
				value = property.CoerceValue(this, value);

			BindablePropertyContext context = GetOrCreateContext(property);
			if (manuallySet) {
				context.Attributes |= BindableContextAttributes.IsManuallySet;
				context.Attributes &= ~BindableContextAttributes.IsSetFromStyle;
			} else
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

		internal void ApplyBindings(bool skipBindingContext, bool fromBindingContextChanged)
		{
			var prop = _properties.ToArray();
			for (int i = 0, propLength = prop.Length; i < propLength; i++) {
				BindablePropertyContext context = prop [i];
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
			bindable.ApplyBindings(skipBindingContext: true, fromBindingContextChanged:true);
			bindable.OnBindingContextChanged();
		}

		void ClearValue(BindableProperty property, bool fromStyle, bool checkAccess)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			if (checkAccess && property.IsReadOnly)
				throw new InvalidOperationException(string.Format("The BindableProperty \"{0}\" is readonly.", property.PropertyName));

			BindablePropertyContext bpcontext = GetContext(property);
			if (bpcontext == null)
				return;

			if (   fromStyle && bpcontext != null
				&& (bpcontext.Attributes & BindableContextAttributes.IsDefaultValue) != 0
				&& (bpcontext.Attributes & BindableContextAttributes.IsSetFromStyle) == 0)
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
			bpcontext.Attributes |= BindableContextAttributes.IsDefaultValue;

			if (!same)
			{
				OnPropertyChanged(property.PropertyName);
				property.PropertyChanged?.Invoke(this, original, newValue);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		BindablePropertyContext CreateAndAddContext(BindableProperty property)
		{
			var context = new BindablePropertyContext { Property = property, Value = property.DefaultValueCreator != null ? property.DefaultValueCreator(this) : property.DefaultValue };

			if (property.DefaultValueCreator != null)
				context.Attributes = BindableContextAttributes.IsDefaultValue;

			_properties.Add(context);
			return context;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		BindablePropertyContext GetContext(BindableProperty property)
		{
			List<BindablePropertyContext> properties = _properties;

			for (var i = 0; i < properties.Count; i++)
			{
				BindablePropertyContext context = properties[i];
				if (ReferenceEquals(context.Property, property))
					return context;
			}

			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		BindablePropertyContext GetOrCreateContext(BindableProperty property)
		{
			BindablePropertyContext context = GetContext(property);
			if (context == null)
			{
				context = CreateAndAddContext(property);
			}

			return context;
		}

		void RemoveBinding(BindableProperty property, BindablePropertyContext context)
		{
			context.Binding.Unapply();

			if (property.BindingChanging != null)
				property.BindingChanging(this, context.Binding, null);

			context.Binding = null;
		}

		void SetValue(BindableProperty property, object value, bool fromStyle, bool checkAccess)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			if (checkAccess && property.IsReadOnly)
				throw new InvalidOperationException(string.Format("The BindableProperty \"{0}\" is readonly.", property.PropertyName));

			BindablePropertyContext context = null;
			if (fromStyle && (context = GetContext(property)) != null && (context.Attributes & BindableContextAttributes.IsDefaultValue) == 0 &&
				(context.Attributes & BindableContextAttributes.IsSetFromStyle) == 0)
				return;

			SetValueCore(property, value, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearDynamicResource,
				(fromStyle ? SetValuePrivateFlags.FromStyle : SetValuePrivateFlags.ManuallySet) | (checkAccess ? SetValuePrivateFlags.CheckAccess : 0));
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
				if (property.PropertyChanging != null)
					property.PropertyChanging(this, original, value);

				OnPropertyChanging(property.PropertyName);
			}

			if (!same || raiseOnEqual)
			{
				context.Value = value;
			}

			context.Attributes &= ~BindableContextAttributes.IsDefaultValue;

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

		[Flags]
		enum BindableContextAttributes
		{
			IsManuallySet = 1 << 0,
			IsBeingSet = 1 << 1,
			IsDynamicResource = 1 << 2,
			IsSetFromStyle = 1 << 3,
			IsDefaultValue = 1 << 4
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
