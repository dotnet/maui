using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Platform;
using Xunit;

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

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void DonSetValueOnPrivateBP(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				XamlExceptionAssert.ThrowsBuildException(7, 26, s => s.Contains("No property,", StringComparison.Ordinal), () => MockCompiler.Compile(typeof(Bz44216)));
			else if (inflator == XamlInflator.Runtime)
				XamlExceptionAssert.ThrowsXamlParseException(7, 26, s => s.StartsWith("Cannot assign property", StringComparison.Ordinal), () => new Bz44216(inflator));
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
				Assert.True(diagnostics.Any(d => d.Id == "MAUIX2002"));

			}
		}
	}
}
