using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Platform;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz44216Behavior : Behavior<ContentPage>
{
	static readonly BindableProperty MinLenghProperty = BindableProperty.Create("MinLengh", typeof(int), typeof(Bz44216Behavior), 1);

	public int MinLengh
	{
		get { return (int)base.GetValue(MinLenghProperty); }
		private set { base.SetValue(MinLenghProperty, value > 0 ? value : 1); }
	}
}

[XamlCompilation(XamlCompilationOptions.Skip)]
[XamlProcessing(XamlInflator.Runtime, true)] //this should be enough to disable XamlC and SG
public partial class Bz44216 : ContentPage
{
	public Bz44216()
	{	
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void DonSetValueOnPrivateBP([Values]XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(7, 26, s => s.Contains("No property,", StringComparison.Ordinal)), () => MockCompiler.Compile(typeof(Bz44216)));
			if(inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(7, 26, s => s.StartsWith("Cannot assign property", StringComparison.Ordinal)), () => new Bz44216(inflator));
			if (inflator == XamlInflator.SourceGen)
			{
				//sourcegen succeeds
				var compilation = MockSourceGenerator.CreateMauiCompilation();
				var result = MockSourceGenerator.RunMauiSourceGenerator(compilation, typeof(Bz44216));
				var diagnostics = result.Diagnostics;
				Assert.That(diagnostics.Any(d=>d.Id == "MAUIX2002"));

			}
		}
	}
}
