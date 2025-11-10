using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XStaticException : ContentPage
{
	public XStaticException() => InitializeComponent();


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
		//{x:Static Member=prefix:typeName.staticMemberName}
		//{x:Static prefix:typeName.staticMemberName}

		//The code entity that is referenced must be one of the following:
		// - A constant
		// - A static property
		// - A field
		// - An enumeration value
		// All other cases should throw

		[Theory]
		[Values]
		public void ThrowOnInstanceProperty(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				BuildExceptionHelper.AssertThrows(() => MockCompiler.Compile(typeof(XStaticException)), 7, 6);
			else if (inflator == XamlInflator.Runtime)
				XamlParseExceptionHelper.AssertThrows(() => new XStaticException(inflator), 7, 6);
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class XStaticException : ContentPage
{
	public XStaticException() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(XStaticException));
				Assert.True(result.Diagnostics.Any());
			}
			else
			{
				Assert.Fail($"Unknown inflator type: {inflator}");
			}

		}
	}
}