using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class TypeMismatch : ContentPage
	{
		public TypeMismatch() => InitializeComponent();
		public TypeMismatch(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[Theory]
			public void Method(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					new BuildExceptionConstraint(7, 16, m => m.Contains("No property, BindableProperty", StringComparison.Ordinal)).Validate(() => MockCompiler.Compile(typeof(TypeMismatch)));
				else
					new XamlParseExceptionConstraint(7, 16, m => m.StartsWith("Cannot assign property", StringComparison.Ordinal)).Validate(() => new TypeMismatch(useCompiledXaml));
			}
		}
	}
}