using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	// related to https://github.com/dotnet/maui/issues/23711
	public partial class Gh2517 : ContentPage
	{
		public Gh2517()
		{
			InitializeComponent();
		}

		public Gh2517(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true)]
			public void BindingWithInvalidPathIsNotCompiled(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Gh2517));

				var view = new Gh2517(useCompiledXaml);

				var binding = view.Label.GetContext(Label.TextProperty).Bindings.GetValue();
				Assert.IsType<Binding>(binding);
			}
		}
	}
}
