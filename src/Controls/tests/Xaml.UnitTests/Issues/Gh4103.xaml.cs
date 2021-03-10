using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh4103VM
	{
		public string SomeNullableValue { get; set; } = "initial";
	}

	public partial class Gh4103 : ContentPage
	{
		public Gh4103() => InitializeComponent();

		public Gh4103(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[TestCase(true), TestCase(false)]
			public void CompiledBindingsTargetNullValue(bool useCompiledXaml)
			{
				var layout = new Gh4103(useCompiledXaml) { BindingContext = new Gh4103VM() };
				Assert.That(layout.label.Text, Is.EqualTo("initial"));

				layout.BindingContext = new Gh4103VM { SomeNullableValue = null };
				Assert.That(layout.label.Text, Is.EqualTo("target null"));
			}
		}
	}
}