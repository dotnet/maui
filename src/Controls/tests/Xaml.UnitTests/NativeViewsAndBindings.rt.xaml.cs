using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Xaml.Internals;
using Xunit;
using static System.String;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class MockNativeView
{
	public string Foo { get; set; }
	public int Bar { get; set; }
	public string Baz { get; set; }
	public IList<MockNativeView> SubViews { get; set; } = new List<MockNativeView>();
}

public class MockNativeViewWrapper : View
{
	public MockNativeView NativeView { get; }

	public MockNativeViewWrapper(MockNativeView nativeView)
	{
		nativeView.TransferbindablePropertiesToWrapper(this);
		NativeView = nativeView;
	}

	protected override void OnBindingContextChanged()
	{
		NativeView.SetBindingContext(BindingContext, nv => nv.SubViews);
		base.OnBindingContextChanged();
	}
}

public static class MockNativeViewExtensions
{
	public static void SetBindingContext(this MockNativeView target, object bindingContext, Func<MockNativeView, IEnumerable<MockNativeView>> getChild = null) => NativeBindingHelpers.SetBindingContext(target, bindingContext, getChild);
	public static void TransferbindablePropertiesToWrapper(this MockNativeView target, MockNativeViewWrapper wrapper) => NativeBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
	public static void SetBinding(this MockNativeView target, string targetProperty, BindingBase binding) => NativeBindingHelpers.SetBinding(target, targetProperty, binding);
	public static void SetBinding(this MockNativeView target, BindableProperty targetProperty, BindingBase binding) => NativeBindingHelpers.SetBinding(target, targetProperty, binding);
	public static void SetValue(this MockNativeView target, BindableProperty targetProperty, object value) => NativeBindingHelpers.SetValue(target, targetProperty, value);
}

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
		propertyChanged ??= target as INotifyPropertyChanged;
		var propertyType = target.GetType().GetProperty(targetProperty)?.PropertyType;
		var defaultValue = target.GetType().GetProperty(targetProperty)?.GetMethod.Invoke(target, Array.Empty<object>());
		bindableProperty = CreateBindableProperty<TNativeView>(targetProperty, propertyType, defaultValue);
		if (binding != null && binding.Mode != BindingMode.OneWay && propertyChanged != null)
			propertyChanged.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName != targetProperty)
					return;
				SetValueFromNative(sender as TNativeView, targetProperty, bindableProperty);
				//we need to keep the listener around he same time we have the proxy
				proxy.NativeINPCListener = propertyChanged;
			};

		if (binding != null && binding.Mode != BindingMode.OneWay)
			SetValueFromNative(target, targetProperty, bindableProperty);

		proxy.SetBinding(bindableProperty, bindingBase);
	}

	static BindableProperty CreateBindableProperty<TNativeView>(string targetProperty, Type propertyType = null, object defaultValue = null) where TNativeView : class
	{
		propertyType ??= typeof(object);
		defaultValue ??= (propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null);
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
		SetValueFromRenderer(proxy, bindableProperty, target.GetType().GetProperty(targetProperty)?.GetMethod.Invoke(target, Array.Empty<object>()));
	}

	static void SetValueFromRenderer(BindableObject bindable, BindableProperty property, object value) => bindable.SetValueCore(property, value);

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
		if (BindableObjectProxy<TNativeView>.BindableObjectProxies.TryGetValue(nativeView, out BindableObjectProxy<TNativeView> proxy))
			proxy.TransferAttachedPropertiesTo(wrapper);
	}

	class EventWrapper : INotifyPropertyChanged
	{
		string TargetProperty { get; set; }
		static readonly MethodInfo handlerinfo = typeof(EventWrapper).GetRuntimeMethods().Single(mi => mi.Name == "OnPropertyChanged" && mi.IsPublic == false);

		public EventWrapper(object target, string targetProperty, string updateSourceEventName)
		{
			TargetProperty = targetProperty;
			Delegate handlerDelegate;
			EventInfo updateSourceEvent;
			try
			{
				updateSourceEvent = target.GetType().GetRuntimeEvent(updateSourceEventName);
				handlerDelegate = handlerinfo.CreateDelegate(updateSourceEvent.EventHandlerType, this);
			}
			catch (Exception)
			{
				throw new ArgumentException(Format("No declared or accessible event {0} on {1}", updateSourceEventName, target.GetType()), nameof(updateSourceEventName));
			}
			if (updateSourceEvent != null && handlerDelegate != null)
				updateSourceEvent.AddEventHandler(target, handlerDelegate);
		}

		void OnPropertyChanged(object sender, EventArgs e) => PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(TargetProperty));

		public event PropertyChangedEventHandler PropertyChanged;
	}

	class BindableObjectProxy<TNativeView> : BindableObject where TNativeView : class
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

class MockNativeValueConverterService : INativeValueConverterService
{
	public bool ConvertTo(object value, Type toType, out object nativeValue)
	{
		nativeValue = null;
		if (typeof(MockNativeView).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View)))
		{
			nativeValue = new MockNativeViewWrapper((MockNativeView)value);
			return true;
		}
		return false;
	}
}

class MockNativeBindingService : INativeBindingService
{
	public bool TrySetBinding(object target, string propertyName, BindingBase binding)
	{
		var view = target as MockNativeView;
		if (view == null)
			return false;
		view.SetBinding(propertyName, binding);
		return true;

	}

	public bool TrySetBinding(object target, BindableProperty property, BindingBase binding)
	{
		var view = target as MockNativeView;
		if (view == null)
			return false;
		view.SetBinding(property, binding);
		return true;
	}

	public bool TrySetValue(object target, BindableProperty property, object value)
	{
		var view = target as MockNativeView;
		if (view == null)
			return false;
		view.SetValue(property, value);
		return true;
	}
}

public partial class NativeViewsAndBindings : ContentPage
{

	public NativeViewsAndBindings() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Test : BaseTestFixture
	{
		protected internal override void Setup()
		{
			base.Setup();
			AppInfo.SetCurrent(new Core.UnitTests.MockAppInfo());
			DependencyService.Register<INativeValueConverterService, MockNativeValueConverterService>();
			DependencyService.Register<INativeBindingService, MockNativeBindingService>();
		}

		protected internal override void TearDown()
		{
			AppInfo.SetCurrent(null);
			base.TearDown();
		}

		[Fact]
		public void NativeInContentView()
		{
			var inflator = XamlInflator.Runtime;
			var layout = new NativeViewsAndBindings(inflator);
			layout.BindingContext = new
			{
				Baz = "Bound Value",
				VerticalOption = LayoutOptions.EndAndExpand
			};
			var nativeView = layout.view0 as MockNativeView;

			var wrapper = layout.stack.Children.First();
			Assert.IsType<MockNativeViewWrapper>(wrapper);
			Assert.Equal(nativeView, ((MockNativeViewWrapper)wrapper).NativeView);

			Assert.Equal("foo", nativeView.Foo);
			Assert.Equal(42, nativeView.Bar);
			Assert.Equal("Bound Value", nativeView.Baz);
		}
	}
}