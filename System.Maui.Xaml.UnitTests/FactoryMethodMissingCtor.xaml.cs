using System;
using NUnit.Framework;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class FactoryMethodMissingCtor : MockView
	{
		public FactoryMethodMissingCtor() => InitializeComponent();
		public FactoryMethodMissingCtor(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void Throw([Values(false, true)]bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new XamlParseExceptionConstraint(7, 4), () => MockCompiler.Compile(typeof(FactoryMethodMissingCtor)));
				else
					Assert.Throws<MissingMethodException>(() => new FactoryMethodMissingCtor(useCompiledXaml));
			}
		}
	}
}