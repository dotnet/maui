using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz24910 : ContentPage
	{
		public Bz24910()
		{
			InitializeComponent();
		}

		public Bz24910(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public void AllowNullableIntProperties(bool useCompiledXaml)
			{
				var page = new Bz24910(useCompiledXaml);
				var control = page.control0;
				Assert.Equal(1, control.NullableInt);
			}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public void AllowNullableDoubleProperties(bool useCompiledXaml)
			{
				var page = new Bz24910(useCompiledXaml);
				var control = page.control0;
				Assert.Equal(2.2d, control.NullableDouble);
			}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public void ConversionForNullable(bool useCompiledXaml)
			{
				var page = new Bz24910(useCompiledXaml);
				var control = page.control1;
				Assert.Equal(2d, control.NullableDouble);
			}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public void AllowNull(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Bz24910));

				var page = new Bz24910(useCompiledXaml);
				var control = page.control2;
				Assert.Null(control.NullableInt);
			}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public void AllowBindingToNullable(bool useCompiledXaml)
			{
				var page = new Bz24910(useCompiledXaml);
				var control = page.control3;
				Assert.Null(control.NullableInt);

				page.BindingContext = 2;
				Assert.Equal(2, control.NullableInt);
			}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public void NullableAttachedBPs(bool useCompiledXaml)
			{
				var page = new Bz24910(useCompiledXaml);
				var control = page.control4;
				Assert.Equal(3, Bz24910Control.GetAttachedNullableInt(control));
			}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public void AllowNonBindableNullable(bool useCompiledXaml)
			{
				var page = new Bz24910(useCompiledXaml);
				var control = page.control5;

				Assert.Equal(5, control.NullableIntProp);
			}
		}
	}

	public class Bz24910Control : Button
	{
		public static readonly BindableProperty NullableIntProperty =
			BindableProperty.Create(nameof(NullableInt), typeof(int?), typeof(Bz24910Control), default(int?));

		public int? NullableInt
		{
			get { return (int?)GetValue(NullableIntProperty); }
			set { SetValue(NullableIntProperty, value); }
		}

		public static readonly BindableProperty NullableDoubleProperty =
			BindableProperty.Create(nameof(NullableDouble), typeof(double?), typeof(Bz24910Control), default(double?));

		public double? NullableDouble
		{
			get { return (double?)GetValue(NullableDoubleProperty); }
			set { SetValue(NullableDoubleProperty, value); }
		}

		public static readonly BindableProperty AttachedNullableIntProperty =
			BindableProperty.CreateAttached(nameof(NullableInt), typeof(int?), typeof(Bz24910Control), default(int?));

		public static int? GetAttachedNullableInt(BindableObject bindable)
		{
			return (int?)bindable.GetValue(AttachedNullableIntProperty);
		}

		public static void SetAttachedNullableInt(BindableObject bindable, int? value)
		{
			bindable.SetValue(AttachedNullableIntProperty, value);
		}

		public int? NullableIntProp { get; set; }
	}
}