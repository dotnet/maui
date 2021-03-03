using System;
using NUnit.Framework;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;

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

		[TestFixture]
		public class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void ThrowsOnMismatchingType([Values(true, false)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new BuildExceptionConstraint(7, 16, m => m.Contains("No property, BindableProperty")), () => MockCompiler.Compile(typeof(TypeMismatch)));
				else
					Assert.Throws(new XamlParseExceptionConstraint(7, 16, m => m.StartsWith("Cannot assign property", StringComparison.Ordinal)), () => new TypeMismatch(useCompiledXaml));
			}
		}
	}
}