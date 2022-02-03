using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class MockPlatformView
	{
		public IList<MockPlatformView> SubViews { get; set; }
		public string Foo { get; set; }
		public int Bar { get; set; }
		public string Baz { get; set; }

		string cantBeNull = "";
		public string CantBeNull
		{
			get { return cantBeNull; }
			set
			{
				if (value == null)
					throw new NullReferenceException();
				cantBeNull = value;
			}
		}

		public void FireBazChanged()
		{
			BazChanged?.Invoke(this, new TappedEventArgs(null));
		}

		public event EventHandler<TappedEventArgs> BazChanged;

		public event EventHandler SelectedColorChanged;

		MockNativeColor _selectedColor;
		public MockNativeColor SelectedColor
		{
			get { return _selectedColor; }
			set
			{
				if (_selectedColor == value)
					return;
				_selectedColor = value;
				SelectedColorChanged?.Invoke(this, EventArgs.Empty);
			}
		}

	}

	class MockPlatformViewWrapper : View
	{
		public MockPlatformView PlatformView { get; }

		public MockPlatformViewWrapper(MockPlatformView nativeView)
		{
			PlatformView = nativeView;
			nativeView.TransferbindablePropertiesToWrapper(this);
		}

		protected override void OnBindingContextChanged()
		{
			PlatformView.SetBindingContext(BindingContext, nv => nv.SubViews);
			base.OnBindingContextChanged();
		}
	}

	public class MockNativeColor
	{

		public MockNativeColor(Color color)
		{
			FormsColor = color;
		}

		public Color FormsColor
		{
			get;
			set;
		}
	}

	public static class MockPlatformViewExtensions
	{
		public static View ToView(this MockPlatformView nativeView)
		{
			return new MockPlatformViewWrapper(nativeView);
		}

		public static void SetBinding(this MockPlatformView target, string targetProperty, BindingBase binding, string updateSourceEventName = null)
		{
			NativeBindingHelpers.SetBinding(target, targetProperty, binding, updateSourceEventName);
		}

		internal static void SetBinding(this MockPlatformView target, string targetProperty, BindingBase binding, INotifyPropertyChanged propertyChanged)
		{
			NativeBindingHelpers.SetBinding(target, targetProperty, binding, propertyChanged);
		}

		public static void SetBinding(this MockPlatformView target, BindableProperty targetProperty, BindingBase binding)
		{
			NativeBindingHelpers.SetBinding(target, targetProperty, binding);
		}

		public static void SetValue(this MockPlatformView target, BindableProperty targetProperty, object value)
		{
			NativeBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this MockPlatformView target, object bindingContext, Func<MockPlatformView, IEnumerable<MockPlatformView>> getChild = null)
		{
			NativeBindingHelpers.SetBindingContext(target, bindingContext, getChild);
		}

		internal static void TransferbindablePropertiesToWrapper(this MockPlatformView target, MockPlatformViewWrapper wrapper)
		{
			NativeBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}
	}

	class MockCustomColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Color)
				return new MockNativeColor((Color)value);
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is MockNativeColor)
				return ((MockNativeColor)value).FormsColor;
			return value;
		}
	}

	class MockINPC : INotifyPropertyChanged
	{
		public void FireINPC(object sender, string propertyName)
		{
			PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}

	class MockVMForNativeBinding : INotifyPropertyChanged
	{
		string fFoo;
		public string FFoo
		{
			get { return fFoo; }
			set
			{
				fFoo = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FFoo"));
			}
		}

		int bBar;
		public int BBar
		{
			get { return bBar; }
			set
			{
				bBar = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BBar"));
			}
		}

		Color cColor;
		public Color CColor
		{
			get { return cColor; }
			set
			{
				if (cColor == value)
					return;
				cColor = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CColor"));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}

	[TestFixture]
	public class NativeBindingTests
	{
		[SetUp]
		public void SetUp()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			//this should collect the ConditionalWeakTable
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void SetOneWayBinding()
		{
			var nativeView = new MockPlatformView();
			Assert.AreEqual(null, nativeView.Foo);
			Assert.AreEqual(0, nativeView.Bar);

			nativeView.SetBinding("Foo", new Binding("FFoo", mode: BindingMode.OneWay));
			nativeView.SetBinding("Bar", new Binding("BBar", mode: BindingMode.OneWay));
			Assert.AreEqual(null, nativeView.Foo);
			Assert.AreEqual(0, nativeView.Bar);

			nativeView.SetBindingContext(new { FFoo = "Foo", BBar = 42 });
			Assert.AreEqual("Foo", nativeView.Foo);
			Assert.AreEqual(42, nativeView.Bar);
		}

		[Test]
		public void AttachedPropertiesAreTransferredFromTheBackpack()
		{
			var nativeView = new MockPlatformView();
			nativeView.SetValue(Grid.ColumnProperty, 3);
			nativeView.SetBinding(Grid.RowProperty, new Binding("foo"));

			var view = nativeView.ToView();
			view.BindingContext = new { foo = 42 };
			Assert.AreEqual(3, view.GetValue(Grid.ColumnProperty));
			Assert.AreEqual(42, view.GetValue(Grid.RowProperty));
		}

		[Test]
		public void Set2WayBindings()
		{
			var nativeView = new MockPlatformView();
			Assert.AreEqual(null, nativeView.Foo);
			Assert.AreEqual(0, nativeView.Bar);

			var vm = new MockVMForNativeBinding();
			nativeView.SetBindingContext(vm);
			var inpc = new MockINPC();
			nativeView.SetBinding("Foo", new Binding("FFoo", mode: BindingMode.TwoWay), inpc);
			nativeView.SetBinding("Bar", new Binding("BBar", mode: BindingMode.TwoWay), inpc);
			Assert.AreEqual(null, nativeView.Foo);
			Assert.AreEqual(0, nativeView.Bar);
			Assert.AreEqual(null, vm.FFoo);
			Assert.AreEqual(0, vm.BBar);

			nativeView.Foo = "oof";
			inpc.FireINPC(nativeView, "Foo");
			nativeView.Bar = -42;
			inpc.FireINPC(nativeView, "Bar");
			Assert.AreEqual("oof", nativeView.Foo);
			Assert.AreEqual(-42, nativeView.Bar);
			Assert.AreEqual("oof", vm.FFoo);
			Assert.AreEqual(-42, vm.BBar);

			vm.FFoo = "foo";
			vm.BBar = 42;
			Assert.AreEqual("foo", nativeView.Foo);
			Assert.AreEqual(42, nativeView.Bar);
			Assert.AreEqual("foo", vm.FFoo);
			Assert.AreEqual(42, vm.BBar);
		}

		[Test]
		public void Set2WayBindingsWithUpdateSourceEvent()
		{
			var nativeView = new MockPlatformView();
			Assert.AreEqual(null, nativeView.Baz);

			var vm = new MockVMForNativeBinding();
			nativeView.SetBindingContext(vm);

			nativeView.SetBinding("Baz", new Binding("FFoo", mode: BindingMode.TwoWay), "BazChanged");
			Assert.AreEqual(null, nativeView.Baz);
			Assert.AreEqual(null, vm.FFoo);

			nativeView.Baz = "oof";
			nativeView.FireBazChanged();
			Assert.AreEqual("oof", nativeView.Baz);
			Assert.AreEqual("oof", vm.FFoo);

			vm.FFoo = "foo";
			Assert.AreEqual("foo", nativeView.Baz);
			Assert.AreEqual("foo", vm.FFoo);
		}

		[Test]
		public void Set2WayBindingsWithUpdateSourceEventInBindingObject()
		{
			var nativeView = new MockPlatformView();
			Assert.AreEqual(null, nativeView.Baz);

			var vm = new MockVMForNativeBinding();
			nativeView.SetBindingContext(vm);

			nativeView.SetBinding("Baz", new Binding("FFoo", mode: BindingMode.TwoWay) { UpdateSourceEventName = "BazChanged" });
			Assert.AreEqual(null, nativeView.Baz);
			Assert.AreEqual(null, vm.FFoo);

			nativeView.Baz = "oof";
			nativeView.FireBazChanged();
			Assert.AreEqual("oof", nativeView.Baz);
			Assert.AreEqual("oof", vm.FFoo);

			vm.FFoo = "foo";
			Assert.AreEqual("foo", nativeView.Baz);
			Assert.AreEqual("foo", vm.FFoo);
		}

		[Test]
		public void PlatformViewsAreCollected()
		{
			WeakReference wr = null;

			int i = 0;
			Action create = null;
			create = () =>
			{
				if (i++ < 1024)
				{
					create();
					return;
				}

				var nativeView = new MockPlatformView();
				nativeView.SetBinding("fooBar", new Binding("Foo", BindingMode.TwoWay));
				nativeView.SetBinding("Baz", new Binding("Qux", BindingMode.TwoWay), "BazChanged");

				wr = new WeakReference(nativeView);
				nativeView = null;

			};

			create();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert.False(wr.IsAlive);
		}

		[Test]
		public void ProxiesAreCollected()
		{
			WeakReference wr = null;

			int i = 0;
			Action create = null;
			create = () =>
			{
				if (i++ < 1024)
				{
					create();
					return;
				}

				var nativeView = new MockPlatformView();
				nativeView.SetBinding("fooBar", new Binding("Foo", BindingMode.TwoWay));
				nativeView.SetBinding("Baz", new Binding("Qux", BindingMode.TwoWay), "BazChanged");

				NativeBindingHelpers.BindableObjectProxy<MockPlatformView> proxy;
				if (!NativeBindingHelpers.BindableObjectProxy<MockPlatformView>.BindableObjectProxies.TryGetValue(nativeView, out proxy))
					Assert.Fail();

				wr = new WeakReference(proxy);
				nativeView = null;
			};

			create();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert.False(wr.IsAlive);
		}

		[Test]
		public void SetBindingContextToSubviews()
		{
			var nativeView = new MockPlatformView { SubViews = new List<MockPlatformView>() };
			var nativeViewChild = new MockPlatformView();

			nativeViewChild.SetBinding("Foo", new Binding("FFoo", mode: BindingMode.OneWay));
			nativeViewChild.SetBinding("Bar", new Binding("BBar", mode: BindingMode.OneWay));

			nativeView.SubViews.Add(nativeViewChild);

			var vm = new MockVMForNativeBinding();
			nativeView.SetBindingContext(vm, v => v.SubViews);

			Assert.AreEqual(null, nativeViewChild.Foo);
			Assert.AreEqual(0, nativeViewChild.Bar);

			nativeView.SetBindingContext(new { FFoo = "Foo", BBar = 42 }, v => v.SubViews);
			Assert.AreEqual("Foo", nativeViewChild.Foo);
			Assert.AreEqual(42, nativeViewChild.Bar);
		}

		[Test]
		public void TestConverterDoesNotThrow()
		{
			var nativeView = new MockPlatformView();
			Assert.AreEqual(null, nativeView.Foo);
			Assert.AreEqual(0, nativeView.Bar);
			var vm = new MockVMForNativeBinding();
			var converter = new MockCustomColorConverter();
			nativeView.SetBinding("SelectedColor", new Binding("CColor", converter: converter));
			Assert.DoesNotThrow(() => nativeView.SetBindingContext(vm));
		}

		[Test]
		public void TestConverterWorks()
		{
			var nativeView = new MockPlatformView();
			Assert.AreEqual(null, nativeView.Foo);
			Assert.AreEqual(0, nativeView.Bar);
			var vm = new MockVMForNativeBinding();
			vm.CColor = Colors.Red;
			var converter = new MockCustomColorConverter();
			nativeView.SetBinding("SelectedColor", new Binding("CColor", converter: converter));
			nativeView.SetBindingContext(vm);
			Assert.AreEqual(vm.CColor, nativeView.SelectedColor.FormsColor);
		}

		[Test]
		public void TestConverter2WayWorks()
		{
			var nativeView = new MockPlatformView();
			Assert.AreEqual(null, nativeView.Foo);
			Assert.AreEqual(0, nativeView.Bar);
			var inpc = new MockINPC();
			var vm = new MockVMForNativeBinding();
			vm.CColor = Colors.Red;
			var converter = new MockCustomColorConverter();
			nativeView.SetBinding("SelectedColor", new Binding("CColor", BindingMode.TwoWay, converter), inpc);
			nativeView.SetBindingContext(vm);
			Assert.AreEqual(vm.CColor, nativeView.SelectedColor.FormsColor);

			var newFormsColor = Colors.Blue;
			var newColor = new MockNativeColor(newFormsColor);
			nativeView.SelectedColor = newColor;
			inpc.FireINPC(nativeView, nameof(nativeView.SelectedColor));

			Assert.AreEqual(newFormsColor, vm.CColor);

		}

		[Test]
		public void Binding2WayWithConvertersDoNotLoop()
		{
			var nativeView = new MockPlatformView();
			int count = 0;

			nativeView.SelectedColorChanged += (o, e) =>
			{
				if (++count > 5)
					Assert.Fail("Probable loop detected");
			};

			var vm = new MockVMForNativeBinding { CColor = Colors.Red };

			nativeView.SetBinding("SelectedColor", new Binding("CColor", BindingMode.TwoWay, new MockCustomColorConverter()), "SelectedColorChanged");
			nativeView.SetBindingContext(vm);

			Assert.AreEqual(count, 1);
		}

		[Test]
		public void ThrowsOnMissingProperty()
		{
			var nativeView = new MockPlatformView();
			nativeView.SetBinding("Qux", new Binding("Foo"));
			Assert.Throws<InvalidOperationException>(() => nativeView.SetBindingContext(new { Foo = 42 }));
		}

		[Test]
		public void ThrowsOnMissingEvent()
		{
			var nativeView = new MockPlatformView();
			Assert.Throws<ArgumentException>(() => nativeView.SetBinding("Foo", new Binding("Foo", BindingMode.TwoWay), "missingEvent"));
		}

		[Test]
		public void OneWayToSourceAppliedOnSetBC()
		{
			var nativeView = new MockPlatformView { Foo = "foobar" };
			nativeView.SetBinding("Foo", new Binding("FFoo", BindingMode.OneWayToSource));
			var vm = new MockVMForNativeBinding { FFoo = "qux" };
			nativeView.SetBindingContext(vm);
			Assert.AreEqual("foobar", vm.FFoo);
		}

		[Test]
		public void DoNotApplyNull()
		{
			var native = new MockPlatformView();
			Assert.NotNull(native.CantBeNull);
			native.SetBinding("CantBeNull", new Binding("FFoo", BindingMode.TwoWay));
			Assert.NotNull(native.CantBeNull);
			native.SetBindingContext(new { FFoo = "foo" });
			Assert.AreEqual("foo", native.CantBeNull);
		}
	}
}