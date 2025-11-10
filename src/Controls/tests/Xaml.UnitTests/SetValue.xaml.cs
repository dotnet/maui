using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;
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


	public class Tests : IDisposable
	{

		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void SetValueToBP(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal("Foo", page.label0.Text);
		}

		[Theory]
		[Values]
		public void SetBindingToBP(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal(Label.TextProperty.DefaultValue, page.label1.Text);

			page.label1.BindingContext = new { labeltext = "Foo" };
			Assert.Equal("Foo", page.label1.Text);
		}

		[Theory]
		[Values]
		public void SetBindingWithImplicitPath(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal(Label.TextProperty.DefaultValue, page.label11.Text);

			page.label11.BindingContext = new { labeltext = "Foo" };
			Assert.Equal("Foo", page.label11.Text);
		}

		[Theory]
		[Values]
		public void SetBindingWithConverter(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			page.label15.BindingContext = new { labeltext = "Foo" };
			Assert.Equal("ooF", page.label15.Text);
		}

		[Theory]
		[Values]
		public void SetEvent(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.False(page.fired);
			(page.button0 as IButtonController).SendClicked();
			Assert.True(page.fired);
		}

		[Theory]
		[Values]
		public void SetBoolValue(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.True(page.image0.IsOpaque);
		}

		//TODO test all value conversions

		[Theory]
		[Values]
		public void SetAttachedBP(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal(1, Grid.GetColumn(page.label2));
			Assert.Equal(2, Grid.GetRow(page.label2));
		}

		[Theory]
		[Values]
		public void SetContent(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Same(page.label3, page.contentview0.Content);
		}

		[Theory]
		[Values]
		public void SetImplicitContent(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Same(page.label4, page.contentview1.Content);
		}

		[Theory]
		[Values]
		public void SetCollectionContent(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.True(page.stack0.Children.Contains(page.label5));
			Assert.True(page.stack0.Children.Contains(page.label6));
		}

		[Theory]
		[Values]
		public void SetImplicitCollectionContent(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.True(page.stack1.Children.Contains(page.label7));
			Assert.True(page.stack1.Children.Contains(page.label8));
		}

		[Theory]
		[Values]
		public void SetSingleCollectionContent(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.True(page.stack2.Children.Contains(page.label9));
		}

		[Theory]
		[Values]
		public void SetImplicitSingleCollectionContent(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.True(page.stack3.Children.Contains(page.label10));
		}

		[Theory]
		[Values]
		public void SetPropertyDefinedOnGenericType(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal(2, page.listView.ItemsSource.Cast<object>().Count());
		}

		[Theory]
		[Values]
		public void SetConvertibleProperties(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal(Colors.Red, page.label12.TextColor);
		}

		[Theory]
		[Values]
		public void SetValueTypeProperties(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal(Colors.Pink, page.label13.TextColor);
		}

		[Theory]
		[Values]
		public void CreateValueTypes(XamlInflator inflator)
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
using Xunit;
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
				Assert.Empty(result.Diagnostics);
			}
			var page = new SetValue(inflator);
			Assert.Equal(Colors.Purple, page.Resources["purple"]);
		}

		[Theory]
		[Values]
		public void DefCollections(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal(2, page.grid0.RowDefinitions.Count);
			Assert.Single(page.grid0.ColumnDefinitions);
		}

		[Theory]
		[Values]
		public void FlagsAreApplied(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal(AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional, AbsoluteLayout.GetLayoutFlags(page.label14));
		}

		[Theory]
		[Values]
		public void ConversionsAreAppliedOnSet(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.IsType<Button>(page.content0.Content);
		}

		[Theory]
		[Values]
		public void ConversionsAreAppliedOnAdd(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.IsType<Button>(page.stack4.Children[0]);
		}

		[Theory]
		[Values]
		public void ListsAreSimplified(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.IsType<Label>(page.contentview2.Content);
		}

		[Theory]
		[Values]
		public void MorePrimitiveTypes(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal('!', page.mockView0.AChar);
			Assert.Equal((byte)2, page.mockView0.AByte);
			Assert.Equal((sbyte)-12, page.mockView0.ASByte);
			Assert.Equal((short)-22, page.mockView0.AShort);
			Assert.Equal((ushort)32, page.mockView0.UShort);
			Assert.Equal((decimal)42, page.mockView0.ADecimal);
		}

		[Theory]
		[Values]
		public void NonIntEnums(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal(IntEnum.Foo, page.enums.IntEnum);
			Assert.Equal(ByteEnum.Bar, page.enums.ByteEnum);
		}

		public void SetValueWithImplicitOperatorOnSource(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal("Bar", page.implicit0.GetValue(MockViewWithValues.BPBarProperty));
		}

		[Theory]
		[Values]
		public void SetValueWithImplicitOperatorOnTarget(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal("Foo", ((SV_Foo)page.implicit1.GetValue(MockViewWithValues.BPFooProperty)).Value);
		}

		[Theory]
		[Values]
		public void SetWithImplicitOperatorOnSource(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal("Bar", page.implicit2.Bar);
		}

		[Theory]
		[Values]
		public void SetWithImplicitOperatorOnTarget(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal("Foo", page.implicit3.Foo.Value);
		}

		[Theory]
		[Values]
		public void StringValueWithUnicode(XamlInflator inflator)
		{
			var page = new SetValue(inflator);
			Assert.Equal("Welcome to \n .NET Multi-platform App UI", page.label16.Text);
		}

	}
}
