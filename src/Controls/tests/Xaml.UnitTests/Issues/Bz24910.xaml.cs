using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz24910 : ContentPage
	{
		public Bz24910()
		{
			InitializeComponent();
		}

		[TestFixture]
		class Tests
		{
			[Test]
			public void AllowNullableIntProperties([Values] XamlInflator inflator)
			{
				var page = new Bz24910(inflator);
				var control = page.control0;
				Assert.AreEqual(1, control.NullableInt);
			}

			[Test]
			public void AllowNullableDoubleProperties([Values] XamlInflator inflator)
			{
				var page = new Bz24910(inflator);
				var control = page.control0;
				Assert.AreEqual(2.2d, control.NullableDouble);
			}

			[Test]
			public void ConversionForNullable([Values] XamlInflator inflator)
			{
				var page = new Bz24910(inflator);
				var control = page.control1;
				Assert.AreEqual(2d, control.NullableDouble);
			}

			[Test]
			public void AllowNull([Values] XamlInflator inflator)
			{
				var page = new Bz24910(inflator);
				var control = page.control2;
				Assert.Null(control.NullableInt);
			}

			[Test]
			public void AllowBindingToNullable([Values] XamlInflator inflator)
			{
				var page = new Bz24910(inflator);
				var control = page.control3;
				Assert.Null(control.NullableInt);

				page.BindingContext = 2;
				Assert.AreEqual(2, control.NullableInt);
			}

			[Test]
			public void NullableAttachedBPs([Values] XamlInflator inflator)
			{
				var page = new Bz24910(inflator);
				var control = page.control4;
				Assert.AreEqual(3, Bz24910Control.GetAttachedNullableInt(control));
			}

			[Test]
			public void AllowNonBindableNullable([Values] XamlInflator inflator)
			{
				var page = new Bz24910(inflator);
				var control = page.control5;

				Assert.AreEqual(5, control.NullableIntProp);
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
