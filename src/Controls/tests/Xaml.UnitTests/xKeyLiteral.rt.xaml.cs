// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class xKeyLiteral : ContentPage
{
	public xKeyLiteral() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		//this requirement might change, see https://github.com/xamarin/Xamarin.Forms/issues/12425
		[Theory]
		[Values]
		public void xKeyRequireStringLiteral(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(xKeyLiteral)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new xKeyLiteral(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class xKeyLiteral : ContentPage
{
	public xKeyLiteral() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(xKeyLiteral));
				Assert.NotEmpty(result.Diagnostics);
			}
			else
			// TODO: Convert to [Theory(Skip="reason")] or use conditional Skip attribute
			{
				// TODO: This branch was using NUnit Assert.Skip, needs proper handling
			}

		}
	}
}
