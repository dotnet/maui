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
		}
		public class Tests
		{
			[Fact]
			public void ThrowsOnMismatchingType([Values(true, false)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new BuildExceptionConstraint(7, 16, m => m.Contains("No property, BindableProperty", StringComparison.Ordinal)), () => MockCompiler.Compile(typeof(TypeMismatch)));
				else
					Assert.Throws(new XamlParseExceptionConstraint(7, 16, m => m.StartsWith("Cannot assign property", StringComparison.Ordinal)), () => new TypeMismatch(useCompiledXaml));
			}
		}
	}
}