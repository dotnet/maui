using System;
using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh3512 : ContentPage
{
	public Gh3512() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ThrowsOnDuplicateXKey([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh3512)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<ArgumentException>(() => new Gh3512(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Gh3512));
				Assert.AreEqual(1, result.Diagnostics.Length);
			}
			else
				Assert.Ignore($"Test not supported for {inflator}");
		}
	}
}
