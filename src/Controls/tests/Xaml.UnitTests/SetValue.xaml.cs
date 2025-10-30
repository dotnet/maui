using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;
using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

using AbsoluteLayout = Microsoft.Maui.Controls.Compatibility.AbsoluteLayout;

[TypeConverter(typeof(ConvertibleToViewTypeConverter))]
public class ConvertibleToView
{
	public static implicit operator View(ConvertibleToView source)
	{
		return new Button();
	}

	internal sealed class ConvertibleToViewTypeConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(View);
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type destinationType)
			=> value switch
			{
				ConvertibleToView convertibleValue when destinationType == typeof(View) => (View)convertibleValue,
				_ => throw new NotSupportedException(),
			};

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => false;
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) => throw new NotSupportedException();
	}
}

public class MockViewWithValues : View
{
	public char AChar { get; set; }
	public byte AByte { get; set; }
	public sbyte ASByte { get; set; }
	public Int16 AShort { get; set; }
	public UInt16 UShort { get; set; }
	public decimal ADecimal { get; set; }
	public SV_Foo Foo { get; set; }
	public string Bar { get; set; }

	public static readonly BindableProperty BPFooProperty =
		BindableProperty.Create("BPFoo", typeof(SV_Foo), typeof(MockViewWithValues), default(SV_Foo));

	public SV_Foo BPFoo
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}

	public static readonly BindableProperty BPBarProperty =
		BindableProperty.Create("BPBar", typeof(string), typeof(MockViewWithValues), default(string));

	public string BPBar
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}
}

[TypeConverter(typeof(SV_FooTypeConveter))]
public class SV_Foo
{
	public string Value { get; set; }
	public static implicit operator SV_Foo(string value)
	{
		return new SV_Foo { Value = value };
	}

	public static implicit operator string(SV_Foo foo)
	{
		return foo.Value;
	}

	internal sealed class SV_FooTypeConveter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type destinationType)
		{
			if (value is SV_Foo foo)
			{
				if (destinationType == typeof(string))
				{
					return (string)foo;
				}
			}

			throw new NotSupportedException();
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
			=> value switch
			{
				string str => (SV_Foo)str,
				_ => throw new NotSupportedException(),
			};
	}
}

public enum IntEnum
{
	Foo,
	Bar,
	Baz
}

public enum ByteEnum : byte
{
	Foo,
	Bar,
	Baz
}

public class ViewWithEnums : View
{
	public IntEnum IntEnum { get; set; }
	public ByteEnum ByteEnum { get; set; }
}

public class SetValue_ReverseConverter : IValueConverter
{
	public static ReverseConverter Instance = new ReverseConverter();

	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		var s = value as string;
		if (s == null)
			return value;
		return new string(s.Reverse().ToArray());
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		var s = value as string;
		if (s == null)
			return value;
		return new string(s.Reverse().ToArray());
	}
}

public partial class SetValue : ContentPage
{
	public SetValue()
	{
		InitializeComponent();
	}

