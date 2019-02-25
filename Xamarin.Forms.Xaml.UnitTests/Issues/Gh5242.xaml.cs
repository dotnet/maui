using System;
using System.Collections.Generic;

using NUnit.Framework;

using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Gh5242VM
	{
		public int? Value {get;set;}
	}

	public partial class Gh5242 : ContentPage
	{
		public static readonly BindableProperty NullableIntProperty = BindableProperty.Create("NullableInt", typeof(int?), typeof(Gh5242), defaultValue:-1);
		public int? NullableInt { get => (int?)GetValue(NullableIntProperty); }

		public Gh5242() => InitializeComponent();
		public Gh5242(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void BindingToNullable([Values (false, true)]bool useCompiledXaml)
			{
				var layout = new Gh5242(useCompiledXaml) {BindingContext = new Gh5242VM {Value = 42}};
				Assert.That(layout.NullableInt, Is.EqualTo(42));

				layout.BindingContext = new Gh5242VM { Value = null };
				Assert.That(layout.NullableInt, Is.Null);
			}
		}
	}
}
