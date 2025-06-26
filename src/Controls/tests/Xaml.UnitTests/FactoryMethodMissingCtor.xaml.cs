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
		}
		public class Tests
		{
			[Theory]
			public void Throw([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new BuildExceptionConstraint(7, 4), () => MockCompiler.Compile(typeof(FactoryMethodMissingCtor)));
				else
					Assert.Throws<MissingMethodException>(() => new FactoryMethodMissingCtor(useCompiledXaml));
			}
		}
	}
}