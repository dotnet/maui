using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Internals;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public abstract class MockPlatformView
	{
		public string Foo { get; set; }
		public int Bar { get; set; }
		public string Baz { get; set; }
	}

	public class MockUIView : MockPlatformView
	{
		public IList<MockUIView> SubViews { get; set; }
	}

	class MockUIViewWrapper : View
	{
		public MockUIView PlatformView { get; }

		public MockUIViewWrapper(MockUIView platformView)
		{
			PlatformView = platformView;
			platformView.TransferbindablePropertiesToWrapper(this);
		}

		protected override void OnBindingContextChanged()
		{
			PlatformView.SetBindingContext(BindingContext, nv => nv.SubViews);
			base.OnBindingContextChanged();
		}
	}

	public class MockAndroidView : MockPlatformView
	{
		public IList<MockAndroidView> SubViews { get; set; }
	}

	class MockAndroidViewWrapper : View
	{
		public MockAndroidView PlatformView { get; }

		public MockAndroidViewWrapper(MockAndroidView platformView)
		{
			PlatformView = platformView;
			platformView.TransferbindablePropertiesToWrapper(this);
		}

		protected override void OnBindingContextChanged()
		{
			PlatformView.SetBindingContext(BindingContext, nv => nv.SubViews);
			base.OnBindingContextChanged();
		}
	}

	public static class MockPlatformViewExtensions
	{
		public static View ToView(this MockUIView platformView)
		{
			return new MockUIViewWrapper(platformView);
		}

		public static void SetBinding(this MockUIView target, string targetProperty, BindingBase binding, string updateSourceEventName = null)
		{
			PlatformBindingHelpers.SetBinding(target, targetProperty, binding, updateSourceEventName);
		}

		internal static void SetBinding(this MockUIView target, string targetProperty, BindingBase binding, INotifyPropertyChanged propertyChanged)
		{
			PlatformBindingHelpers.SetBinding(target, targetProperty, binding, propertyChanged);
		}

		public static void SetBinding(this MockUIView target, BindableProperty targetProperty, BindingBase binding)
		{
			PlatformBindingHelpers.SetBinding(target, targetProperty, binding);
		}

		public static void SetValue(this MockUIView target, BindableProperty targetProperty, object value)
		{
			PlatformBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this MockUIView target, object bindingContext, Func<MockUIView, IEnumerable<MockUIView>> getChild = null)
		{
			PlatformBindingHelpers.SetBindingContext(target, bindingContext, getChild);
		}

		internal static void TransferbindablePropertiesToWrapper(this MockUIView target, MockUIViewWrapper wrapper)
		{
			PlatformBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}

		public static View ToView(this MockAndroidView platformView)
		{
			return new MockAndroidViewWrapper(platformView);
		}

		public static void SetBinding(this MockAndroidView target, string targetProperty, BindingBase binding, string updateSourceEventName = null)
		{
			PlatformBindingHelpers.SetBinding(target, targetProperty, binding, updateSourceEventName);
		}

		internal static void SetBinding(this MockAndroidView target, string targetProperty, BindingBase binding, INotifyPropertyChanged propertyChanged)
		{
			PlatformBindingHelpers.SetBinding(target, targetProperty, binding, propertyChanged);
		}

		public static void SetBinding(this MockAndroidView target, BindableProperty targetProperty, BindingBase binding)
		{
			PlatformBindingHelpers.SetBinding(target, targetProperty, binding);
		}

		public static void SetValue(this MockAndroidView target, BindableProperty targetProperty, object value)
		{
			PlatformBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this MockAndroidView target, object bindingContext, Func<MockAndroidView, IEnumerable<MockAndroidView>> getChild = null)
		{
			PlatformBindingHelpers.SetBindingContext(target, bindingContext, getChild);
		}

		internal static void TransferbindablePropertiesToWrapper(this MockAndroidView target, MockAndroidViewWrapper wrapper)
		{
			PlatformBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}
	}

	public class MockIosPlatformValueConverterService : INativeValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object platformValue)
		{
			platformValue = null;
			if (typeof(MockUIView).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View)))
			{
				platformValue = ((MockUIView)value).ToView();
				return true;
			}
			return false;
		}
	}

	public class MockAndroidPlatformValueConverterService : INativeValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object platformValue)
		{
			platformValue = null;
			if (typeof(MockAndroidView).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View)))
			{
				platformValue = ((MockAndroidView)value).ToView();
				return true;
			}
			return false;
		}
	}

	public class MockIosPlatformBindingService : INativeBindingService
	{
		public bool TrySetBinding(object target, string propertyName, BindingBase binding)
		{
			var view = target as MockUIView;
			if (view == null)
				return false;
			if (target.GetType().GetProperty(propertyName)?.GetMethod == null)
				return false;
			view.SetBinding(propertyName, binding);
			return true;
		}

		public bool TrySetBinding(object target, BindableProperty property, BindingBase binding)
		{
			var view = target as MockUIView;
			if (view == null)
				return false;
			view.SetBinding(property, binding);
			return true;
		}

		public bool TrySetValue(object target, BindableProperty property, object value)
		{
			var view = target as MockUIView;
			if (view == null)
				return false;
			view.SetValue(property, value);
			return true;
		}
	}

	public class MockAndroidPlatformBindingService : INativeBindingService
	{
		public bool TrySetBinding(object target, string propertyName, BindingBase binding)
		{
			var view = target as MockAndroidView;
			if (view == null)
				return false;
			view.SetBinding(propertyName, binding);
			return true;
		}

		public bool TrySetBinding(object target, BindableProperty property, BindingBase binding)
		{
			var view = target as MockAndroidView;
			if (view == null)
				return false;
			view.SetBinding(property, binding);
			return true;
		}

		public bool TrySetValue(object target, BindableProperty property, object value)
		{
			var view = target as MockAndroidView;
			if (view == null)
				return false;
			view.SetValue(property, value);
			return true;
		}
	}

	public partial class PlatformViewsAndBindings : ContentPage
	{
		public PlatformViewsAndBindings()
		{
			InitializeComponent();
		}

		public PlatformViewsAndBindings(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public class Tests : IDisposable
		{
			private MockDeviceInfo mockDeviceInfo;

			public Tests()
			{
				DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			}

			public void Dispose()
			{
				DeviceInfo.SetCurrent(null);
			}

			private DevicePlatform SetUpPlatform(string platform)
			{
				var p = DevicePlatform.Create(platform);
				mockDeviceInfo.Platform = p;
				if (p == DevicePlatform.iOS)
				{
					DependencyService.Register<INativeValueConverterService, MockIosPlatformValueConverterService>();
					DependencyService.Register<INativeBindingService, MockIosPlatformBindingService>();
				}
				else if (p == DevicePlatform.Android)
				{
					DependencyService.Register<INativeValueConverterService, MockAndroidPlatformValueConverterService>();
					DependencyService.Register<INativeBindingService, MockAndroidPlatformBindingService>();
				}
				return p;
			}

			[Theory(Skip = "fails for now")]
			[InlineData(false, "iOS")]
			[InlineData(false, "Android")]
			public void PlatformInContentView(bool useCompiledXaml, string platform)
			{
				var realPlatform = SetUpPlatform(platform);
				var layout = new PlatformViewsAndBindings(useCompiledXaml);
				layout.BindingContext = new
				{
					Baz = "Bound Value",
					VerticalOption = LayoutOptions.EndAndExpand
				};
				var view = layout.view0;
				Assert.NotNull(view.Content);

				MockPlatformView platformView = null;
				if (realPlatform == DevicePlatform.iOS)
				{
					Assert.IsType<MockUIViewWrapper>(view.Content);
					Assert.IsType<MockUIView>(((MockUIViewWrapper)view.Content).PlatformView);
					platformView = ((MockUIViewWrapper)view.Content).PlatformView;
				}
				else if (realPlatform == DevicePlatform.Android)
				{
					Assert.IsType<MockAndroidViewWrapper>(view.Content);
					Assert.IsType<MockAndroidView>(((MockAndroidViewWrapper)view.Content).PlatformView);
					platformView = ((MockAndroidViewWrapper)view.Content).PlatformView;
				}

				Assert.Equal("foo", platformView.Foo);
				Assert.Equal(42, platformView.Bar);
				Assert.Equal("Bound Value", platformView.Baz);
				Assert.Equal(LayoutOptions.End, view.Content.GetValue(View.HorizontalOptionsProperty));
				Assert.Equal(LayoutOptions.EndAndExpand, view.Content.GetValue(View.VerticalOptionsProperty));
			}
		}
	}
}