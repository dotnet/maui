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
			{
				var ex = Assert.ThrowsAny<Exception>(() => MockCompiler.Compile(typeof(Bz44216)));
				Assert.True(ex is Microsoft.Maui.Controls.Build.Tasks.BuildException);
				Assert.Contains("No property,", ex.Message, StringComparison.Ordinal);
			}
			if (inflator == XamlInflator.Runtime)
			{
				var ex = Assert.ThrowsAny<Exception>(() => new Bz44216(inflator));
				Assert.True(ex is XamlParseException);
				Assert.Contains("Cannot assign property", ex.Message, StringComparison.Ordinal);
			}
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
