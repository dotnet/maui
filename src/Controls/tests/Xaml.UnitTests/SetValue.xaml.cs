using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
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

	public partial class SetValue : ContentPage
	{
		public SetValue()
		{
			InitializeComponent();
		}

		public SetValue(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		bool fired;
		void onButtonClicked(object sender, EventArgs args)
		{
			fired = true;
		}
		public public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
			// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetValueToBP(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal("Foo", page.label0.Text);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetBindingToBP(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal(Label.TextProperty.DefaultValue, page.label1.Text);

				page.label1.BindingContext = new { labeltext = "Foo" };
				Assert.Equal("Foo", page.label1.Text);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetBindingWithImplicitPath(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal(Label.TextProperty.DefaultValue, page.label11.Text);

				page.label11.BindingContext = new { labeltext = "Foo" };
				Assert.Equal("Foo", page.label11.Text);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetEvent(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.False(page.fired);
				(page.button0 as IButtonController).SendClicked();
				Assert.True(page.fired);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetBoolValue(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.True(page.image0.IsOpaque);
			}

			//TODO test all value conversions

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetAttachedBP(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal(1, Grid.GetColumn(page.label2));
				Assert.Equal(2, Grid.GetRow(page.label2));
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetContent(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Same(page.label3, page.contentview0.Content);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetImplicitContent(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Same(page.label4, page.contentview1.Content);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetCollectionContent(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.True(page.stack0.Children.Contains(page.label5));
				Assert.True(page.stack0.Children.Contains(page.label6));
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetImplicitCollectionContent(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.True(page.stack1.Children.Contains(page.label7));
				Assert.True(page.stack1.Children.Contains(page.label8));
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetSingleCollectionContent(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.True(page.stack2.Children.Contains(page.label9));
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetImplicitSingleCollectionContent(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.True(page.stack3.Children.Contains(page.label10));
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetPropertyDefinedOnGenericType(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal(2, page.listView.ItemsSource.Cast<object>().Count());
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetConvertibleProperties(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal(Colors.Red, page.label12.TextColor);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetValueTypeProperties(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal(Colors.Pink, page.label13.TextColor);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void CreateValueTypes(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal(Colors.Purple, page.Resources["purple"]);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void DefCollections(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal(2, page.grid0.RowDefinitions.Count);
				Assert.Single(page.grid0.ColumnDefinitions);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void FlagsAreApplied(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal(AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional, AbsoluteLayout.GetLayoutFlags(page.label14));
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void ConversionsAreAppliedOnSet(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.IsType<Button>(page.content0.Content);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void ConversionsAreAppliedOnAdd(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.IsType<Button>(page.stack4.Children[0]);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void ListsAreSimplified(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.IsType<Label>(page.contentview2.Content);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void MorePrimitiveTypes(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal('!', page.mockView0.AChar);
				Assert.Equal((byte)2, page.mockView0.AByte);
				Assert.Equal((sbyte)-12, page.mockView0.ASByte);
				Assert.Equal((short)-22, page.mockView0.AShort);
				Assert.Equal((ushort)32, page.mockView0.UShort);
				Assert.Equal((decimal)42, page.mockView0.ADecimal);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void NonIntEnums(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal(IntEnum.Foo, page.enums.IntEnum);
				Assert.Equal(ByteEnum.Bar, page.enums.ByteEnum);
			}

			public void SetValueWithImplicitOperatorOnSource(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal("Bar", page.implicit0.GetValue(MockViewWithValues.BPBarProperty));
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetValueWithImplicitOperatorOnTarget(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal("Foo", ((SV_Foo)page.implicit1.GetValue(MockViewWithValues.BPFooProperty)).Value);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetWithImplicitOperatorOnSource(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal("Bar", page.implicit2.Bar);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void SetWithImplicitOperatorOnTarget(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.Equal("Foo", page.implicit3.Foo.Value);
			}
		}
	}
}
