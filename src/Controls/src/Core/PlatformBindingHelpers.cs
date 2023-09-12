#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;

using static System.String;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PlatformBindingHelpers.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.PlatformBindingHelpers']/Docs/*" />	
	internal static class PlatformBindingHelpers
	{
		public static void SetBinding<TPlatformView>(TPlatformView target, string targetProperty, BindingBase bindingBase, string updateSourceEventName = null) where TPlatformView : class
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

		public static void SetBinding<TPlatformView>(TPlatformView target, string targetProperty, BindingBase bindingBase, INotifyPropertyChanged propertyChanged) where TPlatformView : class
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (IsNullOrEmpty(targetProperty))
				throw new ArgumentNullException(nameof(targetProperty));

			var binding = bindingBase as Binding;
			var proxy = BindableObjectProxy<TPlatformView>.BindableObjectProxies.GetValue(target, (TPlatformView key) => new BindableObjectProxy<TPlatformView>(key));
			BindableProperty bindableProperty = null;
			propertyChanged = propertyChanged ?? target as INotifyPropertyChanged;
			var targetPropertyInfo = target.GetType().GetProperty(targetProperty);
			var propertyType = targetPropertyInfo?.PropertyType;
			var defaultValue = targetPropertyInfo?.GetMethod.Invoke(target, Array.Empty<object>());
			bindableProperty = CreateBindableProperty<TPlatformView>(targetProperty, propertyType, defaultValue);
			if (binding != null && binding.Mode != BindingMode.OneWay && propertyChanged != null)
				propertyChanged.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName != targetProperty)
						return;
					SetValueFromNative<TPlatformView>(sender as TPlatformView, targetProperty, bindableProperty);
					//we need to keep the listener around he same time we have the proxy
					proxy.NativeINPCListener = propertyChanged;
				};

			if (binding != null && binding.Mode != BindingMode.OneWay)
				SetValueFromNative(target, targetProperty, bindableProperty);

			proxy.SetBinding(bindableProperty, bindingBase);
		}

		static BindableProperty CreateBindableProperty<TPlatformView>(string targetProperty, Type propertyType = null, object defaultValue = null) where TPlatformView : class
		{
			propertyType = propertyType ?? typeof(object);
			defaultValue = defaultValue ?? (propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null);
			return BindableProperty.Create(
				targetProperty,
				propertyType,
				typeof(BindableObjectProxy<TPlatformView>),
				defaultValue: defaultValue,
				defaultBindingMode: BindingMode.Default,
				propertyChanged: (bindable, oldValue, newValue) =>
				{
					TPlatformView platformView;
					if ((bindable as BindableObjectProxy<TPlatformView>).TargetReference.TryGetTarget(out platformView))
						SetPlatformValue(platformView, targetProperty, newValue);
				}
			);
		}

		static void SetPlatformValue<TPlatformView>(TPlatformView target, string targetProperty, object newValue) where TPlatformView : class
		{
			var mi = target.GetType().GetProperty(targetProperty)?.SetMethod;
			if (mi == null)
				throw new InvalidOperationException(Format("Native Binding on {0}.{1} failed due to missing or inaccessible property", target.GetType(), targetProperty));
			mi.Invoke(target, new[] { newValue });
		}

		static void SetValueFromNative<TPlatformView>(TPlatformView target, string targetProperty, BindableProperty bindableProperty) where TPlatformView : class
		{
			BindableObjectProxy<TPlatformView> proxy;
			if (!BindableObjectProxy<TPlatformView>.BindableObjectProxies.TryGetValue(target, out proxy))
				return;
			SetValueFromRenderer(proxy, bindableProperty, target.GetType().GetProperty(targetProperty)?.GetMethod.Invoke(target, new object[] { }));
		}

		static void SetValueFromRenderer(BindableObject bindable, BindableProperty property, object value)
		{
			bindable.SetValue(property, value);
		}

		public static void SetBinding<TPlatformView>(TPlatformView target, BindableProperty targetProperty, BindingBase binding) where TPlatformView : class
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (targetProperty == null)
				throw new ArgumentNullException(nameof(targetProperty));
			if (binding == null)
				throw new ArgumentNullException(nameof(binding));

			var proxy = BindableObjectProxy<TPlatformView>.BindableObjectProxies.GetValue(target, (TPlatformView key) => new BindableObjectProxy<TPlatformView>(key));
			proxy.BindingsBackpack.Add(new KeyValuePair<BindableProperty, BindingBase>(targetProperty, binding));
		}

		public static void SetValue<TPlatformView>(TPlatformView target, BindableProperty targetProperty, object value) where TPlatformView : class
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (targetProperty == null)
				throw new ArgumentNullException(nameof(targetProperty));

			var proxy = BindableObjectProxy<TPlatformView>.BindableObjectProxies.GetValue(target, (TPlatformView key) => new BindableObjectProxy<TPlatformView>(key));
			proxy.ValuesBackpack.Add(new KeyValuePair<BindableProperty, object>(targetProperty, value));
		}

		public static void SetBindingContext<TPlatformView>(TPlatformView target, object bindingContext, Func<TPlatformView, IEnumerable<TPlatformView>> getChild = null) where TPlatformView : class
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));

			var proxy = BindableObjectProxy<TPlatformView>.BindableObjectProxies.GetValue(target, (TPlatformView key) => new BindableObjectProxy<TPlatformView>(key));
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

		public static void TransferBindablePropertiesToWrapper<TPlatformView, TPlatformWrapper>(TPlatformView platformView, TPlatformWrapper wrapper)
			where TPlatformView : class
			where TPlatformWrapper : View
		{
			BindableObjectProxy<TPlatformView> proxy;
			if (!BindableObjectProxy<TPlatformView>.BindableObjectProxies.TryGetValue(platformView, out proxy))
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
		internal class BindableObjectProxy<TPlatformView> : BindableObject where TPlatformView : class
		{
			public static ConditionalWeakTable<TPlatformView, BindableObjectProxy<TPlatformView>> BindableObjectProxies { get; } = new ConditionalWeakTable<TPlatformView, BindableObjectProxy<TPlatformView>>();
			public WeakReference<TPlatformView> TargetReference { get; set; }
			public IList<KeyValuePair<BindableProperty, BindingBase>> BindingsBackpack { get; } = new List<KeyValuePair<BindableProperty, BindingBase>>();
			public IList<KeyValuePair<BindableProperty, object>> ValuesBackpack { get; } = new List<KeyValuePair<BindableProperty, object>>();
			public INotifyPropertyChanged NativeINPCListener;

			public BindableObjectProxy(TPlatformView target)
			{
				TargetReference = new WeakReference<TPlatformView>(target);
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
