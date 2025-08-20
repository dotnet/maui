using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Platform;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

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
		public void DonSetValueOnPrivateBP([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(7, 26, s => s.Contains("No property,", StringComparison.Ordinal)), () => MockCompiler.Compile(typeof(Bz44216)));
			if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(7, 26, s => s.StartsWith("Cannot assign property", StringComparison.Ordinal)), () => new Bz44216(inflator));
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
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

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Bz44216 : ContentPage
{
	public Bz44216()
	{
		InitializeComponent();
	}
}
""")
					.RunMauiSourceGenerator(typeof(Bz44216));
				//sourcegen succeeds
				var diagnostics = result.Diagnostics;
				Assert.That(diagnostics.Any(d => d.Id == "MAUIX2002"));

			}
		}
	}
}
