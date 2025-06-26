using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;
namespace Microsoft.Maui.Controls.Xaml.UnitTests
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
			[TestCase(false), TestCase(true)]
			public void InvalidSourceThrows(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new BuildExceptionConstraint(8, 33), () => MockCompiler.Compile(typeof(ResourceDictionaryWithInvalidSource)));
				else
					Assert.Throws(new XamlParseExceptionConstraint(8, 33), () => new ResourceDictionaryWithInvalidSource(useCompiledXaml));
			}
		}
	}
}