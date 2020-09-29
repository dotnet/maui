using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	public class MockNativeView
	{
		public IList<MockNativeView> SubViews { get; set; }
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

	class MockNativeViewWrapper : View
	{
		public MockNativeView NativeView { get; }

		public MockNativeViewWrapper(MockNativeView nativeView)
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

	public static class MockNativeViewExtensions
	{
		public static View ToView(this MockNativeView nativeView)
		{
			return new MockNativeViewWrapper(nativeView);
		}

		public static void SetBinding(this MockNativeView target, string targetProperty, BindingBase binding, string updateSourceEventName = null)
		{
			NativeBindingHelpers.SetBinding(target, targetProperty, binding, updateSourceEventName);
		}

		internal static void SetBinding(this MockNativeView target, string targetProperty, BindingBase binding, INotifyPropertyChanged propertyChanged)
		{
			NativeBindingHelpers.SetBinding(target, targetProperty, binding, propertyChanged);
		}

		public static void SetBinding(this MockNativeView target, BindableProperty targetProperty, BindingBase binding)
		{
			NativeBindingHelpers.SetBinding(target, targetProperty, binding);
		}

		public static void SetValue(this MockNativeView target, BindableProperty targetProperty, object value)
		{
			NativeBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this MockNativeView target, object bindingContext, Func<MockNativeView, IEnumerable<MockNativeView>> getChild = null)
		{
			NativeBindingHelpers.SetBindingContext(target, bindingContext, getChild);
		}

		internal static void TransferbindablePropertiesToWrapper(this MockNativeView target, MockNativeViewWrapper wrapper)
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
			Device.PlatformServices = new MockPlatformServices();

			//this should collect the ConditionalWeakTable
			GC.Collect();
		}

		[Test]
		public void SetOneWayBinding()
		{
			var nativeView = new MockNativeView();
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
			var nativeView = new MockNativeView();
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
			var nativeView = new MockNativeView();
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
			var nativeView = new MockNativeView();
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
			var nativeView = new MockNativeView();
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
		public void NativeViewsAreCollected()
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

				var nativeView = new MockNativeView();
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

				var nativeView = new MockNativeView();
				nativeView.SetBinding("fooBar", new Binding("Foo", BindingMode.TwoWay));
				nativeView.SetBinding("Baz", new Binding("Qux", BindingMode.TwoWay), "BazChanged");

				NativeBindingHelpers.BindableObjectProxy<MockNativeView> proxy;
				if (!NativeBindingHelpers.BindableObjectProxy<MockNativeView>.BindableObjectProxies.TryGetValue(nativeView, out proxy))
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
			var nativeView = new MockNativeView { SubViews = new List<MockNativeView>() };
			var nativeViewChild = new MockNativeView();

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
			var nativeView = new MockNativeView();
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
			var nativeView = new MockNativeView();
			Assert.AreEqual(null, nativeView.Foo);
			Assert.AreEqual(0, nativeView.Bar);
			var vm = new MockVMForNativeBinding();
			vm.CColor = Color.Red;
			var converter = new MockCustomColorConverter();
			nativeView.SetBinding("SelectedColor", new Binding("CColor", converter: converter));
			nativeView.SetBindingContext(vm);
			Assert.AreEqual(vm.CColor, nativeView.SelectedColor.FormsColor);
		}

		[Test]
		public void TestConverter2WayWorks()
		{
			var nativeView = new MockNativeView();
			Assert.AreEqual(null, nativeView.Foo);
			Assert.AreEqual(0, nativeView.Bar);
			var inpc = new MockINPC();
			var vm = new MockVMForNativeBinding();
			vm.CColor = Color.Red;
			var converter = new MockCustomColorConverter();
			nativeView.SetBinding("SelectedColor", new Binding("CColor", BindingMode.TwoWay, converter), inpc);
			nativeView.SetBindingContext(vm);
			Assert.AreEqual(vm.CColor, nativeView.SelectedColor.FormsColor);

			var newFormsColor = Color.Blue;
			var newColor = new MockNativeColor(newFormsColor);
			nativeView.SelectedColor = newColor;
			inpc.FireINPC(nativeView, nameof(nativeView.SelectedColor));

			Assert.AreEqual(newFormsColor, vm.CColor);

		}

		[Test]
		public void Binding2WayWithConvertersDoNotLoop()
		{
			var nativeView = new MockNativeView();
			int count = 0;

			nativeView.SelectedColorChanged += (o, e) =>
			{
				if (++count > 5)
					Assert.Fail("Probable loop detected");
			};

			var vm = new MockVMForNativeBinding { CColor = Color.Red };

			nativeView.SetBinding("SelectedColor", new Binding("CColor", BindingMode.TwoWay, new MockCustomColorConverter()), "SelectedColorChanged");
			nativeView.SetBindingContext(vm);

			Assert.AreEqual(count, 1);
		}

		[Test]
		public void ThrowsOnMissingProperty()
		{
			var nativeView = new MockNativeView();
			nativeView.SetBinding("Qux", new Binding("Foo"));
			Assert.Throws<InvalidOperationException>(() => nativeView.SetBindingContext(new { Foo = 42 }));
		}

		[Test]
		public void ThrowsOnMissingEvent()
		{
			var nativeView = new MockNativeView();
			Assert.Throws<ArgumentException>(() => nativeView.SetBinding("Foo", new Binding("Foo", BindingMode.TwoWay), "missingEvent"));
		}

		[Test]
		public void OneWayToSourceAppliedOnSetBC()
		{
			var nativeView = new MockNativeView { Foo = "foobar" };
			nativeView.SetBinding("Foo", new Binding("FFoo", BindingMode.OneWayToSource));
			var vm = new MockVMForNativeBinding { FFoo = "qux" };
			nativeView.SetBindingContext(vm);
			Assert.AreEqual("foobar", vm.FFoo);
		}

		[Test]
		public void DoNotApplyNull()
		{
			var native = new MockNativeView();
			Assert.NotNull(native.CantBeNull);
			native.SetBinding("CantBeNull", new Binding("FFoo", BindingMode.TwoWay));
			Assert.NotNull(native.CantBeNull);
			native.SetBindingContext(new { FFoo = "foo" });
			Assert.AreEqual("foo", native.CantBeNull);
		}
	}
}