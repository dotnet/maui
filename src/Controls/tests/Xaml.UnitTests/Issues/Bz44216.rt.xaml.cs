using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Build.Tasks;
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


	public class Tests
	{
		[Theory]
		[Values]
		public void DonSetValueOnPrivateBP(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				var ex = Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Bz44216)));
				// TODO: Verify exception message contains "No property," and occurs at line 7, column 26
			}
			if (inflator == XamlInflator.Runtime)
			{
				var ex = Assert.Throws<XamlParseException>(() => new Bz44216(inflator));
				// TODO: Verify exception message starts with "Cannot assign property" and occurs at line 7, column 26
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
