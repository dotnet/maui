using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

using static System.String;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class NativeBindingHelpers
	{
		public static void SetBinding<TNativeView>(TNativeView target, string targetProperty, BindingBase bindingBase, string updateSourceEventName = null) where TNativeView : class
		{
			var binding = bindingBase as Binding;
			//This will allow setting bindings from Xaml by reusing the MarkupExtension
			if (IsNullOrEmpty(updateSourceEventName) && binding != null && !IsNullOrEmpty(binding.UpdateSourceEventName))
				updateSourceEventName = binding.UpdateSourceEventName;
			INotifyPropertyChanged eventWrapper = null;
			if (!IsNullOrEmpty(updateSourceEventName))
				eventWrapper = new EventWrapper(target, targetProperty, updateSourceEventName);

			SetBinding(target, targetProperty, bindingBase, eventWrapper);
		}

		public static void SetBinding<TNativeView>(TNativeView target, string targetProperty, BindingBase bindingBase, INotifyPropertyChanged propertyChanged) where TNativeView : class
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (IsNullOrEmpty(targetProperty))
				throw new ArgumentNullException(nameof(targetProperty));

			var binding = bindingBase as Binding;
			var proxy = BindableObjectProxy<TNativeView>.BindableObjectProxies.GetValue(target, (TNativeView key) => new BindableObjectProxy<TNativeView>(key));
			BindableProperty bindableProperty = null;
			propertyChanged = propertyChanged ?? target as INotifyPropertyChanged;
			var propertyType = target.GetType().GetProperty(targetProperty)?.PropertyType;
			var defaultValue = target.GetType().GetProperty(targetProperty)?.GetMethod.Invoke(target, new object[] { });
			bindableProperty = CreateBindableProperty<TNativeView>(targetProperty, propertyType, defaultValue);
			if (binding != null && binding.Mode != BindingMode.OneWay && propertyChanged != null)
				propertyChanged.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName != targetProperty)
						return;
					SetValueFromNative<TNativeView>(sender as TNativeView, targetProperty, bindableProperty);
					//we need to keep the listener around he same time we have the proxy
					proxy.NativeINPCListener = propertyChanged;
				};

			if (binding != null && binding.Mode != BindingMode.OneWay)
				SetValueFromNative(target, targetProperty, bindableProperty);

			proxy.SetBinding(bindableProperty, bindingBase);
		}

		static BindableProperty CreateBindableProperty<TNativeView>(string targetProperty, Type propertyType = null, object defaultValue = null) where TNativeView : class
		{
			propertyType = propertyType ?? typeof(object);
			defaultValue = defaultValue ?? (propertyType.GetTypeInfo().IsValueType ? Activator.CreateInstance(propertyType) : null);
			return BindableProperty.Create(
				targetProperty,
				propertyType,
				typeof(BindableObjectProxy<TNativeView>),
				defaultValue: defaultValue,
				defaultBindingMode: BindingMode.Default,
				propertyChanged: (bindable, oldValue, newValue) =>
				{
					TNativeView nativeView;
					if ((bindable as BindableObjectProxy<TNativeView>).TargetReference.TryGetTarget(out nativeView))
						SetNativeValue(nativeView, targetProperty, newValue);
				}
			);
		}

		static void SetNativeValue<TNativeView>(TNativeView target, string targetProperty, object newValue) where TNativeView : class
		{
			var mi = target.GetType().GetProperty(targetProperty)?.SetMethod;
			if (mi == null)
				throw new InvalidOperationException(Format("Native Binding on {0}.{1} failed due to missing or inaccessible property", target.GetType(), targetProperty));
			mi.Invoke(target, new[] { newValue });
		}

		static void SetValueFromNative<TNativeView>(TNativeView target, string targetProperty, BindableProperty bindableProperty) where TNativeView : class
		{
			BindableObjectProxy<TNativeView> proxy;
			if (!BindableObjectProxy<TNativeView>.BindableObjectProxies.TryGetValue(target, out proxy))
				return;
			SetValueFromRenderer(proxy, bindableProperty, target.GetType().GetProperty(targetProperty)?.GetMethod.Invoke(target, new object[] { }));
		}

		static void SetValueFromRenderer(BindableObject bindable, BindableProperty property, object value)
		{
			bindable.SetValueCore(property, value);
		}

		public static void SetBinding<TNativeView>(TNativeView target, BindableProperty targetProperty, BindingBase binding) where TNativeView : class
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (targetProperty == null)
				throw new ArgumentNullException(nameof(targetProperty));
			if (binding == null)
				throw new ArgumentNullException(nameof(binding));

			var proxy = BindableObjectProxy<TNativeView>.BindableObjectProxies.GetValue(target, (TNativeView key) => new BindableObjectProxy<TNativeView>(key));
			proxy.BindingsBackpack.Add(new KeyValuePair<BindableProperty, BindingBase>(targetProperty, binding));
		}

		public static void SetValue<TNativeView>(TNativeView target, BindableProperty targetProperty, object value) where TNativeView : class
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (targetProperty == null)
				throw new ArgumentNullException(nameof(targetProperty));

			var proxy = BindableObjectProxy<TNativeView>.BindableObjectProxies.GetValue(target, (TNativeView key) => new BindableObjectProxy<TNativeView>(key));
			proxy.ValuesBackpack.Add(new KeyValuePair<BindableProperty, object>(targetProperty, value));
		}

		public static void SetBindingContext<TNativeView>(TNativeView target, object bindingContext, Func<TNativeView, IEnumerable<TNativeView>> getChild = null) where TNativeView : class
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));

			var proxy = BindableObjectProxy<TNativeView>.BindableObjectProxies.GetValue(target, (TNativeView key) => new BindableObjectProxy<TNativeView>(key));
			proxy.BindingContext = bindingContext;
			if (getChild == null)
				return;
			var children = getChild(target);
			if (children == null)
				return;
			foreach (var child in children)
				if (child != null)
					SetBindingContext(child, bindingContext, getChild);
		}

		public static void TransferBindablePropertiesToWrapper<TNativeView, TNativeWrapper>(TNativeView nativeView, TNativeWrapper wrapper)
			where TNativeView : class
			where TNativeWrapper : View
		{
			BindableObjectProxy<TNativeView> proxy;
			if (!BindableObjectProxy<TNativeView>.BindableObjectProxies.TryGetValue(nativeView, out proxy))
				return;
			proxy.TransferAttachedPropertiesTo(wrapper);
		}

		class EventWrapper : INotifyPropertyChanged
		{
			string TargetProperty { get; set; }
			static readonly MethodInfo s_handlerinfo = typeof(EventWrapper).GetRuntimeMethods().Single(mi => mi.Name == "OnPropertyChanged" && mi.IsPublic == false);

			public EventWrapper(object target, string targetProperty, string updateSourceEventName)
			{
				TargetProperty = targetProperty;
				Delegate handlerDelegate = null;
				EventInfo updateSourceEvent = null;
				try
				{
					updateSourceEvent = target.GetType().GetRuntimeEvent(updateSourceEventName);
					handlerDelegate = s_handlerinfo.CreateDelegate(updateSourceEvent.EventHandlerType, this);
				}
				catch (Exception)
				{
					throw new ArgumentException(Format("No declared or accessible event {0} on {1}", updateSourceEventName, target.GetType()), nameof(updateSourceEventName));
				}
				if (updateSourceEvent != null && handlerDelegate != null)
					updateSourceEvent.AddEventHandler(target, handlerDelegate);
			}

			[Preserve]
			void OnPropertyChanged(object sender, EventArgs e)
			{
				PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(TargetProperty));
			}

			public event PropertyChangedEventHandler PropertyChanged;
		}

		//This needs to be internal for testing purposes
		internal class BindableObjectProxy<TNativeView> : BindableObject where TNativeView : class
		{
			public static ConditionalWeakTable<TNativeView, BindableObjectProxy<TNativeView>> BindableObjectProxies { get; } = new ConditionalWeakTable<TNativeView, BindableObjectProxy<TNativeView>>();
			public WeakReference<TNativeView> TargetReference { get; set; }
			public IList<KeyValuePair<BindableProperty, BindingBase>> BindingsBackpack { get; } = new List<KeyValuePair<BindableProperty, BindingBase>>();
			public IList<KeyValuePair<BindableProperty, object>> ValuesBackpack { get; } = new List<KeyValuePair<BindableProperty, object>>();
			public INotifyPropertyChanged NativeINPCListener;

			public BindableObjectProxy(TNativeView target)
			{
				TargetReference = new WeakReference<TNativeView>(target);
			}

			public void TransferAttachedPropertiesTo(View wrapper)
			{
				foreach (var kvp in BindingsBackpack)
					wrapper.SetBinding(kvp.Key, kvp.Value);
				foreach (var kvp in ValuesBackpack)
					wrapper.SetValue(kvp.Key, kvp.Value);
			}
		}
	}
}