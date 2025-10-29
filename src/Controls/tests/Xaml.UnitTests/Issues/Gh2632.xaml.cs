using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh2632Base : ContentPage
	{
		public new Gh2632Context BindingContext
		{
			get => base.BindingContext as Gh2632Context;
			set => base.BindingContext = value;
		}

		public class Gh2632Context
		{
			public string Foo { get; set; }
		}
	}

	public partial class Gh2632 : Gh2632Base
	{
		public Gh2632()
		{
			InitializeComponent();
		}

		public Gh2632(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[InlineData(false), InlineData(true)]
			public void BindingDoesNotThrowOnRedefinedProperty(bool useCompiledXaml)
			{
				var layout = new Gh2632(useCompiledXaml);
				layout.BindingContext = new Gh2632Base.Gh2632Context { Foo = "foo" };
			}
		}
	}
}
