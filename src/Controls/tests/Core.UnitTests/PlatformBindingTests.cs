using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;
using Xunit.Sdk;

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

		MockPlatformColor _selectedColor;
		public MockPlatformColor SelectedColor
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

		public MockPlatformViewWrapper(MockPlatformView platformView)
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

	public class MockPlatformColor
	{

		public MockPlatformColor(Color color)
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
		public static View ToView(this MockPlatformView platformView)
		{
			return new MockPlatformViewWrapper(platformView);
		}

		public static void SetBinding(this MockPlatformView target, string targetProperty, BindingBase binding, string updateSourceEventName = null)
		{
			PlatformBindingHelpers.SetBinding(target, targetProperty, binding, updateSourceEventName);
		}

		internal static void SetBinding(this MockPlatformView target, string targetProperty, BindingBase binding, INotifyPropertyChanged propertyChanged)
		{
			PlatformBindingHelpers.SetBinding(target, targetProperty, binding, propertyChanged);
		}

		public static void SetBinding(this MockPlatformView target, BindableProperty targetProperty, BindingBase binding)
		{
			PlatformBindingHelpers.SetBinding(target, targetProperty, binding);
		}

		public static void SetValue(this MockPlatformView target, BindableProperty targetProperty, object value)
		{
			PlatformBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this MockPlatformView target, object bindingContext, Func<MockPlatformView, IEnumerable<MockPlatformView>> getChild = null)
		{
			PlatformBindingHelpers.SetBindingContext(target, bindingContext, getChild);
		}

		internal static void TransferbindablePropertiesToWrapper(this MockPlatformView target, MockPlatformViewWrapper wrapper)
		{
			PlatformBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}
	}

	class MockCustomColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Color)
				return new MockPlatformColor((Color)value);
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is MockPlatformColor)
				return ((MockPlatformColor)value).FormsColor;
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

	class MockVMForPlatformBinding : INotifyPropertyChanged
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


	public class PlatformBindingTests : IDisposable
	{

		public PlatformBindingTests()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			//this should collect the ConditionalWeakTable
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Fact]
		public void SetOneWayBinding()
		{
			var platformView = new MockPlatformView();
			Assert.Null(platformView.Foo);
			Assert.Equal(0, platformView.Bar);

			platformView.SetBinding("Foo", new Binding("FFoo", mode: BindingMode.OneWay));
			platformView.SetBinding("Bar", new Binding("BBar", mode: BindingMode.OneWay));
			Assert.Null(platformView.Foo);
			Assert.Equal(0, platformView.Bar);

			platformView.SetBindingContext(new { FFoo = "Foo", BBar = 42 });
			Assert.Equal("Foo", platformView.Foo);
			Assert.Equal(42, platformView.Bar);
		}

		[Fact]
		public void AttachedPropertiesAreTransferredFromTheBackpack()
		{
			var platformView = new MockPlatformView();
			platformView.SetValue(Grid.ColumnProperty, 3);
			platformView.SetBinding(Grid.RowProperty, new Binding("foo"));

			var view = platformView.ToView();
			view.BindingContext = new { foo = 42 };
			Assert.Equal(3, view.GetValue(Grid.ColumnProperty));
			Assert.Equal(42, view.GetValue(Grid.RowProperty));
		}

		[Fact(Skip = "PlatformBindings aren't used")]
		public void Set2WayBindings()
		{
			var platformView = new MockPlatformView();
			Assert.Null(platformView.Foo);
			Assert.Equal(0, platformView.Bar);

			var vm = new MockVMForPlatformBinding();
			platformView.SetBindingContext(vm);
			var inpc = new MockINPC();
			platformView.SetBinding("Foo", new Binding("FFoo", mode: BindingMode.TwoWay), inpc);
			platformView.SetBinding("Bar", new Binding("BBar", mode: BindingMode.TwoWay), inpc);
			Assert.Null(platformView.Foo);
			Assert.Equal(0, platformView.Bar);
			Assert.Null(vm.FFoo);
			Assert.Equal(0, vm.BBar);

			platformView.Foo = "oof";
			inpc.FireINPC(platformView, "Foo");
			platformView.Bar = -42;
			inpc.FireINPC(platformView, "Bar");
			Assert.Equal("oof", platformView.Foo);
			Assert.Equal(-42, platformView.Bar);
			Assert.Equal("oof", vm.FFoo);
			Assert.Equal(-42, vm.BBar);

			vm.FFoo = "foo";
			vm.BBar = 42;
			Assert.Equal("foo", platformView.Foo);
			Assert.Equal(42, platformView.Bar);
			Assert.Equal("foo", vm.FFoo);
			Assert.Equal(42, vm.BBar);
		}

		[Fact(Skip = "PlatformBindings aren't used")]
		public void Set2WayBindingsWithUpdateSourceEvent()
		{
			var platformView = new MockPlatformView();
			Assert.Null(platformView.Baz);

			var vm = new MockVMForPlatformBinding();
			platformView.SetBindingContext(vm);

			platformView.SetBinding("Baz", new Binding("FFoo", mode: BindingMode.TwoWay), "BazChanged");
			Assert.Null(platformView.Baz);
			Assert.Null(vm.FFoo);

			platformView.Baz = "oof";
			platformView.FireBazChanged();
			Assert.Equal("oof", platformView.Baz);
			Assert.Equal("oof", vm.FFoo);

			vm.FFoo = "foo";
			Assert.Equal("foo", platformView.Baz);
			Assert.Equal("foo", vm.FFoo);
		}

		[Fact(Skip = "PlatformBindings aren't used")]
		public void Set2WayBindingsWithUpdateSourceEventInBindingObject()
		{
			var platformView = new MockPlatformView();
			Assert.Null(platformView.Baz);

			var vm = new MockVMForPlatformBinding();
			platformView.SetBindingContext(vm);

			platformView.SetBinding("Baz", new Binding("FFoo", mode: BindingMode.TwoWay) { UpdateSourceEventName = "BazChanged" });
			Assert.Null(platformView.Baz);
			Assert.Null(vm.FFoo);

			platformView.Baz = "oof";
			platformView.FireBazChanged();
			Assert.Equal("oof", platformView.Baz);
			Assert.Equal("oof", vm.FFoo);

			vm.FFoo = "foo";
			Assert.Equal("foo", platformView.Baz);
			Assert.Equal("foo", vm.FFoo);
		}

		[Fact]
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

				var platformView = new MockPlatformView();
				platformView.SetBinding("fooBar", new Binding("Foo", BindingMode.TwoWay));
				platformView.SetBinding("Baz", new Binding("Qux", BindingMode.TwoWay), "BazChanged");

				wr = new WeakReference(platformView);
				platformView = null;

			};

			create();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert.False(wr.IsAlive);
		}

		[Fact]
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

				var platformView = new MockPlatformView();
				platformView.SetBinding("fooBar", new Binding("Foo", BindingMode.TwoWay));
				platformView.SetBinding("Baz", new Binding("Qux", BindingMode.TwoWay), "BazChanged");

				PlatformBindingHelpers.BindableObjectProxy<MockPlatformView> proxy;
				if (!PlatformBindingHelpers.BindableObjectProxy<MockPlatformView>.BindableObjectProxies.TryGetValue(platformView, out proxy))
					throw new XunitException("Proxy should have been collected.");

				wr = new WeakReference(proxy);
				platformView = null;
			};

			create();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert.False(wr.IsAlive);
		}

		[Fact]
		public void SetBindingContextToSubviews()
		{
			var platformView = new MockPlatformView { SubViews = new List<MockPlatformView>() };
			var platformViewChild = new MockPlatformView();

			platformViewChild.SetBinding("Foo", new Binding("FFoo", mode: BindingMode.OneWay));
			platformViewChild.SetBinding("Bar", new Binding("BBar", mode: BindingMode.OneWay));

			platformView.SubViews.Add(platformViewChild);

			var vm = new MockVMForPlatformBinding();
			platformView.SetBindingContext(vm, v => v.SubViews);

			Assert.Null(platformViewChild.Foo);
			Assert.Equal(0, platformViewChild.Bar);

			platformView.SetBindingContext(new { FFoo = "Foo", BBar = 42 }, v => v.SubViews);
			Assert.Equal("Foo", platformViewChild.Foo);
			Assert.Equal(42, platformViewChild.Bar);
		}

		[Fact]
		public void TestConverterDoesNotThrow()
		{
			var platformView = new MockPlatformView();
			Assert.Null(platformView.Foo);
			Assert.Equal(0, platformView.Bar);
			var vm = new MockVMForPlatformBinding();
			var converter = new MockCustomColorConverter();
			platformView.SetBinding("SelectedColor", new Binding("CColor", converter: converter));
			platformView.SetBindingContext(vm);
		}

		[Fact(Skip = "PlatformBindings aren't used")]
		public void TestConverterWorks()
		{
			var platformView = new MockPlatformView();
			Assert.Null(platformView.Foo);
			Assert.Equal(0, platformView.Bar);
			var vm = new MockVMForPlatformBinding();
			vm.CColor = Colors.Red;
			var converter = new MockCustomColorConverter();
			platformView.SetBinding("SelectedColor", new Binding("CColor", converter: converter));
			platformView.SetBindingContext(vm);
			Assert.Equal(vm.CColor, platformView.SelectedColor.FormsColor);
		}

		[Fact(Skip = "PlatformBindings aren't used")]
		public void TestConverter2WayWorks()
		{
			var platformView = new MockPlatformView();
			Assert.Null(platformView.Foo);
			Assert.Equal(0, platformView.Bar);
			var inpc = new MockINPC();
			var vm = new MockVMForPlatformBinding();
			vm.CColor = Colors.Red;
			var converter = new MockCustomColorConverter();
			platformView.SetBinding("SelectedColor", new Binding("CColor", BindingMode.TwoWay, converter), inpc);
			platformView.SetBindingContext(vm);
			Assert.Equal(vm.CColor, platformView.SelectedColor.FormsColor);

			var newFormsColor = Colors.Blue;
			var newColor = new MockPlatformColor(newFormsColor);
			platformView.SelectedColor = newColor;
			inpc.FireINPC(platformView, nameof(platformView.SelectedColor));

			Assert.Equal(newFormsColor, vm.CColor);

		}

		[Fact(Skip = "PlatformBindings aren't used")]
		public void Binding2WayWithConvertersDoNotLoop()
		{
			var platformView = new MockPlatformView();
			int count = 0;

			platformView.SelectedColorChanged += (o, e) =>
			{
				if (++count > 5)
					throw new XunitException("Probable loop detected");
			};

			var vm = new MockVMForPlatformBinding { CColor = Colors.Red };

			platformView.SetBinding("SelectedColor", new Binding("CColor", BindingMode.TwoWay, new MockCustomColorConverter()), "SelectedColorChanged");
			platformView.SetBindingContext(vm);

			Assert.Equal(1, count);
		}

		[Fact(Skip = "PlatformBindings aren't used")]
		public void ThrowsOnMissingProperty()
		{
			var platformView = new MockPlatformView();
			platformView.SetBinding("Qux", new Binding("Foo"));
			Assert.Throws<InvalidOperationException>(() => platformView.SetBindingContext(new { Foo = 42 }));
		}

		[Fact]
		public void ThrowsOnMissingEvent()
		{
			var platformView = new MockPlatformView();
			Assert.Throws<ArgumentException>(() => platformView.SetBinding("Foo", new Binding("Foo", BindingMode.TwoWay), "missingEvent"));
		}

		[Fact]
		public void OneWayToSourceAppliedOnSetBC()
		{
			var platformView = new MockPlatformView { Foo = "foobar" };
			platformView.SetBinding("Foo", new Binding("FFoo", BindingMode.OneWayToSource));
			var vm = new MockVMForPlatformBinding { FFoo = "qux" };
			platformView.SetBindingContext(vm);
			Assert.Equal("foobar", vm.FFoo);
		}

		[Fact(Skip = "PlatformBindings aren't used")]
		public void DoNotApplyNull()
		{
			var native = new MockPlatformView();
			Assert.NotNull(native.CantBeNull);
			native.SetBinding("CantBeNull", new Binding("FFoo", BindingMode.TwoWay));
			Assert.NotNull(native.CantBeNull);
			native.SetBindingContext(new { FFoo = "foo" });
			Assert.Equal("foo", native.CantBeNull);
		}
	}
}