	bool fired;
	void onButtonClicked(object sender, EventArgs args)
	{
		fired = true;
	}

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void SetValueToBP([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual("Foo", page.label0.Text);
		}

		[Test]
		public void SetBindingToBP([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual(Label.TextProperty.DefaultValue, page.label1.Text);

			page.label1.BindingContext = new { labeltext = "Foo" };
			Assert.AreEqual("Foo", page.label1.Text);
		}

		[Test]
		public void SetBindingWithImplicitPath([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual(Label.TextProperty.DefaultValue, page.label11.Text);

			page.label11.BindingContext = new { labeltext = "Foo" };
			Assert.AreEqual("Foo", page.label11.Text);
		}

		[Test]
		public void SetBindingWithConverter([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			page.label15.BindingContext = new { labeltext = "Foo" };
			Assert.AreEqual("ooF", page.label15.Text);
		}

		[Test]
		public void SetEvent([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.False(page.fired);
			(page.button0 as IButtonController).SendClicked();
			Assert.True(page.fired);
		}

		[Test]
		public void SetBoolValue([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.True(page.image0.IsOpaque);
		}

		//TODO test all value conversions

		[Test]
		public void SetAttachedBP([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual(1, Grid.GetColumn(page.label2));
			Assert.AreEqual(2, Grid.GetRow(page.label2));
		}

		[Test]
		public void SetContent([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreSame(page.label3, page.contentview0.Content);
		}

		[Test]
		public void SetImplicitContent([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreSame(page.label4, page.contentview1.Content);
		}

		[Test]
		public void SetCollectionContent([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.True(page.stack0.Children.Contains(page.label5));
			Assert.True(page.stack0.Children.Contains(page.label6));
		}

		[Test]
		public void SetImplicitCollectionContent([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.True(page.stack1.Children.Contains(page.label7));
			Assert.True(page.stack1.Children.Contains(page.label8));
		}

		[Test]
		public void SetSingleCollectionContent([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.True(page.stack2.Children.Contains(page.label9));
		}

		[Test]
		public void SetImplicitSingleCollectionContent([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.True(page.stack3.Children.Contains(page.label10));
		}

		[Test]
		public void SetPropertyDefinedOnGenericType([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual(2, page.listView.ItemsSource.Cast<object>().Count());
		}

		[Test]
		public void SetConvertibleProperties([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual(Colors.Red, page.label12.TextColor);
		}

		[Test]
		public void SetValueTypeProperties([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual(Colors.Pink, page.label13.TextColor);
		}

		[Test]
		public void CreateValueTypes([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

using AbsoluteLayout = Microsoft.Maui.Controls.Compatibility.AbsoluteLayout;

using AbsoluteLayout = Microsoft.Maui.Controls.Compatibility.AbsoluteLayout;

[TypeConverter(typeof(ConvertibleToViewTypeConverter))]
public class ConvertibleToView
{
	public static implicit operator View(ConvertibleToView source)
	{
		return new Button();
	}

	internal sealed class ConvertibleToViewTypeConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(View);
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type destinationType)
			=> value switch
			{
				ConvertibleToView convertibleValue when destinationType == typeof(View) => (View)convertibleValue,
				_ => throw new NotSupportedException(),
			};

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => false;
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) => throw new NotSupportedException();
	}
}

public class MockViewWithValues : View
{
	public char AChar { get; set; }
	public byte AByte { get; set; }
	public sbyte ASByte { get; set; }
	public Int16 AShort { get; set; }
	public UInt16 UShort { get; set; }
	public decimal ADecimal { get; set; }
	public SV_Foo Foo { get; set; }
	public string Bar { get; set; }

	public static readonly BindableProperty BPFooProperty =
		BindableProperty.Create("BPFoo", typeof(SV_Foo), typeof(MockViewWithValues), default(SV_Foo));

	public SV_Foo BPFoo
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}

	public static readonly BindableProperty BPBarProperty =
		BindableProperty.Create("BPBar", typeof(string), typeof(MockViewWithValues), default(string));

	public string BPBar
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}
}

[TypeConverter(typeof(SV_FooTypeConveter))]
public class SV_Foo
{
	public string Value { get; set; }
	public static implicit operator SV_Foo(string value)
	{
		return new SV_Foo { Value = value };
	}

	public static implicit operator string(SV_Foo foo)
	{
		return foo.Value;
	}

	internal sealed class SV_FooTypeConveter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type destinationType)
		{
			if (value is SV_Foo foo)
			{
				if (destinationType == typeof(string))
				{
					return (string)foo;
				}
			}

			throw new NotSupportedException();
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
			=> value switch
			{
				string str => (SV_Foo)str,
				_ => throw new NotSupportedException(),
			};
	}
}

public enum IntEnum
{
	Foo,
	Bar,
	Baz
}

public enum ByteEnum : byte
{
	Foo,
	Bar,
	Baz
}

public class ViewWithEnums : View
{
	public IntEnum IntEnum { get; set; }
	public ByteEnum ByteEnum { get; set; }
}

public class SetValue_ReverseConverter : IValueConverter
{
	public static ReverseConverter Instance = new ReverseConverter();

	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		var s = value as string;
		if (s == null)
			return value;
		return new string(s.Reverse().ToArray());
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		var s = value as string;
		if (s == null)
			return value;
		return new string(s.Reverse().ToArray());
	}
}

[XamlProcessing(XamlInflator.Default, true)]
public partial class SetValue : ContentPage
{
	public SetValue()
	{
		InitializeComponent();
	}

	bool fired;

	void onButtonClicked(object sender, EventArgs args)
	{
		fired = true;
	}
}
""")
					.RunMauiSourceGenerator(typeof(SetValue));
				Assert.That(result.Diagnostics, Is.Empty);
			}
			var page = new SetValue(inflator);
			Assert.AreEqual(Colors.Purple, page.Resources["purple"]);
		}

		[Test]
		public void DefCollections([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual(2, page.grid0.RowDefinitions.Count);
			Assert.AreEqual(1, page.grid0.ColumnDefinitions.Count);
		}

		[Test]
		public void FlagsAreApplied([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual(AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional, AbsoluteLayout.GetLayoutFlags(page.label14));
		}

		[Test]
		public void ConversionsAreAppliedOnSet([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.That(page.content0.Content, Is.TypeOf<Button>());
		}

		[Test]
		public void ConversionsAreAppliedOnAdd([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.That(page.stack4.Children[0], Is.TypeOf<Button>());
		}

		[Test]
		public void ListsAreSimplified([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.That(page.contentview2.Content, Is.TypeOf<Label>());
		}

		[Test]
		public void MorePrimitiveTypes([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual('!', page.mockView0.AChar);
			Assert.AreEqual((byte)2, page.mockView0.AByte);
			Assert.AreEqual((sbyte)-12, page.mockView0.ASByte);
			Assert.AreEqual((short)-22, page.mockView0.AShort);
			Assert.AreEqual((ushort)32, page.mockView0.UShort);
			Assert.AreEqual((decimal)42, page.mockView0.ADecimal);
		}

		[Test]
		public void NonIntEnums([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual(IntEnum.Foo, page.enums.IntEnum);
			Assert.AreEqual(ByteEnum.Bar, page.enums.ByteEnum);
		}

		public void SetValueWithImplicitOperatorOnSource([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual("Bar", page.implicit0.GetValue(MockViewWithValues.BPBarProperty));
		}

		[Test]
		public void SetValueWithImplicitOperatorOnTarget([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual("Foo", ((SV_Foo)page.implicit1.GetValue(MockViewWithValues.BPFooProperty)).Value);
		}

		[Test]
		public void SetWithImplicitOperatorOnSource([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual("Bar", page.implicit2.Bar);
		}

		[Test]
		public void SetWithImplicitOperatorOnTarget([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual("Foo", page.implicit3.Foo.Value);
		}

		[Test]
		public void StringValueWithUnicode([Values] XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.AreEqual("Welcome to \n .NET Multi-platform App UI", page.label16.Text);
		}

	}
}
