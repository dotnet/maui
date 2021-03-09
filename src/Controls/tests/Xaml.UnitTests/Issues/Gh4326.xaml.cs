using System;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh4326 : ContentPage
	{
		public static string Foo = "Foo";
		internal static string Bar = "Bar";

		public Gh4326()
		{
			InitializeComponent();
		}

		public Gh4326(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(true), TestCase(false)]
			public void FindStaticInternal(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh4326)));
				var layout = new Gh4326(useCompiledXaml);

				Assert.That(layout.labelfoo.Text, Is.EqualTo("Foo"));
				Assert.That(layout.labelbar.Text, Is.EqualTo("Bar"));
				Assert.That(layout.labelinternalvisibleto.Text, Is.EqualTo(Style.StyleClassPrefix));
			}
		}
	}
}
