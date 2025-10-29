using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class FactoryMethodMissingMethod : MockView
	{
		public FactoryMethodMissingMethod()
		{
			InitializeComponent();
		}

		public FactoryMethodMissingMethod(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void Throw(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					new BuildExceptionConstraint(8, 4).Validate(() => MockCompiler.Compile(typeof(FactoryMethodMissingMethod)));
				else
					Assert.Throws<MissingMemberException>(() => new FactoryMethodMissingMethod(useCompiledXaml));
			}
		}
	}
}
