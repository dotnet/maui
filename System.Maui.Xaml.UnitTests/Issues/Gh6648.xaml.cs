using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Maui;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	public partial class Gh6648 : ContentPage
	{
		public Gh6648() => InitializeComponent();
		public Gh6648(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void DoesntFailOnNullDataType([Values(true)]bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh6648)));
			}

			[Test]
			public void BindingsOnxNullDataTypeWorks([Values(true, false)]bool useCompiledXaml)
			{
				var layout = new Gh6648(useCompiledXaml);
				layout.stack.BindingContext = new { foo = "Foo" };
				Assert.That(layout.label.Text, Is.EqualTo("Foo"));
			}
		}
	}
}
