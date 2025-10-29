using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class FactoryMethodMissingCtor : MockView
	{
		public FactoryMethodMissingCtor() => InitializeComponent();
		public FactoryMethodMissingCtor(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[Theory]
			public void Method(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					new BuildExceptionConstraint(7, 4).Validate(() => MockCompiler.Compile(typeof(FactoryMethodMissingCtor)));
				else
					Assert.Throws<MissingMethodException>(() => new FactoryMethodMissingCtor(useCompiledXaml));
			}
		}
	}
}