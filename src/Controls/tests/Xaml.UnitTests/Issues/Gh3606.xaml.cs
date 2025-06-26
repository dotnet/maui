using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	// related to https://github.com/dotnet/maui/issues/23711
	public partial class Gh3606 : ContentPage
	{
		public Gh3606()
		{
			InitializeComponent();
		}

		public Gh3606(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Theory]
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void BindingsWithSourceAndInvalidPathAreNotCompiled(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Gh3606));

				var view = new Gh3606(useCompiledXaml);

				var binding = view.Label.GetContext(Label.TextProperty).Bindings.GetValue();
				Assert.That(binding, Is.TypeOf<Binding>());
			}
		}
	}
}
