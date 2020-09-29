using System;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Internals;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public abstract class MockNativeView
	{
		public string Foo { get; set; }
		public int Bar { get; set; }
		public string Baz { get; set; }
	}

	public class MockUIView : MockNativeView
	{
		public IList<MockUIView> SubViews { get; set; }
	}

	class MockUIViewWrapper : View
	{
		public MockUIView NativeView { get; }

		public MockUIViewWrapper(MockUIView nativeView)
		{
			NativeView = nativeView;
			nativeView.TransferbindablePropertiesToWrapper(this);
		}

		protected override void OnBindingContextChanged()
		{
			NativeView.SetBindingContext(BindingContext, nv => nv.SubViews);
			base.OnBindingContextChanged();
		}
	}

	public class MockAndroidView : MockNativeView
	{
		public IList<MockAndroidView> SubViews { get; set; }
	}

	class MockAndroidViewWrapper : View
	{
		public MockAndroidView NativeView { get; }

		public MockAndroidViewWrapper(MockAndroidView nativeView)
		{
			NativeView = nativeView;
			nativeView.TransferbindablePropertiesToWrapper(this);
		}

		protected override void OnBindingContextChanged()
		{
			NativeView.SetBindingContext(BindingContext, nv => nv.SubViews);
			base.OnBindingContextChanged();
		}
	}

	public static class MockNativeViewExtensions
	{
		public static View ToView(this MockUIView nativeView)
		{
			return new MockUIViewWrapper(nativeView);
		}

		public static void SetBinding(this MockUIView target, string targetProperty, BindingBase binding, string updateSourceEventName = null)
		{
			NativeBindingHelpers.SetBinding(target, targetProperty, binding, updateSourceEventName);
		}

		internal static void SetBinding(this MockUIView target, string targetProperty, BindingBase binding, INotifyPropertyChanged propertyChanged)
		{
			NativeBindingHelpers.SetBinding(target, targetProperty, binding, propertyChanged);
		}

		public static void SetBinding(this MockUIView target, BindableProperty targetProperty, BindingBase binding)
		{
			NativeBindingHelpers.SetBinding(target, targetProperty, binding);
		}

		public static void SetValue(this MockUIView target, BindableProperty targetProperty, object value)
		{
			NativeBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this MockUIView target, object bindingContext, Func<MockUIView, IEnumerable<MockUIView>> getChild = null)
		{
			NativeBindingHelpers.SetBindingContext(target, bindingContext, getChild);
		}

		internal static void TransferbindablePropertiesToWrapper(this MockUIView target, MockUIViewWrapper wrapper)
		{
			NativeBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}

		public static View ToView(this MockAndroidView nativeView)
		{
			return new MockAndroidViewWrapper(nativeView);
		}

		public static void SetBinding(this MockAndroidView target, string targetProperty, BindingBase binding, string updateSourceEventName = null)
		{
			NativeBindingHelpers.SetBinding(target, targetProperty, binding, updateSourceEventName);
		}

		internal static void SetBinding(this MockAndroidView target, string targetProperty, BindingBase binding, INotifyPropertyChanged propertyChanged)
		{
			NativeBindingHelpers.SetBinding(target, targetProperty, binding, propertyChanged);
		}

		public static void SetBinding(this MockAndroidView target, BindableProperty targetProperty, BindingBase binding)
		{
			NativeBindingHelpers.SetBinding(target, targetProperty, binding);
		}

		public static void SetValue(this MockAndroidView target, BindableProperty targetProperty, object value)
		{
			NativeBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this MockAndroidView target, object bindingContext, Func<MockAndroidView, IEnumerable<MockAndroidView>> getChild = null)
		{
			NativeBindingHelpers.SetBindingContext(target, bindingContext, getChild);
		}

		internal static void TransferbindablePropertiesToWrapper(this MockAndroidView target, MockAndroidViewWrapper wrapper)
		{
			NativeBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}
	}

	public class MockIosNativeValueConverterService : INativeValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object nativeValue)
		{
			nativeValue = null;
			if (typeof(MockUIView).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View)))
			{
				nativeValue = ((MockUIView)value).ToView();
				return true;
			}
			return false;
		}
	}

	public class MockAndroidNativeValueConverterService : INativeValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object nativeValue)
		{
			nativeValue = null;
			if (typeof(MockAndroidView).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View)))
			{
				nativeValue = ((MockAndroidView)value).ToView();
				return true;
			}
			return false;
		}
	}

	public class MockIosNativeBindingService : INativeBindingService
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

	public class MockAndroidNativeBindingService : INativeBindingService
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

	public partial class NativeViewsAndBindings : ContentPage
	{
		public NativeViewsAndBindings()
		{
			InitializeComponent();
		}

		public NativeViewsAndBindings(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			void SetUpPlatform(string platform)
			{
				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = platform;
				if (platform == Device.iOS)
				{
					DependencyService.Register<INativeValueConverterService, MockIosNativeValueConverterService>();
					DependencyService.Register<INativeBindingService, MockIosNativeBindingService>();
				}
				else if (platform == Device.Android)
				{
					DependencyService.Register<INativeValueConverterService, MockAndroidNativeValueConverterService>();
					DependencyService.Register<INativeBindingService, MockAndroidNativeBindingService>();
				}
			}

			[TestCase(false, Device.iOS)]
			[TestCase(false, Device.Android)]
			//[TestCase(true)]
			public void NativeInContentView(bool useCompiledXaml, string platform)
			{
				SetUpPlatform(platform);
				var layout = new NativeViewsAndBindings(useCompiledXaml);
				layout.BindingContext = new
				{
					Baz = "Bound Value",
					VerticalOption = LayoutOptions.EndAndExpand
				};
				var view = layout.view0;
				Assert.NotNull(view.Content);
				MockNativeView nativeView = null;
				if (platform == Device.iOS)
				{
					Assert.That(view.Content, Is.TypeOf<MockUIViewWrapper>());
					Assert.That(((MockUIViewWrapper)view.Content).NativeView, Is.TypeOf<MockUIView>());
					nativeView = ((MockUIViewWrapper)view.Content).NativeView;
				}
				else if (platform == Device.Android)
				{
					Assert.That(view.Content, Is.TypeOf<MockAndroidViewWrapper>());
					Assert.That(((MockAndroidViewWrapper)view.Content).NativeView, Is.TypeOf<MockAndroidView>());
					nativeView = ((MockAndroidViewWrapper)view.Content).NativeView;
				}

				Assert.AreEqual("foo", nativeView.Foo);
				Assert.AreEqual(42, nativeView.Bar);
				Assert.AreEqual("Bound Value", nativeView.Baz);
				Assert.AreEqual(LayoutOptions.End, view.Content.GetValue(View.HorizontalOptionsProperty));
				Assert.AreEqual(LayoutOptions.EndAndExpand, view.Content.GetValue(View.VerticalOptionsProperty));
			}
		}
	}
}