using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh4102VM
	{
		public Gh4102VM SomeNullValue { get; set; }
		public string SomeProperty { get; set; } = "Foo";
	}

	public partial class Gh4102 : ContentPage
	{
		public Gh4102() => InitializeComponent();

		public Gh4102(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[TestCase(true), TestCase(false)]
			public void CompiledBindingsNullInPath(bool useCompiledXaml)
			{
				var layout = new Gh4102(useCompiledXaml) { BindingContext = new Gh4102VM() };
				Assert.That(layout.label.Text, Is.EqualTo(null));
			}
		}
	}
}
