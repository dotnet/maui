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
		}		public class Tests
		{
			[InlineData(false), InlineData(true)]
			public void InvalidSourceThrows(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					new BuildExceptionConstraint(8, 33).Validate(() => MockCompiler.Compile(typeof(ResourceDictionaryWithInvalidSource)));
				else
					new XamlParseExceptionConstraint(8, 33).Validate(() => new ResourceDictionaryWithInvalidSource(useCompiledXaml));
			}
		}
	}
}