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

		// [TestFixture] - removed for xUnit
		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void Throw(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new BuildExceptionConstraint(8, 4), () => MockCompiler.Compile(typeof(FactoryMethodMissingMethod)));
				else
					Assert.Throws<MissingMemberException>(() => new FactoryMethodMissingMethod(useCompiledXaml));
			}
		}
	}
}
