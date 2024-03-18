using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
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


		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[TestCase(true)]
			[TestCase(false)]
			public void BindingsWithSourceAreCompiled(bool useCompiledXaml)
			{
				if (useCompiledXaml)
				{
					// The XAML file contains a mismatch between the source the x:DataType attribute so the compilation of the binding will fail
					Assert.Throws(new BuildExceptionConstraint(4, 16), () => MockCompiler.Compile(typeof(Gh3606)));
				}
				else
				{
					Assert.DoesNotThrow(() => new Gh3606(useCompiledXaml: false));
				}
			}
		}
	}
}
