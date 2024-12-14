using System;
using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Converters;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[System.ComponentModel.TypeConverter(typeof(ThicknessTypeConverter))]
	public struct Bz55862Bar
	{
	}

	[XamlCompilation(XamlCompilationOptions.Skip)]
	[XamlProcessing(XamlInflator.Runtime, true)]
	public partial class Bz55862 : ContentPage
	{
		public Bz55862Bar Foo { get; set; }
		public Bz55862() => InitializeComponent();

		[TestFixture]
		class Tests
		{
			[Test] public void BindingContextWithConverter([Values(XamlInflator.Runtime,XamlInflator.XamlC,XamlInflator.SourceGen)] XamlInflator inflator)
			{
				// if (inflator == XamlInflator.XamlC)
				// 	Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Bz55862)));
				//else 
				if (inflator == XamlInflator.Runtime)
					Assert.Throws<XamlParseException>(() => new Bz55862(inflator));
				else if (inflator == XamlInflator.SourceGen)
				{
					var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Bz55862));
					Assert.That(result.Diagnostics.Any());
				}
					
			}
		}
	}
}
