using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
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

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TestCase(false)]
			[TestCase(true)]
			public void Throw(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new XamlParseExceptionConstraint(8, 4), () => MockCompiler.Compile(typeof(FactoryMethodMissingMethod)));
				else
					Assert.Throws<MissingMemberException>(() => new FactoryMethodMissingMethod(useCompiledXaml));
			}
		}
	}
}
