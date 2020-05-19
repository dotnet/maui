using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;
namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class ResourceDictionaryWithInvalidSource : ContentPage
	{
		public ResourceDictionaryWithInvalidSource()
		{
			InitializeComponent();
		}

		public ResourceDictionaryWithInvalidSource(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixtureAttribute]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(false), TestCase(true)]
			public void InvalidSourceThrows(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new XamlParseExceptionConstraint(8, 33), () => MockCompiler.Compile(typeof(ResourceDictionaryWithInvalidSource)));
				else
					Assert.Throws(new XamlParseExceptionConstraint(8, 33), () => new ResourceDictionaryWithInvalidSource(useCompiledXaml));
			}
		}
	}
}