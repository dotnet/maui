using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	[XamlProcessing(XamlInflator.Runtime, true)]
	public partial class Gh5095 : ContentPage
	{
		public Gh5095() => InitializeComponent();

		[TestFixture]
		class Tests
		{
			[Test]
			public void ThrowsOnInvalidXaml([Values] XamlInflator inflator)
			{
				if (inflator == XamlInflator.XamlC)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh5095)));
				else if (inflator == XamlInflator.Runtime)
					Assert.Throws<XamlParseException>(() => new Gh5095(inflator));
				else if (inflator == XamlInflator.SourceGen)
				{
					var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Gh5095));
					//FIXME check the diagnostic code
					Assert.That(result.Diagnostics.Length, Is.EqualTo(1));
				}
				else if (inflator == XamlInflator.Default)
					Assert.Ignore($"no test for inflator {inflator}");
			}
		}
	}
}
