using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17333 : ResourceDictionary
{
	public Maui17333() => InitializeComponent();

	public class Test
	{
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());

		[Theory]
		[Values]
		public void CompilerDoesntThrowOnOnPlatform(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				MockCompiler.Compile(typeof(Maui17333), targetFramework: "net-ios");
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui17333));
				Assert.Empty(result.Diagnostics);
			}
			else
			// TODO: Convert to [Theory(Skip="reason")] or use conditional Skip attribute
			{
				// TODO: This branch was using NUnit Assert.Skip, needs proper handling
			}

		}
	}
}